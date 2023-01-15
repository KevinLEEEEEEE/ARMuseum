using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;


public class GrabbableExhibit : MonoBehaviour
{
    public GameObject Orbs;
    public Material defaultMaterial;

    private Transform centerAnchor
    {
        get
        {
            return NRSessionManager.Instance.CenterCameraAnchor;
        }
    }
    //private bool isDeleting = false;

    private void OnEnable()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        HideOrbs();
        //isDeleting = false;
    }

    public void DeleteStart()
    {
        //isDeleting = true;
    }

    public void DeleteStop()
    {
        //isDeleting = false;
    }

    public void DeleteComplete()
    {
        //isDeleting = false;
        SendMessageUpwards("InactiveGrabbleItem", transform.gameObject);
    }

    public void ShowOrbs()
    {
        foreach (Transform child in Orbs.transform)
        {
            child.gameObject.SetActive(true);
        }
    }   

    public void HideOrbs()
    {
        foreach (Transform child in Orbs.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void MoveToDestinationFrom(Transform point)
    {
        Vector3 startPoint = point.position;
        Vector3 endPoint = centerAnchor.position + centerAnchor.forward * 0.4f;

        StartCoroutine(.4f.Tweeng((p) => {
            transform.position = p;
            transform.LookAt(2 * transform.position - centerAnchor.transform.position);
        }, startPoint, endPoint));
        
    }
}


