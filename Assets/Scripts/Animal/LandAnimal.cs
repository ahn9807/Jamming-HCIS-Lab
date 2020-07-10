using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using Valve.VR;

[RequireComponent(typeof(CharacterController))]
public class LandAnimal : MonoBehaviour
{
    public enum ELandingAnimalState
    {
        Idle,
        Walking,
        Attacking,
        Sleeping,
        Sitting,
        Running,
        Dead,
    }

    [System.Serializable]
    public struct SLandingAnimalAnimation
    {
        public ELandingAnimalState state;
        public string transitionName;
    }

    [System.Serializable]
    public struct SLandingAnimalSound
    {
        public ELandingAnimalState state;
        public AudioClip clip;
    }

    #region 변수 선언
    public SLandingAnimalAnimation[] animations;
    public SLandingAnimalSound[] sounds;
    public Dictionary<ELandingAnimalState, string> landingAnimalAnimationDictionary = new Dictionary<ELandingAnimalState, string>();
    public Dictionary<ELandingAnimalState, AudioClip> landingAnimalSoundDictionary = new Dictionary<ELandingAnimalState, AudioClip>();


    [SerializeField] float walkingSpeed;
    [SerializeField] float runningSpeed;
    [SerializeField] float turnSpeed;
    [SerializeField] float gravity;
    [SerializeField] float maxHeadingChange;
    [SerializeField] float maxDistanceFromBase;
    [SerializeField] bool returnToBase;
    [SerializeField] string groundLayer;
    [SerializeField] string waterLayer;

    [SerializeField] float health;
    [SerializeField] float attackPower;

    [Range(0, 1)] [SerializeField] float chanceToWalking;
    [Range(0, 1)] [SerializeField] float chanceToRunning;
    [Range(0, 1)] [SerializeField] float chanceToSleeping;
    [Range(0, 1)] [SerializeField] float chanceToSitting;
    [Range(0, 1)] [SerializeField] float chanceToAttacking;
    [Range(0, 1)] [SerializeField] float chanceToIdling;
    [SerializeField] float minWanderChangeDirectionInSeconds;
    [SerializeField] float maxWanderChangeDirectionInSeconds;
    [SerializeField] float minSoundIntervalInSeconds;
    [SerializeField] float maxXoundIntervalInSeconds;

    [SerializeField] float distantToGround;
    [SerializeField] float groundCheckRadius;

    [SerializeField] ArduinoInteraction arduinoInteraction;

    Animator animator;
    [SerializeField] ELandingAnimalState state;
    CharacterController controller;
    [SerializeField] AudioSource staticAudio;
    [SerializeField] AudioSource dynamicAudio;
    float distanceFromBase;

    Vector3 velocity;
    Vector2 wanderDirection;


    #endregion 
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        for (int i = 0; i < animations.Length; i++)
        {
            landingAnimalAnimationDictionary.Add(animations[i].state, animations[i].transitionName);
        }

        for (int i = 0; i < sounds.Length; i++)
        {
            landingAnimalSoundDictionary.Add(sounds[i].state, sounds[i].clip);
        }

        StartCoroutine(IERandomDirection());
    }

    private void FixedUpdate()
    {
        switch(state)
        {
            case ELandingAnimalState.Idle:
                FixedUpdateIdle();
                break;
            case ELandingAnimalState.Walking:
                FixedUpdateWalking();
                break;
            case ELandingAnimalState.Running:
                FixedUpdateRunning();
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
    }

    #region fixedUpdates
    void FixedUpdateIdle()
    {
        SetAnimator(ELandingAnimalState.Idle, true);
    }

    void FixedUpdateWalking()
    {
        SetAnimator(ELandingAnimalState.Walking, true);
        PlaySound(ELandingAnimalState.Walking);
        PlaySound(ELandingAnimalState.Idle, minWanderChangeDirectionInSeconds, maxWanderChangeDirectionInSeconds);

        UpdateVelocityAndMove(wanderDirection, walkingSpeed);
    }

    void FixedUpdateRunning()
    {
        SetAnimator(ELandingAnimalState.Running, true);
        PlaySound(ELandingAnimalState.Running);

        UpdateVelocityAndMove(wanderDirection, runningSpeed);
    }

    void FixedUpdateAttacking()
    {
        SetAnimator(ELandingAnimalState.Attacking, true);
        PlaySound(ELandingAnimalState.Attacking);
    }

    void FixedUpdateSitting()
    {
        SetAnimator(ELandingAnimalState.Sitting, true);
        PlaySound(ELandingAnimalState.Sitting);

    }

    void FixedUpdateSleeping()
    {
        SetAnimator(ELandingAnimalState.Sleeping, true);
        PlaySound(ELandingAnimalState.Sleeping);
     
    }

    void FixedUpdateDead()
    {

    }
    #endregion

    #region 유용한 함수들
    void UpdateVelocityAndMove(Vector2 targetDirection, float speed)
    {
        Vector3 desireMove = transform.forward;
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo);
        desireMove = Vector3.ProjectOnPlane(desireMove, hitInfo.normal).normalized;

        velocity.x = desireMove.x * speed;
        velocity.z = desireMove.z * speed;
        velocity.y -= gravity;

        Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        float rotateAngle = Vector2.SignedAngle(Vector2.up, targetDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, surfaceRotation * Quaternion.AngleAxis(rotateAngle, Vector3.up), turnSpeed * Time.fixedDeltaTime);

        controller.Move(velocity * Time.fixedDeltaTime);
    }
    void SetAnimator(ELandingAnimalState state, bool b)
    {
        if(landingAnimalAnimationDictionary.ContainsKey(state) && animator.GetBool(landingAnimalAnimationDictionary[state]) == false)
        {
            for(int i=0;i<animations.Length; i++)
            {
                animator.SetBool(animations[i].transitionName, false);
            }
            animator.SetBool(landingAnimalAnimationDictionary[state], b);
        }
    }

    // 그냥 사용하면 계속 재생되며 (static source 를 통해 재생), min max를 주면 랜덤하게 variation 을 주면서 재생된다. (dynamic source 를 통해 재생)
    void PlaySound(ELandingAnimalState state, float randomRangeMin = 0, float randomRangeMax = 0)
    {
        if (randomRangeMax == 0)
        {
            if (landingAnimalSoundDictionary.ContainsKey(state) && staticAudio.clip != landingAnimalSoundDictionary[state])
            {
                staticAudio.clip = landingAnimalSoundDictionary[state];
                StartCoroutine(IEPlaySound(landingAnimalSoundDictionary[state], 0));
            }
        } else
        {
            if (landingAnimalSoundDictionary.ContainsKey(state) && dynamicAudio.clip != landingAnimalSoundDictionary[state])
            {
                dynamicAudio.clip = landingAnimalSoundDictionary[state];
                StartCoroutine(IEPlaySoundRandomly(landingAnimalSoundDictionary[state], randomRangeMin, randomRangeMax));
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
        while(true)
        {
            if(clip == dynamicAudio.clip)
            {
                dynamicAudio.Play();
            } else
            {
                yield break;
            }

            yield return new WaitForSeconds(Random.Range(randomStart, randomEnd) + dynamicAudio.clip.length);
        }
    }

    IEnumerator IEPlaySound(AudioClip clip, float soundInterval)
    {
        if(soundInterval == 0)
        {
            staticAudio.loop = true;
        } else
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

            if(staticAudio.loop == false)
            {
                yield return new WaitForSeconds(staticAudio.clip.length + soundInterval + float.Epsilon);
            } else
            {
                yield break;
            }

        }
    }

    float CalculateChangeBySecond(float chance)
    {
        return (1f / chance) * Random.Range(0, 2) * 1f / Time.fixedDeltaTime;
    }

    IEnumerator IERandomDirection()
    {
        while(true)
        {
            wanderDirection = RandomDirection();
            yield return new WaitForSeconds(Random.Range(minWanderChangeDirectionInSeconds, maxWanderChangeDirectionInSeconds));
        }
    }

    Vector3 RandomDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    bool IsGrounded()
    {
        return Physics.BoxCast(transform.position, new Vector3(groundCheckRadius, distantToGround, groundCheckRadius), -Vector3.up, Quaternion.identity, distantToGround + float.Epsilon);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.down * distantToGround, new Vector3(groundCheckRadius * 2, distantToGround, groundCheckRadius * 2));
    }
    #endregion
}
