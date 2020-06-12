using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class FlyingAnimal : MonoBehaviour
{
    public enum EFlyingAnimalState
    {
        Idle,
        Chase,
        Fly,
        Stand,
        Pecking,
        RunAway,
        Hoping,
    }
    
    [System.Serializable]
    public struct FlyingAnimalAnimation
    {
        public EFlyingAnimalState flyingAnimalState;
        public string transitionName;
    }

    #region 변수 선언
    public HandGestureManager leftHand;
    public HandGestureManager rightHand;
    public FlyingAnimalAnimation[] animations;

    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float minAltitude;
    [SerializeField] float maxAltitude;
    [SerializeField] float turnSpeed, idleSpeed, flySpeed, hopSpeed;
    [SerializeField] float perceptionRadius;
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

    Animator birdAnimator;
    EFlyingAnimalState birdState;
    Rigidbody birdBody;
    float distanceFromBase, distanceFromTarget;
    Vector3 birdRotation, position, direction, velocity;
    Quaternion lookRotation;
    Vector3 baseTarget;
    GameObject flyingTarget;


    float prevz, prevSpeed;
    float changeToTarget = 0f, changeToFind = 0f, changeToFly = 0f, changeToStand = 0f, changeToPecking = 0f, changeToRunAway = 0f;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        prevz = 0;
        birdState = EFlyingAnimalState.Idle;
        birdAnimator = GetComponent<Animator>();
        birdBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (birdState)
        {
            case EFlyingAnimalState.Idle:
                UpdateIdle();
                break;
            case EFlyingAnimalState.Chase:
                UpdateChase();
                break;
            case EFlyingAnimalState.RunAway:
                UpdateRunAway();
                break;
            case EFlyingAnimalState.Fly:
                UpdateFly();
                break;
            case EFlyingAnimalState.Pecking:
                UpdatePecking();
                break;
            case EFlyingAnimalState.Stand:
                UpdateStand();
                break;
            case EFlyingAnimalState.Hoping:
                UpdateHopping();
                break;
        }
    }

    private void FixedUpdate()
    {
        FixedUpdateFlyingMovement();
        UpdateChance();
    }

    #region Update Methods
    void UpdateIdle()
    {
        FixedUpdateFlyingMovement();
        /*
        if(leftHand.GetHandGestureState() == EHandGestureState.Planed || rightHand.GetHandGestureState() == EHandGestureState.Planed)
        {
            birdState = EBirdState.Chase;
        } else
        {
            birdState = EBirdState.Fly;
        }
        */
    }

    void UpdateRunAway()
    {

    }

    void UpdateHopping()
    {

    }

    void UpdateChase()
    {

    }

    void UpdateFly()
    {

    }

    void UpdateStand()
    {

    }

    void UpdatePecking()
    {

    }

    void FixedUpdateFlyingMovement()
    {
        distanceFromBase = Vector3.Magnitude(baseTarget - transform.position);

        if(returnToBase && maxDistanceFromBase < distanceFromBase)
        {
            birdRotation = baseTarget - transform.position;
        }

        if(changeToTarget <= 0f && flyingTarget == null)
        {
            birdRotation = RandomDirection(transform.position);
            Debug.Log(birdRotation);
            changeToTarget = CalculateChangeBySecond(chanceToChangeTarget);
        } else if(flyingTarget != null)
        {
            birdRotation = flyingTarget.transform.position - transform.position;
        }

        if (CalculateAmplitude() < minAltitude) 
        {
            birdRotation.y = Mathf.Abs(birdRotation.y);
        }
        if(CalculateAmplitude() > maxAltitude)
        {
            birdRotation.y = -Mathf.Abs(birdRotation.y);
        }

        //새가 회전하는 방향으로 새의 중심축을 이동한다.
        float temp = prevz;
        float zturn = Mathf.Clamp(Vector3.SignedAngle(birdRotation, direction, Vector3.up), -45f, 45f);
        if(prevz < zturn)
        {
            prevz += Mathf.Min(turnSpeed * Time.fixedDeltaTime, zturn - prevz);
        } else if(prevz >= zturn)
        {
            prevz -= Mathf.Min(turnSpeed * Time.fixedDeltaTime, prevz - zturn);
        }
        prevz = Mathf.Clamp(prevz, -45f, 45f);

        //새가 바라보는 방향으로 로테이션을 설정한다.
        lookRotation = Quaternion.LookRotation(birdRotation, Vector3.up);

        //천천히 새가 바라보는 방향으로 고개를 튼다. 또한 전에 설정한 prevz 를 적용시킨다.
        Vector3 rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
        transform.eulerAngles = rotation;
        transform.Rotate(0f, 0f, prevz - temp, Space.Self);

        direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
        birdBody.velocity = birdState == EFlyingAnimalState.Idle ? idleSpeed * direction : flySpeed * direction;
    }
    void UpdateChance()
    {
        if(changeToTarget > 0)
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
        if(changeToStand > 0)
        {
            changeToStand -= 1;
        }
    }

    #endregion

    //Scripts for usefull things
    float CalculateAmplitude()
    {
        int layerMask = 1 << LayerMask.NameToLayer(groundLayer);
        layerMask += 1 << LayerMask.NameToLayer(waterLayer);

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue, layerMask);

        return hitInfo.distance;
    }

    float CalculateChangeBySecond(float chance)
    {
        return (1f / chance) * Random.Range(0, 2) * 1f/Time.fixedDeltaTime;
    }

    Vector3 RandomDirection(Vector3 currentPosition)
    {
        float angleXZ = Random.Range(-Mathf.PI, Mathf.PI);
        float angleY = Random.Range(-Mathf.PI, Mathf.PI);

        return Mathf.Sin(angleXZ) * Vector3.forward + Mathf.Cos(angleXZ) * Vector3.right + Mathf.Sin(angleY) * Vector3.up;
    }
}