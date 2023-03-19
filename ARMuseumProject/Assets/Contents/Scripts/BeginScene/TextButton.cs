using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Menu m_Menu;
    [SerializeField] private bool enableButton = false;

    private TextMeshProUGUI textMeshComp;

    void Start()
    {
        textMeshComp = transform.GetComponentInChildren<TextMeshProUGUI>();   
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableButton) return;
        m_Menu.PlayButtonHoverAudio();
        textMeshComp.transform.DOScale(1.35f, 0.15f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enableButton) return;

        textMeshComp.transform.DOScale(1, 0.15f);
    }
}
