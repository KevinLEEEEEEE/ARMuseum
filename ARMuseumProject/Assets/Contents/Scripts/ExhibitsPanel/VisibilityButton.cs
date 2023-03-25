using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using NRKernal;
using TMPro;
using DG.Tweening;

public class VisibilityButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    [FormerlySerializedAs("GameController")]
    private GameController gameController;

    [SerializeField]
    [FormerlySerializedAs("InteractionHint_Pinch")]
    private InteractionHint interactionHint;

    [SerializeField] private SpriteRenderer spriteRendererComp;
    [SerializeField] private Sprite visibleSprite;
    [SerializeField] private Sprite invisibleSprite;
    [SerializeField] private TextMeshProUGUI textMeshComp;
    [SerializeField] private string visibleText;
    [SerializeField] private string invisibleText;

    private MeshRenderer meshRenderer;
    private Animation animator;
    private Transform root;
    private bool isHandCoached_Pinch = false; // 不需要重置
    private bool isPointerDown = false;
    private bool freezeAfterPointerDown = false;
    private bool canDetectRaycast = true;
    private bool visibility = false;

    void Start()
    {
        meshRenderer = transform.GetComponent<MeshRenderer>();
        animator = transform.GetComponent<Animation>();
        root = transform.GetChild(0);

        gameController.StartRaycastEvent += StartRaycastDetection;
        gameController.StopRaycastEvent += StopRayastDetection;
        gameController.FoundObserverEvent += Found;

        interactionHint.gameObject.SetActive(true);
        meshRenderer.enabled = false;
    }

    private void Found()
    {
        if(!isHandCoached_Pinch)
        {
            interactionHint.StartHintLoop();
        }  
    }

    // Triggered by animation: OrbClick
    private void ToggleVisibility()
    {
        visibility = !visibility;

        if (visibility)
        {
            spriteRendererComp.sprite = invisibleSprite;
            textMeshComp.text = invisibleText;
            gameController.BeginTour();
        }
        else
        {
            spriteRendererComp.sprite = visibleSprite;
            textMeshComp.text = visibleText;
            gameController.EndTour();
        }
    }

    public void StartRaycastDetection()
    {
        canDetectRaycast = true;
    }

    public void StopRayastDetection()
    {
        canDetectRaycast = false;
        meshRenderer.enabled = false;

        if(isPointerDown)
        {
            freezeAfterPointerDown = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canDetectRaycast)
        {
            if (!isHandCoached_Pinch)
            {
                isHandCoached_Pinch = true;
                interactionHint.StopHintLoop();
            }

            if (freezeAfterPointerDown)
            {
                freezeAfterPointerDown = false;
            } else
            {
                animator.Play();
                isPointerDown = false;
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
            meshRenderer.enabled = true;
            root.DOScale(1.15f, 0.15f);
        }   
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (canDetectRaycast)
        {
            meshRenderer.enabled = false;
            root.DOScale(1, 0.15f);
        }
    }
}
