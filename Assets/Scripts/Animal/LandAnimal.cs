using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEditorInternal;
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(CharacterController))]
public class LandAnimal : MonoBehaviour
{
    public enum ELandingAnimalState
    {
        Idle,
        Wander,
        Following,
        Chasing,
        RunAway,
        Attacking,
        Sleeping,
        Sitting,
        Dead,
    }

    public enum ELandingAnimalAnimationState
    {
        Idle,
        Walking,
        Running,
        Attacking,
        Sleeping,
        Sitting,
        Dead,
    }

    public enum ELandingAnimalSoundState
    {
        Idle,
        Walking,
        Running,
        Attacking,
        Sleeping,
        Sitting,
        Dead,
    }

    [System.Serializable]
    public struct SLandingAnimalAnimation
    {
        public ELandingAnimalAnimationState state;
        public string transitionName;
    }

    [System.Serializable]
    public struct SLandingAnimalSound
    {
        public ELandingAnimalSoundState state;
        public AudioClip clip;
        public float minSoundIntervalInSeconds;
        public float maxXoundIntervalInSeconds;
        public float soundInterval;
    }

    #region 변수 선언
    [Header("--- Animal Animation and Sound Settings ---")]
    public SLandingAnimalAnimation[] animations;
    public SLandingAnimalSound[] sounds;
    public Dictionary<ELandingAnimalAnimationState, SLandingAnimalAnimation> landingAnimalAnimationDictionary = new Dictionary<ELandingAnimalAnimationState, SLandingAnimalAnimation>();
    public Dictionary<ELandingAnimalSoundState, SLandingAnimalSound> landingAnimalSoundDictionary = new Dictionary<ELandingAnimalSoundState, SLandingAnimalSound>();

    [Header("--- Animal Movement Settings ---")]
    [SerializeField] float walkingSpeed;
    [SerializeField] float runningSpeed;
    [SerializeField] float turnSpeed;
    [SerializeField] float minWanderChangeDirectionInSeconds;
    [SerializeField] float maxWanderChangeDirectionInSeconds;
    [SerializeField] float gravity;
    [SerializeField] bool avoidWater;
    [SerializeField] float seaLevel;
    [SerializeField] bool returnToBase;
    [SerializeField] float maxDistanceFromBase;

    [Header("--- Animal State Transition Settings (probalbity in seconds) ---")]
    [SerializeField] [Range(0, 1)] float IdleToWander;
    [SerializeField] [Range(0, 1)] float wanderToSleeping;
    [SerializeField] [Range(0, 1)] float wanderToSitting;
    [SerializeField] [Range(0, 1)] float wanderToIdle;
    [SerializeField] [Range(0, 1)] float sittingToSleeping;
    [SerializeField] [Range(0, 1)] float sittingToWander;
    [SerializeField] [Range(0, 1)] float sleepingToAwake;

    [Header("--- Animal Perception Settings ---")]
    [SerializeField] float perceptionRadius;
    [SerializeField] float actingRadius;
    [SerializeField] GameObject[] followingTargets;
    [SerializeField] GameObject[] enemyTargets;
    [SerializeField] GameObject[] runAwayTargets;

    [Header("--- Animal Stamina Settings ---")]

    [SerializeField] float health;
    [SerializeField] float attackPower;

    [Header("--- System Settings ---")]
    [SerializeField] AudioSource staticAudio;
    [SerializeField] AudioSource dynamicAudio;

    //private variables
    Animator animator;
    [SerializeField] ELandingAnimalState state;
    CharacterController controller;
    SphereCollider perceptionCollider;
    GameObject currentEnemy;
    GameObject currentFollower;
    GameObject currentRunAwayTarget;

    Vector3 velocity;
    Vector2 wanderDirection;


    #endregion 
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        perceptionCollider = transform.gameObject.AddComponent<SphereCollider>();
        perceptionCollider.isTrigger = true;
        perceptionCollider.center = transform.position;
        perceptionCollider.radius = perceptionRadius;

        for (int i = 0; i < animations.Length; i++)
        {
            landingAnimalAnimationDictionary.Add(animations[i].state, animations[i]);
        }

        for (int i = 0; i < sounds.Length; i++)
        {
            landingAnimalSoundDictionary.Add(sounds[i].state, sounds[i]);
        }

        StartCoroutine(IERandomDirection());
        state = ELandingAnimalState.Wander;
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case ELandingAnimalState.Idle:
                FixedUpdateIdle();
                break;
            case ELandingAnimalState.Wander:
                FixedUpdateWander();
                break;
            case ELandingAnimalState.Following:
                FixedUpdateFollowing();
                break;
            case ELandingAnimalState.Attacking:
                FixedUpdateAttacking();
                break;
            case ELandingAnimalState.Sleeping:
                FixedUpdateSleeping();
                break;
            case ELandingAnimalState.Sitting:
                FixedUpdateSitting();
                break;
            case ELandingAnimalState.Dead:
                FixedUpdateDead();
                break;
        }

        UpdatePerceptedObjects();
    }

    #region fixedUpdates
    void FixedUpdateIdle()
    {
        //에러 처리 루틴이 필요할 경우 혹은 아무 것도 안하고 서 있는 경우
        SetAnimator(ELandingAnimalAnimationState.Idle, true);
        PlaySound(ELandingAnimalSoundState.Idle);

        //어느정도 시간이 지나면
        if(CalculateChance(IdleToWander))
            state = ELandingAnimalState.Wander;

        //idle 하다가 following 할려는 대상을 발견하면
        if (followingTargets != null)
            state = ELandingAnimalState.Following;
    }

    void FixedUpdateWander()
    {
        UpdateWalking(wanderDirection);

        //갑자기 서고 싶으면
        if (CalculateChance(wanderToIdle))
            state = ELandingAnimalState.Idle;

        //갑자기 앉고 싶으면
        if (CalculateChance(wanderToSitting))
            state = ELandingAnimalState.Sitting;
        //갑자기 자고 싶으면
        if (CalculateChance(wanderToSleeping))
            state = ELandingAnimalState.Sleeping;

        //wander 하다가 following 할려는 대상을 발견하면
        if (currentFollower != null)
            state = ELandingAnimalState.Following;
        //wander 하다가 적을 발견하면 -> 구현 안함

        //wander 하다가 도망쳐야 하는 대상을 발견하면 -> 구현 안함

    }

    void FixedUpdateFollowing()
    {
        //following 하다가 대상이 너무 멀어지면 / 조건이 일치하지 않으면 즉 currentFollower 가 null 이거나 조건에 안 맞으면
        if (currentFollower == null)
            state = ELandingAnimalState.Wander;
        else
        {
            //following 하다가 대상과 충분히 가까워 지면 -> sitting 으로 전환
            if (Vector3.Distance(currentFollower.transform.position, transform.position) < actingRadius)
                state = ELandingAnimalState.Sitting;

            UpdateWalking((new Vector2(currentFollower.transform.position.x, currentFollower.transform.position.z) - new Vector2(transform.position.x, transform.position.z)).normalized);
        }
    }

    void FixedUpdateAttacking()
    {
        SetAnimator(ELandingAnimalAnimationState.Attacking, true);
        PlaySound(ELandingAnimalSoundState.Attacking);
    }

    void FixedUpdateSitting()
    {
        SetAnimator(ELandingAnimalAnimationState.Sitting, true);
        PlaySound(ELandingAnimalSoundState.Sitting);

        //앉아 있다 following 해야 하는 대상이 생기면
        if (currentFollower != null)
            state = ELandingAnimalState.Following;
        //앉아 있다 chasing 해야 하는 대상이 생기면
        if (currentEnemy != null)
            state = ELandingAnimalState.Chasing;
        //일정한 시간이 지나면
        if (CalculateChance(sittingToWander))
            state = ELandingAnimalState.Wander;
        //위협을 느끼면
        

    }

    void FixedUpdateSleeping()
    {
        SetAnimator(ELandingAnimalAnimationState.Sleeping, true);
        PlaySound(ELandingAnimalSoundState.Sleeping);

        //일정한 시간이 지나면
        if (CalculateChance(sleepingToAwake))
            state = ELandingAnimalState.Idle;
    }

    void FixedUpdateDead()
    {
        //천천히 사라진다.
    }
    #endregion

    #region collider 충돌 체크 및 perception 체크
    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < followingTargets.Length; i++)
        {
            if (followingTargets[i] == other.gameObject)
            {
                currentFollower = other.gameObject;
                return;
            }
        }

        for (int i = 0; i < enemyTargets.Length; i++)
        {
            if (followingTargets[i] == other.gameObject)
            {
                currentEnemy = other.gameObject;
                return;
            }
        }

        for (int i = 0; i<runAwayTargets.Length;i++)
        {
            if(runAwayTargets[i] == other.gameObject)
            {
                currentRunAwayTarget = other.gameObject;
                return;
            }
        }
    }

    private void UpdatePerceptedObjects()
    {
        if(currentEnemy != null)
        {
            if (Vector3.Distance(currentEnemy.transform.position, transform.position) > perceptionRadius + actingRadius)
                currentEnemy = null;
        }
        if(currentFollower != null)
        {
            if (Vector3.Distance(currentFollower.transform.position, transform.position) > perceptionRadius + actingRadius)
                currentFollower = null;
        }
        if(currentRunAwayTarget != null)
        {
            if (Vector3.Distance(currentRunAwayTarget.transform.position, transform.position) > perceptionRadius + actingRadius)
                currentRunAwayTarget = null;
        }
    }
    #endregion

    #region 유용한 함수들
    void UpdateWalking(Vector2 direction)
    {
        SetAnimator(ELandingAnimalAnimationState.Walking, true);
        PlaySound(ELandingAnimalSoundState.Walking);
        PlaySound(ELandingAnimalSoundState.Idle);

        UpdateVelocityAndMove(direction, walkingSpeed);
    }

    void UpdateRunning(Vector2 direction)
    {
        SetAnimator(ELandingAnimalAnimationState.Running, true);
        PlaySound(ELandingAnimalSoundState.Running);
        PlaySound(ELandingAnimalSoundState.Idle);

        UpdateVelocityAndMove(direction, runningSpeed);
    }

    void UpdateVelocityAndMove(Vector2 targetDirection, float speed)
    {
        Vector3 desireMove = transform.forward;
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo);
        desireMove = Vector3.ProjectOnPlane(desireMove, hitInfo.normal).normalized;

        velocity.x = desireMove.x * speed;
        velocity.z = desireMove.z * speed;
        velocity.y -= gravity;

        if (IsWater())
        {
            wanderDirection = -wanderDirection;
        }

        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        float rotateAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(targetDirection.x, 0, targetDirection.y), Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, surfaceRotation * Quaternion.AngleAxis(rotateAngle, Vector3.up), turnSpeed);

        controller.Move(velocity * Time.fixedDeltaTime);
    }
    void SetAnimator(ELandingAnimalAnimationState state, bool b)
    {
        if (landingAnimalAnimationDictionary.ContainsKey(state) && animator.GetBool(landingAnimalAnimationDictionary[state].transitionName) == false)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                animator.SetBool(animations[i].transitionName, false);
            }
            animator.SetBool(landingAnimalAnimationDictionary[state].transitionName, b);
        }
    }

    // 그냥 사용하면 계속 재생되며 (static source 를 통해 재생), min max를 주면 랜덤하게 variation 을 주면서 재생된다. (dynamic source 를 통해 재생)
    void PlaySound(ELandingAnimalSoundState state)
    {
        if (landingAnimalSoundDictionary.ContainsKey(state) && landingAnimalSoundDictionary[state].maxXoundIntervalInSeconds == 0)
        {
            if (staticAudio.clip != landingAnimalSoundDictionary[state].clip)
            {
                staticAudio.clip = landingAnimalSoundDictionary[state].clip;
                StartCoroutine(IEPlaySound(landingAnimalSoundDictionary[state].clip, landingAnimalSoundDictionary[state].soundInterval));
            }
        }
        else
        {
            if (landingAnimalSoundDictionary.ContainsKey(state) && dynamicAudio.clip != landingAnimalSoundDictionary[state].clip)
            {
                dynamicAudio.clip = landingAnimalSoundDictionary[state].clip;
                StartCoroutine(IEPlaySoundRandomly(
                    landingAnimalSoundDictionary[state].clip,
                    landingAnimalSoundDictionary[state].minSoundIntervalInSeconds,
                    landingAnimalSoundDictionary[state].maxXoundIntervalInSeconds)
                );
            }
        }

    }

    void StopAllSound()
    {
        staticAudio.Stop();
        dynamicAudio.Stop();
    }

    IEnumerator IEPlaySoundRandomly(AudioClip clip, float randomStart = 0, float randomEnd = 0)
    {
        while (true)
        {
            if (clip == dynamicAudio.clip)
            {
                dynamicAudio.Play();
            }
            else
            {
                yield break;
            }

            yield return new WaitForSeconds(Random.Range(randomStart, randomEnd) + dynamicAudio.clip.length);
        }
    }

    IEnumerator IEPlaySound(AudioClip clip, float soundInterval)
    {
        if (soundInterval == 0)
        {
            staticAudio.loop = true;
        }
        else
        {
            staticAudio.loop = false;
        }

        while (true)
        {
            if (clip == staticAudio.clip)
            {
                staticAudio.Play();
            }
            else
            {
                yield break;
            }

            if (staticAudio.loop == false)
            {
                yield return new WaitForSeconds(staticAudio.clip.length + soundInterval + float.Epsilon);
            }
            else
            {
                yield break;
            }

        }
    }

    bool CalculateChance(float chance)
    {
        if(Random.Range(0,1f) < chance * Time.fixedDeltaTime)
        {
            return true;
        } else
        {
            return false;
        }
    }

    IEnumerator IERandomDirection()
    {
        while (true)
        {
            wanderDirection = RandomDirection();
            yield return new WaitForSeconds(Random.Range(minWanderChangeDirectionInSeconds, maxWanderChangeDirectionInSeconds));
        }
    }

    Vector3 RandomDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    bool IsWater()
    {
        if (transform.position.y < seaLevel)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(transform.position, Vector3.down * Mathf.Infinity);
        //Gizmos.DrawWireSphere(transform.position, perceptionRadius);
        //Gizmos.DrawWireSphere(transform.position, actingRadius);
        Gizmos.DrawRay(transform.position, wanderDirection);
    }
    #endregion
}
