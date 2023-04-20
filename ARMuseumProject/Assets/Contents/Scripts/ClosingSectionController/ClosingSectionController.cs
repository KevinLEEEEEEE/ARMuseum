using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Cysharp.Threading.Tasks;
using NRKernal;

public class ClosingSectionController : MonoBehaviour
{
    [SerializeField] private GameController_Historical m_GameController;
    [SerializeField] private DialogGenerator m_DialogGenerator;
    [SerializeField] private TextMeshProUGUI userIDComp;
    [SerializeField] private AudioClip fadeInClip;
    [SerializeField] private AudioClip postcardInstructionClip;
    [SerializeField] private AudioClip orbActivationClip;

    private AudioGenerator fadeInPlayer;
    private AudioGenerator postcardInstructionPlayer;
    private AudioGenerator orbActivationPlayer;
    private Animation animationComp;

    private void Awake()
    {
        animationComp = transform.GetComponent<Animation>();

        Reset();
    }

    private void Reset()
    {
        SetRootsActive(false);
    }

    private void SetRootsActive(bool state)
    {
        foreach (Transform trans in transform)
        {
            trans.gameObject.SetActive(state);
        }
    }

    public void Init()
    {
        fadeInPlayer = new AudioGenerator(gameObject, fadeInClip);
        postcardInstructionPlayer = new AudioGenerator(gameObject, postcardInstructionClip);
        orbActivationPlayer = new AudioGenerator(gameObject, orbActivationClip);
        userIDComp.text = m_GameController.UserID;

        FadeIn();
    }

    private async void FadeIn()
    {
        SetRootsActive(true);
        fadeInPlayer.Play();
        animationComp.Play("FadeIn");

        await UniTask.Delay(TimeSpan.FromSeconds(14.7), ignoreTimeScale: false);

        orbActivationPlayer.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(10), ignoreTimeScale: false);

        m_DialogGenerator.GenerateDialog("已经留存你的青铜记忆");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        postcardInstructionPlayer.Play();
        animationComp.Play("Postcard");
        NRInput.RaycastersActive = true;
    }

    public void ReturnToMenu()
    {
        NRInput.RaycastersActive = false;
        m_GameController.HideGroundMask();
        m_GameController.NextScene();
    }
}
