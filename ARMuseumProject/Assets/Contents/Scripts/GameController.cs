using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using NRKernal;

public class GameController : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("Observer")]
    private TrackableObserver observer;

    [SerializeField]
    [FormerlySerializedAs("TrackingItemList")]
    private Transform[] FollowTrackerList;

    public event Action FoundObserverEvent;
    public event Action LostObserverEvent;
    public event Action StartRaycastEvent;
    public event Action StopRaycastEvent;
    public event Action BeginTourEvent;
    public event Action EndTourEvent;

    private bool isTracking = false;
    private bool isGrabbing = false;

    void Start()
    {
#if !UNITY_EDITOR
        Destroy(GameObject.Find("Env_Room"));
#endif
        observer.FoundEvent += Found;
        observer.LostEvent += Lost;
    }

    private void Found(Vector3 pos, Quaternion qua)
    {
        foreach(Transform trans in FollowTrackerList)
        {
            trans.position = pos;
            trans.rotation = qua;
        }

        if(!isTracking)
        {
            FoundObserverEvent?.Invoke();
            isTracking = true;
        }
    }

    private void Lost()
    {
        if(isTracking)
        {
            LostObserverEvent?.Invoke();
            isTracking = false;
        }
    }

    public void BeginTour()
    {
        BeginTourEvent?.Invoke();
    }

    public void EndTour()
    {
        EndTourEvent?.Invoke();
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
            StopRaycastEvent?.Invoke();
            NRInput.LaserVisualActive = canRaycast;
        } else if (!NRInput.LaserVisualActive && canRaycast)
        {
            StartRaycastEvent?.Invoke();
            NRInput.LaserVisualActive = canRaycast;
        }
    }
}
