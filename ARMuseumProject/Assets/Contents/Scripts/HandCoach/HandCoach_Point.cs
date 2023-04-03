using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class HandCoach_Point : MonoBehaviour
{
    public InteractionHint interactionHint;
    public GameController gameController;
    private Transform centerAnchor
    {
        get
        {
            return NRSessionManager.Instance.CenterCameraAnchor;
        }
    }
    private bool isFirstUse = true;

    private void Start()
    {
        //gameController.StopRaycastEvent += GrabStart;
    }

    public void StartHintLoop()
    {
        if (isFirstUse)
        {
            transform.position = centerAnchor.position + centerAnchor.forward * 0.4f;
            interactionHint.StartHintLoop();
        }
    }

    private void GrabStart()
    {
        if(isFirstUse)
        {
            interactionHint.StopHintLoop();
            isFirstUse = false;
        }
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
