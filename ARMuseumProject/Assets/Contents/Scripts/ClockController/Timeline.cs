using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;

[Serializable]
public class Dynasty
{
    public int founded;
    public string name;
}

public class Timeline : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private GameObject timeline;
    [SerializeField] private TextMeshProUGUI dynastyComp;
    [SerializeField] private TextMeshProUGUI dateComp;
    [SerializeField] private ParticleSystem ringParticleComp;
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private Dynasty[] dynastyChronology;
    private int currentDynastyIndex;

    void Start()
    {
        _clockController.dateMessageListener += DateMessageHandler;
        _clockController.startEventListener += StartEventHandler;
        _clockController.stopEventListener += StopEventHandler;
        _clockController.loadEventListener += LoadEventHandler;
        _clockController.unloadEventListener += UnloadEventHandler;

        Reset();
    }

    private void Reset()
    {
        timeline.SetActive(false);
        DateMessageHandler(-2500, 0);
        SetTextColor(new Color(1, 1, 1, 0));
        currentDynastyIndex = 0;
    }

    private void LoadEventHandler()
    {
        timeline.SetActive(true);
    }

    private async void UnloadEventHandler()
    {
        StartCoroutine(fadeOutDuration.Tweeng((t) =>
        {
            SetTextColor(new Color(1, 1, 1, t));
        }, 1f, 0f));

        await UniTask.Delay(TimeSpan.FromSeconds(fadeOutDuration), ignoreTimeScale: false);

        Reset();
    }

    private void StartEventHandler()
    {
        StartCoroutine(fadeInDuration.Tweeng((t) =>
        {
            SetTextColor(new Color(1, 1, 1, t));
        }, 0f, 1f));
    }

    private void SetTextColor(Color color)
    {
        dynastyComp.color = color;
        dateComp.color = color;
    }

    private void StopEventHandler()
    {
        ringParticleComp.Play();
    }

    private void DateMessageHandler(int date, float percentage)
    {
        UpdateDate(date);
        UpdateDynasty(date);
    }

    private void UpdateDate(int date)
    {
        dateComp.text = string.Format("公元{0}{1}年", date < 0 ? "前" : "", Mathf.Abs(date));
    }

    private void UpdateDynasty(int date)
    {
        if(currentDynastyIndex >= dynastyChronology.Length)
        {
            return;
        }

        if(dynastyChronology[currentDynastyIndex].founded <= date)
        {
            dynastyComp.text = dynastyChronology[currentDynastyIndex].name;
            currentDynastyIndex++;
        }
    }
}
