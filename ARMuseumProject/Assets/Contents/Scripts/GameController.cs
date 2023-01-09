using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController : MonoBehaviour
{
    [SerializeField] private TrackableObserver Observer;
    [SerializeField] private TrackingItemsController _TrackingItemsController;
    [SerializeField] private Orb_Switch _Orb_Switch;
    [SerializeField] private GrabbableController _GrabbableController;
    [SerializeField] private Transform[] TrackingItemList;
    private bool isGrabbing = false;

    void Start()
    {
#if !UNITY_EDITOR
        Destroy(GameObject.Find("Env_Room"));
#endif
        Observer.FoundEvent += Found;
        Observer.LostEvent += Lost;
    }

    private void Found(Vector3 pos, Quaternion qua)
    {
        foreach(Transform child in TrackingItemList)
        {
            child.position = pos;
            child.rotation = qua;
        }
    }

    private void Lost()
    {
        // 考虑新增图像丢失提示功能
    }

    public void BeginTour()
    {
        _TrackingItemsController.StartNavigating();
    }

    public void EndTour()
    {
        _TrackingItemsController.StopNavigating();
        _GrabbableController.ResetAll();
    }

    public void GrabStart()
    {
        isGrabbing = true;
    }

    public void GrabEnd()
    {
        isGrabbing = false;
    }

    private void Update()
    {
        HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        bool canRaycast = true;

        // Stop raycast detection when grabbing or pointing
        if (isGrabbing || rightHandState.currentGesture == HandGesture.Point || leftHandState.currentGesture == HandGesture.Point)
        {
            canRaycast = false;
        }

        if(NRInput.LaserVisualActive && !canRaycast)
        {
            _TrackingItemsController.StopRayastDetection();
            _Orb_Switch.StopRayastDetection();
            NRInput.LaserVisualActive = canRaycast;
        } else if (!NRInput.LaserVisualActive && canRaycast)
        {
            _TrackingItemsController.StartRaycastDetection();
            _Orb_Switch.StartRaycastDetection();
            NRInput.LaserVisualActive = canRaycast;
        }
    }
}
