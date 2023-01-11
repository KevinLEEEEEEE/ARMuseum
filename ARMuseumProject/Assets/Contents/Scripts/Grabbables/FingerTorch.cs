using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class FingerTorch : MonoBehaviour
{
    public Transform RightIndexTipLight;
    public Transform LeftIndexTipLight;

    void Start()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        RightIndexTipLight.gameObject.SetActive(false);
        LeftIndexTipLight.gameObject.SetActive(false);
    }

    void Update()
    {
        HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);

        if(rightHandState.isTracked && rightHandState.currentGesture == HandGesture.Point)
        {
            RightIndexTipLight.position = rightHandState.GetJointPose(HandJointID.IndexTip).position;

            if(!RightIndexTipLight.gameObject.activeSelf)
            {
                RightIndexTipLight.gameObject.SetActive(true);
            }
        } else
        {
            if (RightIndexTipLight.gameObject.activeSelf)
            {
                RightIndexTipLight.gameObject.SetActive(false);
            }
        }

        if(leftHandState.isTracked && leftHandState.currentGesture == HandGesture.Point)
        {
            LeftIndexTipLight.position = leftHandState.GetJointPose(HandJointID.IndexTip).position;

            if (!LeftIndexTipLight.gameObject.activeSelf)
            {
                LeftIndexTipLight.gameObject.SetActive(true);
            }
        } else
        {
            if (LeftIndexTipLight.gameObject.activeSelf)
            {
                LeftIndexTipLight.gameObject.SetActive(false);
            }
        }
    }
}
