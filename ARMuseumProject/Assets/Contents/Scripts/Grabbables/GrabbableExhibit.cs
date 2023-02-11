using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;


public class GrabbableExhibit : MonoBehaviour
{
    public GameObject mesh;
    public GameObject Orbs;
    public Material emissiveMaterial;
    private Transform centerAnchor
    {
        get
        {
            return NRSessionManager.Instance.CenterCameraAnchor;
        }
    }

    private void Start()
    {
        //emissiveMaterial = mesh.GetComponent<MeshRenderer>().material;
    }

    private void OnEnable()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        HideOrbs();
        DeleteStop();
    }

    public void DeleteStart()
    {
        emissiveMaterial.EnableKeyword("_EMISSION");
        StartCoroutine(.6f.Tweeng((p) =>
        {
            emissiveMaterial.SetColor("_EmissionColor", new Color(0.2f, 0, 0) * p);
        }, 2.5f, 0));  
    }

    public void DeleteStop()
    {
        emissiveMaterial.DisableKeyword("_EMISSION");
        emissiveMaterial.SetColor("_EmissionColor", new Color(0.2f, 0, 0) * 2.5f);
    }

    public void DeleteComplete()
    {
        DeleteStop();
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


