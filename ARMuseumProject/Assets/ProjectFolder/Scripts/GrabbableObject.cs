using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class GrabbableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Material DeleteMaterial;
    public Material RealisticMaterial;
    public float ForwardOffset;

    private GameObject ObjectMesh;
    private GameObject ObjectBorder;
    private GameObject InfoContact;
    private GameObject InfoContent;
    private Vector3 TargetPosition
    {
        get
        {
            return CenterCamera.transform.position + CenterCamera.transform.forward * ForwardOffset;
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
    private bool canDelete = false;

    private void OnEnable()
    {
        InitGameObject();

        LookAtCamera();

        ExitDeleteMode();

        HideInfoContact();

        canFollowCamera = true;

        transform.position = TargetPosition;
    }

    private void InitGameObject()
    {
        if(ObjectMesh != null)
        {
            return;
        }

        ObjectMesh = transform.Find("ObjectMesh").gameObject;
        InfoContact = transform.Find("InfoContact").gameObject;
        InfoContent = transform.Find("InfoContent").gameObject;
        ObjectBorder = transform.Find("ObjectBorder").gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(canDelete == true)
        {
            Debug.Log("[Player] delete object: " + transform.name);

            SendMessageUpwards("InactiveGrabbleItem", transform.gameObject);
        }
    }

    public void ShowInfoContact()
    {
        //if(canFollowCamera == false)
        //{
            foreach (Transform child in InfoContact.transform)
            {
                child.gameObject.SetActive(true);
            }
        //}
    }

    public void HideInfoContact()
    {
        foreach (Transform child in InfoContact.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void HideInfoContent()
    {
        foreach (Transform child in InfoContent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void EnterDeleteMode()
    {
        //if (canFollowCamera == false)
        //{
            ObjectMesh.GetComponent<Renderer>().material = DeleteMaterial;

            canDelete = true;

            Debug.Log("[Player] " + transform.name + " enter delete mode");
        //}
    }

    public void ExitDeleteMode()
    {
        ObjectMesh.GetComponent<Renderer>().material = RealisticMaterial;

        canDelete = false;
    }

    public void ResetAll()
    {
        ExitDeleteMode();

        HideInfoContact();
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
        //if (canFollowCamera == true)
        //{
        //    if(TargetPosition != transform.position)
        //    {
        //        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, 0.025f);
        //    } else
        //    {
        //        canFollowCamera = false;
        //    }
        //}
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

        ObjectBorder.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exit object: " + transform.name);

        ObjectBorder.SetActive(false);
    }
}
