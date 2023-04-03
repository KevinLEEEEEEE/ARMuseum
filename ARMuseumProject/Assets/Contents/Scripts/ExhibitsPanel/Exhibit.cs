using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Exhibit : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public HoverExhibit hoverExhibitEvent;
    public ClickExhibit clickExhibitEvent;

    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material hightlightMaterial;
    [SerializeField] private string exhibitID;

    private BoxCollider colliderComp;
    private GameObject root;
    private MeshRenderer rendererComp;

    private void Awake()
    {
        colliderComp = transform.GetComponent<BoxCollider>();
        root = transform.GetChild(0).gameObject;
        rendererComp = root.GetComponent<MeshRenderer>();

        DisableExhibit();
    }

    public void EnableExhibit()
    {
        root.SetActive(true);
        colliderComp.enabled = true;
        EnableState();
    }

    public void DisableExhibit()
    {
        root.SetActive(false);
        colliderComp.enabled = false;
        DisableState();
    }

    private void HightlightState()
    {
        rendererComp.material = hightlightMaterial;
        root.transform.DOLocalMove(new Vector3(0, 0.1f, 0), 0.5f);
    }

    private void EnableState()
    { 
        rendererComp.material = defaultMaterial;
        root.transform.DOLocalMove(new Vector3(0, 0, 0), 0.5f);
    }

    private void DisableState()
    {
        rendererComp.material = defaultMaterial;
        root.transform.localPosition = new Vector3(0, -0.1f, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        HightlightState();
        hoverExhibitEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EnableState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickExhibitEvent?.Invoke(exhibitID, transform);
    }

    public delegate void HoverExhibit();
    public delegate void ClickExhibit(string exhibitID, Transform trans);
}
