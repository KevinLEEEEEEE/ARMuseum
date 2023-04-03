using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainPageButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameController m_gameController;

    private MeshRenderer meshRendererComp;
    private Animation animatorComp;
    private Transform root;
    private bool isPointerDown = false;
    private bool freezeAfterPointerDown = false;
    private bool canDetectRaycast = true;

    void Start()
    {
        meshRendererComp = transform.GetComponent<MeshRenderer>();
        animatorComp = transform.GetComponent<Animation>();

        //m_gameController.StartRaycastEvent += StartRaycastDetection;
        //m_gameController.StopRaycastEvent += StopRayastDetection;

        root = transform.GetChild(0);

        meshRendererComp.enabled = false;
    }

    public void StartRaycastDetection()
    {
        canDetectRaycast = true;
    }

    public void StopRayastDetection()
    {
        canDetectRaycast = false;
        meshRendererComp.enabled = false;

        if (isPointerDown)
        {
            freezeAfterPointerDown = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canDetectRaycast)
        {
            if (freezeAfterPointerDown)
            {
                freezeAfterPointerDown = false;
            }
            else
            {
                animatorComp.Play();
                isPointerDown = false;

                //m_gameController.LoadMainScene();

                // »ØÍË
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        freezeAfterPointerDown = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canDetectRaycast)
        {
            meshRendererComp.enabled = true;
            root.DOScale(1.2f, 0.15f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (canDetectRaycast)
        {
            meshRendererComp.enabled = false;
            root.DOScale(1, 0.15f);
        }
    }
}
