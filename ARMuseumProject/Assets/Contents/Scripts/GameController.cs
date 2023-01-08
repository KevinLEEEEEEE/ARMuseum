using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController : MonoBehaviour
{
    [SerializeField] private TrackableObserver Observer;
    [SerializeField] private TrackingItemsController _TrackingItemsController;
    [SerializeField] private GrabbableController _GrabbableController;
    [SerializeField] private Transform[] TrackingItemList;
    private bool isTracking = false;

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

        isTracking = true;
    }

    private void Lost()
    {
        isTracking = false;
    }

    public void BeginTour()
    {
        if(isTracking)
        {
            _TrackingItemsController.StartNavigating();
        }
        
    }

    public void EndTour()
    {
        if(isTracking)
        {
            _TrackingItemsController.StopNavigating();
            _GrabbableController.ResetAll();
        }
    }
}
