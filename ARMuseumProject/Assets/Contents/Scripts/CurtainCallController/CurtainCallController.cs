using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;

[Serializable]
public class CurtainCallObject
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float delayBefore;
    [SerializeField] private float textFadeInDuration;

    public void Reset()
    {
        text.color = new Color(1, 1, 1, 0);
    }

    public async UniTask<bool> Active()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delayBefore), ignoreTimeScale: false);

        text.DOColor(new Color(1, 1, 1, 1), textFadeInDuration);

        if(particle != null)
        {
            particle.Play();
        }

        await UniTask.Delay(TimeSpan.FromSeconds(textFadeInDuration), ignoreTimeScale: false);

        return true;
    }
}

public class CurtainCallController : MonoBehaviour
{
    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private GameObject Root;
    [SerializeField] private TextMeshProUGUI countdownTextComp;
    [SerializeField] private string countdownTextFormat;
    [SerializeField] private int countdownDuration;
    [SerializeField] private CurtainCallObject[] curtainCalls;

    private void Start()
    {
        Reset();
    }

    private void Reset()
    {
        Root.SetActive(false);

        for (int i = 0; i < curtainCalls.Length; i++)
        {
            curtainCalls[i].Reset();
        }
    }

    public void Init()
    {
        BeginScene();
    }

    private async void BeginScene()
    {
        Root.SetActive(true);
        _gameController.StopAmbientSound();

        for (int i = 0; i < curtainCalls.Length; i++)
        {
            await curtainCalls[i].Active();
        }

        await CountingDown();

        _gameController.NextScene();
    }

    private async UniTask<bool> CountingDown()
    {
        int count = countdownDuration;

        while(count >= 0)
        {
            countdownTextComp.text = string.Format(countdownTextFormat, count--);

            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);
        }

        return true;
    }
}
