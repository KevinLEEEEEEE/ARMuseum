using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GrabbableExhibit : MonoBehaviour
{
    public Material defaultMaterial;
    public float forwardOffset;

    private GameObject objectMesh;
    private GameObject infoOrb;
    private Transform m_CenterAnchor;
    private Transform centerAnchor
    {
        get
        {
            if (m_CenterAnchor == null)
            {
                m_CenterAnchor = NRSessionManager.Instance.NRHMDPoseTracker.centerAnchor;
            }
            return m_CenterAnchor;
        }
    }
    private bool canMoveTowards = true;
    private bool isDeleting = false;

    private void OnEnable()
    {
        objectMesh = transform.Find("Mesh").gameObject;
        infoOrb = transform.Find("Orbs").gameObject;
        objectMesh.transform.LookAt(centerAnchor);
        ResetAll();
    }

    private void OnDisable()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        HideOrbs();

        canMoveTowards = true;
        isDeleting = false;
    }

    public void DeleteStart()
    {
        isDeleting = true;
    }

    public void DeleteStop()
    {
        isDeleting = false;
    }

    public void DeleteComplete()
    {
        isDeleting = false;
        SendMessageUpwards("InactiveGrabbleItem", transform.gameObject);
    }

    public void ShowOrbs()
    {
        foreach (Transform child in infoOrb.transform)
        {
            child.gameObject.SetActive(true);
        }
    }   

    public void HideOrbs()
    {
        foreach (Transform child in infoOrb.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (canMoveTowards)
        {
            Vector3 target = centerAnchor.position + centerAnchor.forward * forwardOffset;

            if (Vector3.Distance(transform.position, target) >= 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, 0.02f);
            }
            else
            {
                canMoveTowards = false;
            }
        }
    }
}
