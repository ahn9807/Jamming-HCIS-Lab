using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class FlyingAnimal : MonoBehaviour
{
    public enum EFlyingAnimalState
    {
        Idle,
        Chase,
        Wander,
        Following,
        Chasing,
        Stand,
        Landing,
    }

    public enum EFlyingAnimalAnimationState
    {
        Flight_straight,
        Flight_turn_left,
        Flight_turn_right,
        Glide_straight,
        Glide_turn_left,
        Glide_turn_right,
        Hop,
        Hover_flight,
        Idle_1,
        Idle_2,
        Parrot_animation,
        Take_off,
        Touch_down,
    }

    public enum EFlyingAnimalSoundState
    {
        Idle,
        Wander,
        Chase,
        Following,
        Fly,
        Stand,
        Landing,
    }

    [System.Serializable]
    public struct SFlyingAnimalAnimation
    {
        public EFlyingAnimalAnimationState flyingAnimalAnimationState;
        public string transitionName;
    }

    [System.Serializable]
    public struct SFlyingAnimalSound
    {
        public EFlyingAnimalSoundState flyingAnimalSoundState;
        public AudioClip clip;
        public float minSoundIntervalInSeconds;
        public float maxSoundIntervalInSeconds;
        public float soundInterval;
    }

    #region 변수 선언
    [Header("--- Animal Animation and Sound Settings ---")]
    public SFlyingAnimalAnimation[] animations;
    public SFlyingAnimalSound[] sounds;
    public Dictionary<EFlyingAnimalAnimationState, SFlyingAnimalAnimation> flyingAnimalAnimationDictionary;
    public Dictionary<EFlyingAnimalSoundState, SFlyingAnimalSound> flyingAnimalSoundDictionary;

    [Header("--- Animal Movement Settings ---")]
    [SerializeField] float minFlySpeed;
    [SerializeField] float maxFlySpeed;
    [SerializeField] float minAltitude;
    [SerializeField] float maxAltitude;
    [SerializeField] float turnSpeed;
    [SerializeField] float maxDistanceFromBase;
    [SerializeField] float minDistanceFromBase;
    [SerializeField] bool returnToBase;
    [SerializeField] string groundLayerName;
    [SerializeField] float seaLevel;
    [SerializeField] float minWanderChangeInterval;
    [SerializeField] float maxWanderChangeInterval;

    [Header("--- Animal State Transition Settings (probalbity in seconds) ---")]
    [SerializeField] [Range(0, 1)] float sampleValue;

    [Header("--- Animal Perception Settings ---")]
    [SerializeField] float perceptionRadius;
    [SerializeField] [Range(0, 1)] float variation;
    [SerializeField] float fromRadius;
    [SerializeField] float toRadius;

    [Header("--- System Settings ---")]
    [SerializeField] AudioSource staticAudio;
    [SerializeField] AudioSource dynamicAudio;
    public GameObject currentFollower;

    Animator birdAnimator;
    [SerializeField] EFlyingAnimalState flyingAnimalState;
    CharacterController controller;
    SphereCollider perceptionCollider;

    Vector3 velocity;
    Vector3 direction;
    Vector3 wanderDirection;
    Vector3 basePosition;
    float prevz;

    #endregion

    #region Unity Functions
    // Start is called before the first frame update
    void Start()
    {
        prevz = 0;
        flyingAnimalState = EFlyingAnimalState.Idle;
        birdAnimator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        flyingAnimalAnimationDictionary = new Dictionary<EFlyingAnimalAnimationState, SFlyingAnimalAnimation>();
        flyingAnimalSoundDictionary = new Dictionary<EFlyingAnimalSoundState, SFlyingAnimalSound>();

        perceptionCollider = transform.gameObject.AddComponent<SphereCollider>();
        perceptionCollider.isTrigger = true;
        perceptionCollider.center = transform.position;
        perceptionCollider.radius = perceptionRadius;

        for (int i = 0; i < animations.Length; i++)
        {
            flyingAnimalAnimationDictionary.Add(animations[i].flyingAnimalAnimationState, animations[i]);
        }
        for(int i=0;i<sounds.Length;i++)
        {
            flyingAnimalSoundDictionary.Add(sounds[i].flyingAnimalSoundState, sounds[i]);
        }

        StartCoroutine(IERandomDirection());
    }

    void FixedUpdate()
    {
        switch (flyingAnimalState)
        {
            case EFlyingAnimalState.Idle:
                FixedUpdateIdle();
                break;
            case EFlyingAnimalState.Chase:
                FixedUpdateChase();
                break;
            case EFlyingAnimalState.Following:
                FixedUpdateFollowing();
                break;
            case EFlyingAnimalState.Wander:
                FixedUpdateWander();
                break;
            case EFlyingAnimalState.Stand:
                FixedUpdateStand();
                break;
            case EFlyingAnimalState.Landing:
                FixedUpdateLanding();
                break;
        }
    }
    #endregion

    #region Update Methods
    void FixedUpdateIdle()
    {

    }

    void FixedUpdateStand()
    {

    }

    void FixedUpdateChase()
    {

    }

    void FixedUpdateFollowing()
    {
        Vector3 nextDirection;
        //만약 following 하는 대상이 인지하지 못할 정도로 멀어지면 / 조건이 일치하지 않으면 즉 null이면
        if (currentFollower == null)
        {
            flyingAnimalState = EFlyingAnimalState.Wander;
            return;
        } else
        {
            float distanceFromTarget = Vector3.Distance(currentFollower.transform.position, transform.position);

            //팔로잉 하는 대상과 너무 가까워 지면 즉 fromRadius 보다 가까워지면
            if(distanceFromTarget < fromRadius)
            {
                Debug.Log("1");
                nextDirection = transform.position - currentFollower.transform.position + variation * wanderDirection;
            }
            //팔로잉 하는 대상과 너무 멀어지면 즉 toRadius 보다 멀어지면 
            else if(distanceFromTarget < toRadius && distanceFromTarget > fromRadius)
            {
                Debug.Log("2");
                nextDirection = currentFollower.transform.position - transform.position + variation * wanderDirection;
            }
            //팔로잉 하려는 대상과 따라가야 하는 거리에 있으면
            else
            {
                Debug.Log("following");
                nextDirection = currentFollower.transform.position - transform.position;
            }
        }

        //고도 처리
        if (CalculateAmplitude() < minAltitude)
        {
            nextDirection.y = 0.5f;
            //need to climb faster so we set 1 cause if return value of RandomDirection is btwn 0 and 1
        }
        else if (CalculateAmplitude() > maxAltitude)
        {
            nextDirection.y = -0.5f;
        }

        UpdateVelocityAndAnimation(nextDirection, maxFlySpeed);
        controller.Move(velocity * Time.fixedDeltaTime);
    }

    void FixedUpdateLanding()
    {

    }

    void FixedUpdateWander()
    {
        float distanceFromBase = Vector3.Distance(basePosition, transform.position);

        if (returnToBase && maxDistanceFromBase < distanceFromBase)
        {
            direction = basePosition - transform.position;
        }

        if (CalculateAmplitude() < minAltitude)
        {
            wanderDirection.y = 0.5f;
            //need to climb faster so we set 1 cause if return value of RandomDirection is btwn 0 and 1
        }
        else if (CalculateAmplitude() > maxAltitude)
        {
            wanderDirection.y = -0.5f;
        }


        Debug.Log(CalculateAmplitude());
        UpdateVelocityAndAnimation(wanderDirection, minFlySpeed);
        controller.Move(velocity * Time.fixedDeltaTime);
    }

    //날아다니는 새의 회전축 기울임과, 속력 그리고 애니매이션을 업데이트한다. 
    void UpdateVelocityAndAnimation(Vector3 rotation, float speed)
    {
        //새가 회전하는 방향으로 새의 중심축을 이동한다.
        float temp = prevz;
        float zturn = Mathf.Clamp(Vector3.SignedAngle(rotation, direction, Vector3.up), -45f, 45f);
        if (prevz < zturn)
        {
            prevz += Mathf.Min(turnSpeed * Time.fixedDeltaTime, zturn - prevz);
        }
        else if (prevz >= zturn)
        {
            prevz -= Mathf.Min(turnSpeed * Time.fixedDeltaTime, prevz - zturn);
        }
        prevz = Mathf.Clamp(prevz, -45f, 45f);

        //새가 바라보는 방향으로 로테이션을 설정한다.
        Quaternion lookRotation = Quaternion.LookRotation(rotation, Vector3.up);

        //천천히 새가 바라보는 방향으로 고개를 튼다. 또한 전에 설정한 prevz 를 적용시킨다.
        transform.eulerAngles = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
        transform.Rotate(0f, 0f, prevz - temp, Space.Self);

        //direction 을 설정하고, 계산된 속도를 바탕으로 속력을 구한다. 이 속력은 Move 를 통해 업데이트 된다. 
        direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
        velocity = speed * direction;

        //애니매이션을 업데이트 하는 구간
        if (transform.rotation.eulerAngles.y > 0)
        {
            //turn turn right
            if (zturn < 0)
            {
                SetAnimator(EFlyingAnimalAnimationState.Flight_turn_right, true);
            }
            else if (zturn > 0)
            {
                SetAnimator(EFlyingAnimalAnimationState.Flight_turn_left, true);
            }
            else
            {
                SetAnimator(EFlyingAnimalAnimationState.Flight_straight, true);
            }
        }
        else if (transform.rotation.eulerAngles.y <= 0)
        {
            //turn turn right
            if (zturn < 0)
            {
                SetAnimator(EFlyingAnimalAnimationState.Glide_turn_right, true);
            }
            else if (zturn > 0)
            {
                SetAnimator(EFlyingAnimalAnimationState.Glide_turn_left, true);
            }
            else
            {
                SetAnimator(EFlyingAnimalAnimationState.Glide_straight, true);
            }
        }
    }
    #endregion


    #region Extra functions
    void SetAnimator(EFlyingAnimalAnimationState state, bool b)
    {
        if (flyingAnimalAnimationDictionary.ContainsKey(state) && birdAnimator.GetBool(flyingAnimalAnimationDictionary[state].transitionName) == false)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                birdAnimator.SetBool(animations[i].transitionName, false);
            }
            birdAnimator.SetBool(flyingAnimalAnimationDictionary[state].transitionName, b);
        }
    }

    //Scripts for usefull things
    float CalculateAmplitude()
    {
        int layerMask = 1 << LayerMask.NameToLayer(groundLayerName);

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue, layerMask);

        if(hitInfo.point.y < seaLevel)
        {
            return transform.position.y - seaLevel;
        }

        return hitInfo.distance;
    }

    Vector3 GetLandingPosition(Vector3 point)
    {
        Physics.Raycast(point, Vector3.down, out RaycastHit hitInfo, float.MaxValue);

        Debug.DrawLine(point, hitInfo.point);

        return hitInfo.point;
    }

    float CalculateChangeBySecond(float chance)
    {
        return (1f / chance) * Random.Range(0, 2) * 1f / Time.fixedDeltaTime;
    }

    IEnumerator IERandomDirection()
    {
        while(true)
        {
            wanderDirection = new Vector3(Random.Range(-1, 1f), Random.Range(-0.5f, 0.5f), Random.Range(-1, 1f));

            yield return new WaitForSeconds(Random.Range(minWanderChangeInterval, maxWanderChangeInterval));
        }

    }

    void PlaySound(EFlyingAnimalSoundState state)
    {
        if (flyingAnimalSoundDictionary.ContainsKey(state) && flyingAnimalSoundDictionary[state].maxSoundIntervalInSeconds == 0)
        {
            if (staticAudio.clip != flyingAnimalSoundDictionary[state].clip)
            {
                staticAudio.clip = flyingAnimalSoundDictionary[state].clip;
                StartCoroutine(IEPlaySound(flyingAnimalSoundDictionary[state].clip, flyingAnimalSoundDictionary[state].soundInterval));
            }
        }
        else
        {
            if (flyingAnimalSoundDictionary.ContainsKey(state) && dynamicAudio.clip != flyingAnimalSoundDictionary[state].clip)
            {
                dynamicAudio.clip = flyingAnimalSoundDictionary[state].clip;
                StartCoroutine(IEPlaySoundRandomly(
                    flyingAnimalSoundDictionary[state].clip,
                    flyingAnimalSoundDictionary[state].minSoundIntervalInSeconds,
                    flyingAnimalSoundDictionary[state].maxSoundIntervalInSeconds)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, fromRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, toRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, perceptionRadius);
    }
    #endregion
}