using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class FlyingAnimal : MonoBehaviour
{
    public enum EFlyingAnimalState
    {
        Idle,
        Chase,
        Fly,
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
        public AudioClip audioClip;
    }

    #region 변수 선언
    public HandGestureManager leftHand;
    public HandGestureManager rightHand;
    public SFlyingAnimalAnimation[] animations;
    public SFlyingAnimalSound[] sounds;
    public Dictionary<EFlyingAnimalAnimationState, string> flyingAnimalAnimationDictionary;
    public Dictionary<EFlyingAnimalSoundState, AudioClip> flyingAnimalSoundDictionary;
    public Transform flyingTargetTransform;

    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float gravity;
    [SerializeField] float landingDistance;
    [SerializeField] float minAltitude;
    [SerializeField] float maxAltitude;
    [SerializeField] float minAltitudeLimit;
    [SerializeField] float maxAltitudeLimit;
    [SerializeField] float turnSpeed, idleSpeed, flySpeed, hopSpeed;
    [SerializeField] float maxDistanceFromBase;
    [SerializeField] bool returnToBase;
    [SerializeField] string groundLayer;
    [SerializeField] string waterLayer;
    [Range(0, 1)][SerializeField] float chanceToFind;
    [Range(0, 1)] [SerializeField] float chanceToFly;
    [Range(0, 1)] [SerializeField] float chanceToStand;
    [Range(0, 1)] [SerializeField] float chanceToPecking;
    [Range(0, 1)] [SerializeField] float chanceToRunAway;
    [Range(0, 1)] [SerializeField] float chanceToChangeTarget;
    [Range(0, 1)] [SerializeField] float chanceToRandomSound;

    [SerializeField] ArduinoInteraction arduinoInteraction;

    Animator birdAnimator;
    [SerializeField]
    EFlyingAnimalState flyingAnimalState;
    CharacterController controller;
    float distanceFromBase, distanceFromTarget;
    Vector3 rotation, position, direction, velocity;
    Vector3 landingPosition;
    Quaternion lookRotation;
    Vector3 baseTarget;
    public bool prevGrounded;
    public EHandType prevHand = EHandType.Null;

    float prevz, zturn;
    float changeToTarget = 0f, changeToFind = 0f, changeToFly = 0f, changeToStand = 0f, changeToPecking = 0f, changeToRunAway = 0f;
    #endregion

    #region Unity Functions
    // Start is called before the first frame update
    void Start()
    {
        prevz = 0;
        flyingAnimalState = EFlyingAnimalState.Idle;
        birdAnimator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        flyingAnimalAnimationDictionary = new Dictionary<EFlyingAnimalAnimationState, string>();
        changeToStand = CalculateChangeBySecond(chanceToStand);

        for(int i=0;i<animations.Length;i++)
        {
            flyingAnimalAnimationDictionary.Add(animations[i].flyingAnimalAnimationState, animations[i].transitionName);
        }
    }

    // Update is called once per frame
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
            case EFlyingAnimalState.Fly:
                FixedUpdateFly();
                break;
            case EFlyingAnimalState.Stand:
                FixedUpdateStand();
                break;
            case EFlyingAnimalState.Landing:
                FixedUpdateLanding();
                break;
        }

        UpdateChance();

        if (rightHand.GetHandGestureState() == EHandGestureState.Planed && rightHand.IsPositionUpside())
        {
            flyingTargetTransform = rightHand.HandPosition();
            if(flyingAnimalState == EFlyingAnimalState.Fly)
            {
                flyingAnimalState = EFlyingAnimalState.Chase;
            }
        } else
        {
            flyingAnimalState = EFlyingAnimalState.Fly;
        }

        //debug purpose only
        /*
        if (flyingTargetTransform.gameObject.activeSelf == true && flyingAnimalState == EFlyingAnimalState.Fly)
        {
            flyingAnimalState = EFlyingAnimalState.Chase;
        }
        else if(flyingTargetTransform.gameObject.activeSelf == false)
        {
            flyingAnimalState = EFlyingAnimalState.Fly;
        }
        */
        
        if(controller.isGrounded)
        {
            arduinoInteraction.SetStiffness(1);
        } else
        {
            arduinoInteraction.SetStiffness(0);
        }

        //새가 날지 않는 상황 즉 chase 하지도 않고, fly 하지도 않는 상태면 중력을 준다. 
        if(flyingAnimalState == EFlyingAnimalState.Stand)
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
        }

        controller.Move(velocity * Time.fixedDeltaTime);
    }
    #endregion

    #region Update Methods
    void FixedUpdateIdle()
    {
        flyingAnimalState = EFlyingAnimalState.Fly;
    }

    void FixedUpdateStand()
    {
        if (flyingTargetTransform == null)
        {
            flyingAnimalState = EFlyingAnimalState.Idle;
            return;
        }

        SetAnimator(EFlyingAnimalAnimationState.Idle_1, true);

        if (prevGrounded && controller.isGrounded == false)
        {
            SetAnimator(EFlyingAnimalAnimationState.Flight_straight, true);
            flyingAnimalState = EFlyingAnimalState.Fly;
        }

        if(controller.isGrounded == false)
        {
            SetAnimator(EFlyingAnimalAnimationState.Flight_straight, true);
            prevGrounded = false;
        } else
        {
            prevGrounded = true;
        }

        if(rightHand.GetHandGestureState() == EHandGestureState.Planed)
        {
            controller.transform.position = rightHand.HandPosition().position;
            transform.rotation = rightHand.HandPosition().rotation * Quaternion.Euler(180, 90, 0);
        } else
        {
            flyingAnimalState = EFlyingAnimalState.Fly;
            SetAnimator(EFlyingAnimalAnimationState.Take_off, true);
        }
    }

    void FixedUpdateChase()
    {
        if (flyingTargetTransform == null)
        {
            flyingAnimalState = EFlyingAnimalState.Idle;
            return;
        }

        //일단은 100을 더해서 레이를 쏴보자...
        landingPosition = GetLandingPosition(flyingTargetTransform.position + new Vector3(0, 100, 0)) + new Vector3(0, landingDistance / 1.5f, 0);

        distanceFromTarget = Vector3.Magnitude(landingPosition - transform.position);

        if (flyingTargetTransform != null)
        {
            this.rotation = landingPosition - transform.position;
        } else
        {
            flyingAnimalState = EFlyingAnimalState.Idle;
        }

        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(landingPosition.x, landingPosition.z)) < landingDistance)
        {
            flyingAnimalState = EFlyingAnimalState.Landing;
            Debug.Log("Landing");
        }

        UpdateFlyingMotionVelocityAndAnimation();
    }

    void FixedUpdateLanding()
    {
        if(flyingTargetTransform == null)
        {
            flyingAnimalState = EFlyingAnimalState.Idle;
            return;
        }

        SetAnimator(EFlyingAnimalAnimationState.Flight_straight, true);

        //일단은 100을 더해서 레이를 쏴보자...
        landingPosition = GetLandingPosition(flyingTargetTransform.position + new Vector3(0, 100, 0)) + new Vector3(0, landingDistance / 1.5f, 0);

        rotation = landingPosition - transform.position;
        rotation.y = 0;

        UpdateFlyingMotionVelocityAndAnimation();

        if(Vector2.Distance(new Vector2(landingPosition.x, landingPosition.z), new Vector2(transform.position.x, transform.position.z)) < controller.radius / 1f)
        {
            flyingAnimalState = EFlyingAnimalState.Stand;
            velocity = Vector3.zero;
        }
    }

    void FixedUpdateFly()
    {
        distanceFromBase = Vector3.Magnitude(baseTarget - transform.position);

        if (returnToBase && maxDistanceFromBase < distanceFromBase)
        {
            this.rotation = baseTarget - transform.position;
        }

        if (changeToTarget <= 0f)
        {
            this.rotation = RandomDirection(transform.position);
            changeToTarget = CalculateChangeBySecond(chanceToChangeTarget);
        }

        if(changeToStand <= 0f)
        {
            changeToStand = CalculateChangeBySecond(changeToStand);

            int layerMask = 1 << LayerMask.NameToLayer(groundLayer);
            layerMask += 1 << LayerMask.NameToLayer(waterLayer);

            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue, layerMask))
            {
                landingPosition = hitInfo.point;
                flyingAnimalState = EFlyingAnimalState.Landing;
            }
        }

        if (CalculateAmplitude() < minAltitude)
        {
            this.rotation.y = Mathf.Abs(this.rotation.y);
            //need to climb faster so we set 1 cause if return value of RandomDirection is btwn 0 and 1
        }
        else if (CalculateAmplitude() > maxAltitude)
        {
            this.rotation.y = -Mathf.Abs(this.rotation.y);
        }

        UpdateFlyingMotionVelocityAndAnimation();
    }

    //날아다니는 새의 회전축 기울임과, 속력 그리고 애니매이션을 업데이트한다. 
    void UpdateFlyingMotionVelocityAndAnimation()
    {
        //새가 회전하는 방향으로 새의 중심축을 이동한다.
        float temp = prevz;
        zturn = Mathf.Clamp(Vector3.SignedAngle(this.rotation, direction, Vector3.up), -45f, 45f);
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
        lookRotation = Quaternion.LookRotation(this.rotation, Vector3.up);

        //천천히 새가 바라보는 방향으로 고개를 튼다. 또한 전에 설정한 prevz 를 적용시킨다.
        Vector3 rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
        transform.eulerAngles = rotation;
        transform.Rotate(0f, 0f, prevz - temp, Space.Self);

        //direction 을 설정하고, 계산된 속도를 바탕으로 속력을 구한다. 이 속력은 Move 를 통해 업데이트 된다. 
        direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
        velocity = flyingAnimalState == EFlyingAnimalState.Idle ? idleSpeed * direction : flySpeed * direction;

        //애니매이션을 업데이트 하는 구간
        if (this.rotation.y > 0)
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
        else if (this.rotation.y <= 0)
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
        if (flyingAnimalAnimationDictionary.ContainsKey(state) && birdAnimator.GetBool(flyingAnimalAnimationDictionary[state]) == false)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                birdAnimator.SetBool(animations[i].transitionName, false);
            }
            birdAnimator.SetBool(flyingAnimalAnimationDictionary[state], b);
        }
    }

    void UpdateChance()
    {
        if (changeToTarget > 0)
        {
            changeToTarget -= 1;
        }
        if (changeToFind > 0)
        {
            changeToFind -= 1;
        }
        if (changeToFly > 0)
        {
            changeToFly -= 1;
        }
        if (changeToPecking > 0)
        {
            changeToPecking -= 1;
        }
        if (changeToRunAway > 0)
        {
            changeToRunAway -= 1;
        }
        if (changeToStand > 0)
        {
            changeToStand -= 1;
        }
    }

    //Scripts for usefull things
    void OccupyHand(EHandType handType)
    {
        if(handType == EHandType.Right)
        {
            rightHand.OccupyHand();
            prevHand = EHandType.Right;
        } else
        {
            leftHand.OccupyHand();
            prevHand = EHandType.Left;
        }
    }

    void ReleaseHand(EHandType handType)
    {
        if(handType == EHandType.Right)
        {
            rightHand.ReleaseHand();
            prevHand = EHandType.Null;
        } else
        {
            leftHand.ReleaseHand();
            prevHand = EHandType.Null;
        }
    }
    float CalculateAmplitude()
    {
        int layerMask = 1 << LayerMask.NameToLayer(groundLayer);
        layerMask += 1 << LayerMask.NameToLayer(waterLayer);

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue, layerMask);

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

    Vector3 RandomDirection(Vector3 currentPosition)
    {
        float angleXZ = Random.Range(-Mathf.PI, Mathf.PI);
        float angleY = Random.Range(-Mathf.PI, Mathf.PI);

        return Mathf.Sin(angleXZ) * Vector3.forward + Mathf.Cos(angleXZ) * Vector3.right + Mathf.Sin(angleY) * Vector3.up;
    }
    #endregion
}