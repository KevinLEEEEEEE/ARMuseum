using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using System;

[Serializable]
public class ButtonTransition
{
    public string text;
    public Sprite sprite;
}

public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] bool needTransition = false;
    [SerializeField] ButtonTransition[] transitions;
    [SerializeField] bool enterSoundActive = true;
    [SerializeField] private AudioClip buttonEnterClip;
    [SerializeField] bool exitSoundActive = true;
    [SerializeField] private AudioClip buttonExitClip;
    [SerializeField] bool clickSoundActive = true;
    [SerializeField] private AudioClip buttonClickClip;

    private AudioGenerator buttonEnterPlayer;
    private AudioGenerator buttonExitPlayer;
    private AudioGenerator buttonClickPlayer;
    private Transform root;
    private TextMeshProUGUI textMeshComp;
    private SpriteRenderer spriteComp;
    private Button buttonComp;
    private int index = 0;

    void Start()
    {
        root = transform.GetChild(0);
        buttonComp = transform.GetComponent<Button>();
        textMeshComp = root.GetComponentInChildren<TextMeshProUGUI>();
        spriteComp = root.GetComponentInChildren<SpriteRenderer>();

        if(enterSoundActive)
            buttonEnterPlayer = new AudioGenerator(gameObject, buttonEnterClip);
        if(exitSoundActive)
            buttonExitPlayer = new AudioGenerator(gameObject, buttonExitClip);
        if(clickSoundActive)
            buttonClickPlayer = new AudioGenerator(gameObject, buttonClickClip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!buttonComp.interactable) 
            return;

        if(enterSoundActive)
            buttonEnterPlayer.Play();

        root.DOScale(1.2f, 0.15f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!buttonComp.interactable) 
            return;

        if(exitSoundActive)
            buttonExitPlayer.Play();

        root.DOScale(1, 0.15f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!buttonComp.interactable) 
            return;

        if(clickSoundActive)
            buttonClickPlayer.Play();

        if (needTransition)
        {
            index = index == 0 ? 1 : 0;
            SetContent(transitions[index]);
        }
    }

    private void SetContent(ButtonTransition transition)
    {
        textMeshComp.text = transition.text;
        spriteComp.sprite = transition.sprite;
    }
}
