using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GrabbableController : MonoBehaviour
{
    public TrackingItemsController _TrackingItemController;
    private GrabbableState CurrState = GrabbableState.Default;
    private enum GrabbableState
    {
        Default,
        ShowDelete,
        ShowInfoContact
    }

    void Start()
    {
        ResetAll();
    }

    public void ActiveGrabbableItem(GameObject obj)
    {
        Transform targetObject = transform.Find(obj.name + "-Grabbable");

        targetObject.position = obj.transform.position;
        targetObject.rotation = obj.transform.rotation;
        targetObject.gameObject.SetActive(true);
    }

    public void InactiveGrabbleItem(string name)
    {
        Transform targetObject = transform.Find(name);

        targetObject.gameObject.SetActive(false);

        _TrackingItemController.RestoreObject(name.Substring(0, name.Length - 10));
    }

    private void ShowInfoContact()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableObject>().ShowInfoContact();
        }
    }

    private void HideInfoContact()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableObject>().HideInfoContact();
        }
    }

    private void EnterDeleteMode()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableObject>().EnterDeleteMode();
        }
    }

    private void ExitDeleteMode()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableObject>().ExitDeleteMode();
        }
    }

    public void ResetAll()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        CurrState = GrabbableState.Default;
    }

    private void Update()
    {
        HandState handState = NRInput.Hands.GetHandState(HandEnum.RightHand);

        if (handState.isTracked == true)
        {
            if (handState.currentGesture == HandGesture.Point)
            {
                if(CurrState == GrabbableState.ShowInfoContact)
                {
                    return;
                }

                ExitDeleteMode();
                ShowInfoContact();

                CurrState = GrabbableState.ShowInfoContact;
            }
            else if (handState.currentGesture == HandGesture.Victory || Input.GetKey(KeyCode.Space)) // 因为模拟器无法模拟Victory，以空格替代
            {
                if (CurrState == GrabbableState.ShowDelete)
                {
                    return;
                }

                HideInfoContact();
                EnterDeleteMode();

                CurrState = GrabbableState.ShowDelete;
            } else
            {
                if (CurrState == GrabbableState.Default)
                {
                    return;
                }

                ExitDeleteMode();
                HideInfoContact();

                CurrState = GrabbableState.Default;
            }  
        }
    }
}
