using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hi5_Interaction_Core;

public enum EHandGestureState
{
    Idle,
    Fist,
    Planed,
    Ok,
    IndexPoint,
};
public enum EHandType
{
    Left,
    Right,
}
public class HandGestureManager : MonoBehaviour
{
    public bool isLeft;
    public Hi5_Glove_Interaction_Hand handInteraction = null;
    public Transform moveAnchor;
    private EHandGestureState gestureStates;
    private EHandType handType;
    public Vector3 handOffset;

    private void Awake()
    {

    }

    private void Start()
    {
        if(isLeft)
        {
            handType = EHandType.Left;
        } else
        {
            handType = EHandType.Right;
        }
    }

    void Update()
    {
        if (IsCloseThumbAndIndexCollider())
        {
            handInteraction.mVisibleHand.SetThumbAndIndexFingerCollider(false);
        }
        else
            handInteraction.mVisibleHand.SetThumbAndIndexFingerCollider(true);

        if (IsHandFist())
        {
            //mHand.mVisibleHand.ChangeColor(Color.red);
            gestureStates = EHandGestureState.Fist;
            //Debug.Log("Hand Fist!");
        }
        else if (IsHandIndexPoint())
        {
            //mHand.mVisibleHand.ChangeColor(Color.black);
            gestureStates = EHandGestureState.IndexPoint;
            //Debug.Log("Hand Indexed!");
        }

        else if (IsHandPlane())
        {
            //mHand.mVisibleHand.ChangeColor(Color.green);
            gestureStates = EHandGestureState.Planed;
            //Debug.Log("Hand Planed!");
        }
        else if (IsOk())
        {
            //mHand.mVisibleHand.ChangeColor(Color.yellow);
            gestureStates = EHandGestureState.Ok;
            //Debug.Log("Hand OK!");
        }
        else
        {
            handInteraction.mVisibleHand.ChangeColor(handInteraction.mVisibleHand.orgColor);
            gestureStates = EHandGestureState.Idle;
            //Debug.Log("Hand Idle!");
        }
    }

    public Transform HandPosition()
    {
        return moveAnchor;
    }
    public EHandGestureState GetHandGestureState()
    {
        return gestureStates;
    }

    public EHandType GetHandType()
    {
        return handType;
    }

    internal bool IsOk()
    {
        // return mHand.mFingers[Hi5_Glove_Interaction_Finger_Type.EThumb].IsTumbColliderIndex();
        if (handInteraction != null && handInteraction.mState != null && handInteraction.mState.mJudgeMent != null)
        {
            return handInteraction.mState.mJudgeMent.IsOK();
        }
        else
            return false;
    }

    internal bool IsCloseThumbAndIndexCollider()
    {
        if (handInteraction != null && handInteraction.mState != null && handInteraction.mState.mJudgeMent != null)
        {
            return handInteraction.mState.mJudgeMent.IsCloseThumbAndIndexCollider();
        }
        else
            return false;
    }

    internal bool IsFlyPinch()
    {
        if (handInteraction != null && handInteraction.mState != null && handInteraction.mState.mJudgeMent != null)
        {
            return handInteraction.mState.mJudgeMent.IsGestureFlyPinch();
        }
        else
            return false;
    }

    internal bool IsPinch2()
    {
        if (handInteraction != null && handInteraction.mState != null && handInteraction.mState.mJudgeMent != null)
        {
            return handInteraction.mState.mJudgeMent.IsGesturePinch2();
        }
        else
            return false;
    }

    internal bool IsHandPlane()
    {
        if (handInteraction != null && handInteraction.mState != null && handInteraction.mState.mJudgeMent != null)
        {
            return handInteraction.mState.mJudgeMent.IsFingerPlane();
        }
        else
            return false;
    }

    internal bool IsHandFist()
    {
        if (handInteraction != null && handInteraction.mState != null && handInteraction.mState.mJudgeMent != null)
        {
            return handInteraction.mState.mJudgeMent.IsHandFist();
        }
        else
            return false;
    }

    internal bool IsHandIndexPoint()
    {
        if (handInteraction != null && handInteraction.mState != null && handInteraction.mState.mJudgeMent != null)
        {
            return handInteraction.mState.mJudgeMent.IsHandIndexPoint();
        }
        else
            return false;
    }
}
