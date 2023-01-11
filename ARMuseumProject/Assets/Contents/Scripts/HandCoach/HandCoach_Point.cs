using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class HandCoach_Point : MonoBehaviour
{
    public InteractionHint _HandCoach;
    public float ForwardOffset;
    private GameObject CenterCameraAnchor;
    private bool isFirstUse = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        CenterCameraAnchor = GameObject.Find("NRCameraRig/CenterAnchor");
        transform.position = CenterCameraAnchor.transform.position + CenterCameraAnchor.transform.forward * ForwardOffset;
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
                _HandCoach.StopHintLoop();
                isFirstUse = false;
            }
        }
    }
}
