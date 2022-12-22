using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class GrabbableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector3 TargetPosition;
    //public Quaternion TargetRotation;
    public Transform TargetTransform;
    public Material DeleteMaterial;
    public Material RealisticMaterial;

    private bool canFollowCamera = true;
    private bool isReadyToDelete = false;

    private void OnEnable()
    {
        transform.LookAt(TargetTransform);

        transform.GetComponent<Renderer>().material = RealisticMaterial;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }

    void Update()
    {
        if (canFollowCamera == true)
        {
            if(TargetPosition != transform.position)
            {
                transform.position = Vector3.MoveTowards(transform.position, TargetPosition, 0.025f);
                //transform.rotation = Quaternion.Slerp(transform.rotation, TargetTransform.rotation, 0.2f);
            } else
            {
                canFollowCamera = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pointer click object: " + transform.name);

        if (isReadyToDelete == false)
        {
            transform.GetComponent<Renderer>().material = DeleteMaterial;
            isReadyToDelete = true;
        } else
        {
            Debug.Log("Delete object: " + transform.name);

            canFollowCamera = true;
            isReadyToDelete = false;
            SendMessageUpwards("InactiveGrabbleItem", transform.name);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer enter object: " + transform.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exit object: " + transform.name);

        if(isReadyToDelete == true)
        {
            transform.GetComponent<Renderer>().material = RealisticMaterial;
            isReadyToDelete = false;
        }
    }
}
