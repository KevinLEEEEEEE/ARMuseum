using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using NRKernal;

public class Orb_Switch : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    [FormerlySerializedAs("GameController")]
    private GameController _GameController;

    public HandCoach _HandCoach;
    public Sprite SeeSprite;
    public Sprite UnseeSprite;
    private MeshRenderer _MeshRenderer;
    private SpriteRenderer _SpriteRenderer;
    private Animation _Animation;

    private bool isFirstUse = true;
    private bool isPointerDown = false;
    private bool isFirstClickAfterRaycastStart = false;
    private bool canDetectRaycast = true;
    private bool canSee = false;

    void Start()
    {
        _MeshRenderer = transform.GetComponent<MeshRenderer>();
        _SpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _Animation = transform.GetComponent<Animation>();
        _MeshRenderer.enabled = false;
    }

    private void ToggleSee()
    {
        canSee = !canSee;

        if(canSee)
        {
            _SpriteRenderer.sprite = SeeSprite;
            _GameController.BeginTour();
        } else
        {
            _SpriteRenderer.sprite = UnseeSprite;
            _GameController.EndTour();
        }
    }

    public void StartRaycastDetection()
    {
        canDetectRaycast = true;
    }

    public void StopRayastDetection()
    {
        canDetectRaycast = false;
        isFirstClickAfterRaycastStart = true;
        _MeshRenderer.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(isFirstUse)
        {
            isFirstUse = false;
            _HandCoach.StopHintLoop();
        }

        if(canDetectRaycast)
        {
            if(isPointerDown && isFirstClickAfterRaycastStart)
            {
                isFirstClickAfterRaycastStart = false;
                return;
            }

            _Animation.Play();
            isPointerDown = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canDetectRaycast)
        {
            _MeshRenderer.enabled = true;
        }  
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _MeshRenderer.enabled = false;
    }
}
