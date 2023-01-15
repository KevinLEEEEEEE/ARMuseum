using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class HandCoach_Point : MonoBehaviour
{
    public InteractionHint interactionHint;
    private Transform centerAnchor
    {
        get
        {
            return NRSessionManager.Instance.CenterCameraAnchor;
        }
    }
    private bool isFirstUse = true;

    public void StartHintLoop()
    {
        transform.position = centerAnchor.position + centerAnchor.forward * 0.4f;
        interactionHint.StartHintLoop();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFirstUse)
        {
            HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
            HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

            if (rightHandState.currentGesture == HandGesture.Point || leftHandState.currentGesture == HandGesture.Point)
            {
                interactionHint.StopHintLoop();
                isFirstUse = false;
            }
        }
    }
}
