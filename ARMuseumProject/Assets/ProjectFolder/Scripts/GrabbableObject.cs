using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    public Material DeleteMaterial;
    public Material RealisticMaterial;
    public float ForwardOffset;

    private GameObject ObjectMesh;
    private GameObject InfoContact;
    private GameObject CenterCameraAnchor;
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
        if(ObjectMesh == null)
        {
            ObjectMesh = transform.Find("ObjectMesh").gameObject;
            InfoContact = transform.Find("InfoOrb").gameObject;
            CenterCameraAnchor = GameObject.Find("NRCameraRig/CenterAnchor");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(canDelete && !isDeleting)
        {
            Debug.Log("[Player] delete object: " + transform.name);

            isDeleting = true;
            transform.GetComponent<Animation>().Play("DeleteExhibits");
            SendMessageUpwards("DeleteExhibitsMessage");
        }
    }

    private void DeleteAnimationFinished()
    {
        ObjectMesh.GetComponent<Renderer>().material = RealisticMaterial;
        SendMessageUpwards("InactiveGrabbleItem", transform.gameObject);
    }

    public void ShowInfoContact()
    {
        if (!canFollowCamera && !isDeleting)
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

    public void EnterDeleteMode()
    {
        if (!canFollowCamera && !isDeleting)
        {
            ObjectMesh.GetComponent<Renderer>().material = DeleteMaterial;
            canDelete = true;
        }
    }

    public void ExitDeleteMode()
    {
        if(!isDeleting)
        {
            ObjectMesh.GetComponent<Renderer>().material = RealisticMaterial;
            canDelete = false;
        }
    }

    public void ResetAll()
    {
        ExitDeleteMode();
        HideInfoContact();
    }

    private void LookAtCamera()
    {
        ObjectMesh.transform.LookAt(CenterCameraAnchor.transform);
    }

    void Update()
    {
        if (canFollowCamera)
        {
            Vector3 target = CenterCameraAnchor.transform.position + CenterCameraAnchor.transform.forward * ForwardOffset;

            if (Vector3.Distance(transform.position, target) >= 0.02f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, 0.025f);
            }
            else
            {
                canFollowCamera = false;
            }
        }
    }
}
