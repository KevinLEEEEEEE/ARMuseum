using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using DG.Tweening;

public class GrabbableExhibit : MonoBehaviour
{
    [SerializeField] private OrbButton deleteOrb;
    [SerializeField] private GameObject mesh;
    [SerializeField] private GameObject Orbs;
    [SerializeField] private Material emissiveMaterial;
    private Transform CenterAnchor
    {
        get
        {
            return NRSessionManager.Instance.CenterCameraAnchor;
        }
    }

    private void Start()
    {
        deleteOrb.orbButtonStartEvent += DeleteStart;
        deleteOrb.orbButtonStopEvent += DeleteStop;
        deleteOrb.orbButtonFinishEvent += DeleteComplete;
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

        DOTween.To((p) =>
        {
            emissiveMaterial.SetColor("_EmissionColor", new Color(0.2f, 0, 0) * p);
        }, 2.5f, 0, 0.6f);
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
        Vector3 endPoint = CenterAnchor.position + CenterAnchor.forward * 0.4f;

        StartCoroutine(.4f.Tweeng((p) => {
            transform.position = p;
            transform.LookAt(2 * transform.position - CenterAnchor.transform.position);
        }, startPoint, endPoint));
    }
}


