using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using NRKernal;

public class Orb_Switch : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    [FormerlySerializedAs("GameController")]
    private GameController _GameController;

    //public float ToggleDuration = 2f;
    public Sprite SeeSprite;
    public Sprite UnseeSprite;
    private MeshRenderer _MeshRenderer;
    private SpriteRenderer _SpriteRenderer;

    private bool canSee = false;

    void Start()
    {
        _MeshRenderer = transform.GetComponent<MeshRenderer>();
        _SpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
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

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSee();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _MeshRenderer.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _MeshRenderer.enabled = false;
    }
}
