using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class ShellMatchManager : MonoBehaviour
{
    private Animator animatorComp;
    private HandState rightHandState;
    private HandState leftHandState;
    private HandState holdingHand;
    private bool isActive;

    private void Start()
    {
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        animatorComp = transform.GetComponent<Animator>();

        Reset();
    }

    public void Reset()
    {
        isActive = false;
    }

    public void LightUpMatch()
    {
        if (isActive)
            return;

        isActive = true;
        holdingHand = rightHandState.isPinching ? rightHandState : leftHandState;
        animatorComp.Play("MatchFadeIn");
    }

    public void PutOutMatch()
    {
        if (!isActive)
            return;

        isActive = false;
        holdingHand = null;
        animatorComp.Play("MatchBurnout");
    }

    private void Update()
    {
        if(isActive)
        {
            transform.position = holdingHand.GetJointPose(HandJointID.ThumbTip).position;
        }
    }
}
