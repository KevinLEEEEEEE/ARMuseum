using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GrabbableController : MonoBehaviour
{
    public TrackingItemsController _TrackingItemController;

    private GameObject IndexTip;
    private GameObject MiddleTip;
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

        InitFingerTip();
    }

    private void InitFingerTip()
    {
        GameObject NRHandVisual = GameObject.Find("NRHandCapsuleVisual_R");

        IndexTip = NRHandVisual.transform.GetChild(31).gameObject;
        MiddleTip = NRHandVisual.transform.GetChild(35).gameObject;

        IndexTip.GetComponent<SphereCollider>().enabled = true;
        IndexTip.GetComponent<SphereCollider>().isTrigger = true;

        MiddleTip.GetComponent<SphereCollider>().enabled = true;
        MiddleTip.GetComponent<SphereCollider>().isTrigger = true;
    }

    public void ActiveGrabbableItem(GameObject obj)
    {
        Transform targetObject = transform.Find(obj.name + "-Grabbable");

        targetObject.position = obj.transform.position;
        targetObject.rotation = obj.transform.rotation;
        targetObject.gameObject.SetActive(true);
    }

    public void InactiveGrabbleItem(GameObject obj)
    {
        obj.SetActive(false);

        string objectName = obj.transform.name;

        _TrackingItemController.RestoreObject(objectName.Substring(0, objectName.Length - 10));
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
