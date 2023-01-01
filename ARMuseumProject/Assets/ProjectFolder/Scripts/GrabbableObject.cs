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
    private bool isDeleting = false;

    private void OnEnable()
    {
        InitGameObject();

        LookAtCamera();

        ExitDeleteMode();

        HideInfoContact();

        canFollowCamera = true;
        canDelete = false;
        isDeleting = false;
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
        if(canDelete == true && !isDeleting)
        {
            Debug.Log("[Player] delete object: " + transform.name);

            isDeleting = true;

            transform.GetComponent<Animation>().Play("DeleteExhibits");

            transform.GetComponent<AudioSource>().Play();

            Invoke("DeleteSelf", 0.84f);
        }
    }

    private void DeleteSelf()
    {
        transform.GetComponent<Animation>().Stop("DeleteExhibits");

        ObjectMesh.GetComponent<Renderer>().material = RealisticMaterial;

        SendMessageUpwards("InactiveGrabbleItem", transform.gameObject);
    }

    public void ShowInfoContact()
    {
        if (canFollowCamera == false && !isDeleting)
        {
            foreach (Transform child in InfoContact.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
    }   

    public void HideInfoContact()
    {
        if (!isDeleting)
        {
            foreach (Transform child in InfoContact.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void HideInfoContent()
    {
        if(isDeleting) {
            return;
        }

        foreach (Transform child in InfoContent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void EnterDeleteMode()
    {
        if (canFollowCamera == false && !isDeleting)
        {
            ObjectMesh.GetComponent<Renderer>().material = DeleteMaterial;

            canDelete = true;

            Debug.Log("[Player] " + transform.name + " enter delete mode");
        }
    }

    public void ExitDeleteMode()
    {
        if(isDeleting)
        {
            return;
        }

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
        if (canFollowCamera == true)
        {
            if (Vector3.Distance(transform.position, TargetPosition) >= 0.02f)
            {
                transform.position = Vector3.MoveTowards(transform.position, TargetPosition, 0.025f);
            }
            else
            {
                canFollowCamera = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pointer click object: " + transform.name);
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
