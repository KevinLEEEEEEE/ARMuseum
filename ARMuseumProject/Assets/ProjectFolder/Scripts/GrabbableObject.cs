using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class GrabbableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ObjectMesh;
    public Material DeleteMaterial;
    public Material RealisticMaterial;
    public GameObject InfoContact;

    private Vector3 TargetPosition
    {
        get
        {
            return CenterCamera.transform.position + CenterCamera.transform.forward * 0.4f;
        }
    }
    private Transform m_CenterCamera;
    private Transform CenterCamera
    {
        get
        {
            if (m_CenterCamera == null)
            {
                if (NRSessionManager.Instance.CenterCameraAnchor != null)
                {
                    m_CenterCamera = NRSessionManager.Instance.CenterCameraAnchor;
                }
                else if (Camera.main != null)
                {
                    m_CenterCamera = Camera.main.transform;
                }
            }
            return m_CenterCamera;
        }
    }
    private bool canFollowCamera = true;
    //private bool isReadyToDelete = false;

    private void OnEnable()
    {
        LookAtCamera();

        ExitDeleteMode();

        HideInfoContact();
    }

    public void ShowInfoContact()
    {
        if(canFollowCamera == false)
        {
            foreach (Transform child in InfoContact.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    public void HideInfoContact()
    {
        foreach (Transform child in InfoContact.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void EnterDeleteMode()
    {
        if (canFollowCamera == false)
        {
            ObjectMesh.GetComponent<Renderer>().material = DeleteMaterial;

            Debug.Log("[Player] " + transform.name + " enter delete mode");
        }
    }

    public void ExitDeleteMode()
    {
        ObjectMesh.GetComponent<Renderer>().material = RealisticMaterial;
    }

    public void ResetAll()
    {
        
    }

    private void LookAtCamera()
    {
        ObjectMesh.transform.LookAt(CenterCamera.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[Player] Grabbable Object on collision " + collision.gameObject.name);
    }

    void Update()
    {
        if (canFollowCamera == true)
        {
            if(TargetPosition != transform.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, TargetPosition, 0.025f);
            } else
            {
                canFollowCamera = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pointer click object: " + transform.name);

        LookAtCamera();

        canFollowCamera = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer enter object: " + transform.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exit object: " + transform.name);
    }
}
