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
        Grab, // ��Grab�ڼ�֪ͨ���������raycast������
        ShowDelete,
        ShowInfoContact,
    }

    void Start()
    {
        ResetAll();
    }

    public void ActiveGrabbableItem(GameObject obj)
    {
        Transform targetObject = transform.Find(obj.name + "-Grabbable");

        targetObject.position = obj.transform.GetChild(0).position; // ���ó�ʼλ��
        targetObject.rotation = obj.transform.GetChild(0).rotation;
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

        SwitchFingerTipState(true);
    }

    private void ExitDeleteMode()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<GrabbableObject>().ExitDeleteMode();
        }

        SwitchFingerTipState(false);
    }

    public void StartGrab()
    {
        _TrackingItemController.StopRayastDetection();

        Debug.Log("[Player] Start grab.");
    }

    public void StopGrab()
    {
        _TrackingItemController.StartRaycastDetection();
        Debug.Log("[Player] Stop grab.");
    }

    private void SwitchFingerTipState(bool canTrigger)
    {
        GameObject NRHandVisual = GameObject.Find("NRHandCapsuleVisual_R");

        IndexTip = NRHandVisual.transform.GetChild(31).gameObject;
        MiddleTip = NRHandVisual.transform.GetChild(35).gameObject;

        IndexTip.GetComponent<SphereCollider>().enabled = canTrigger;
        IndexTip.GetComponent<SphereCollider>().isTrigger = canTrigger;

        MiddleTip.GetComponent<SphereCollider>().enabled = canTrigger;
        MiddleTip.GetComponent<SphereCollider>().isTrigger = canTrigger;
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
            else if (handState.currentGesture == HandGesture.Victory || Input.GetKey(KeyCode.Space)) // ��Ϊģ�����޷�ģ��Victory���Կո����
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
