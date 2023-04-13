using Cysharp.Threading.Tasks;
using NRKernal.NRExamples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameController_Tutorial : MonoBehaviour
{
    [SerializeField] private InstructionGenerator m_InstructionGenerator;
    [SerializeField] private OrbButton m_OrbButton;
    [SerializeField] private VideoClip pinchGestureClip;
    [SerializeField] private VideoClip pointGestureClip;
    [SerializeField] private AudioClip fadeInClip;
    [SerializeField] private AudioClip firstStepClip;
    [SerializeField] private AudioClip secondStepClip;
    [SerializeField] private GameObject videoPlayRoot;
    [SerializeField] private RenderTexture renderTexture;

    private MoveWithCamera m_MoveWithCamera;
    private VideoPlayer videoPlayerComp;
    private Animator animatorComp;
    private AudioGenerator fadeInPlayer;
    private AudioGenerator firstStepPlayer;
    private AudioGenerator secondStepPlayer;

    void Awake()
    {
        fadeInPlayer = new AudioGenerator(gameObject, fadeInClip, false, false, 0.5f);
        firstStepPlayer = new AudioGenerator(gameObject, firstStepClip);
        secondStepPlayer = new AudioGenerator(gameObject, secondStepClip);
        animatorComp = transform.GetComponent<Animator>();
        videoPlayerComp = transform.GetComponent<VideoPlayer>();
        m_MoveWithCamera = transform.GetComponent<MoveWithCamera>();

        m_OrbButton.orbButtonFinishEvent += ThirdStep;

        videoPlayRoot.SetActive(false);
    }

    private void Start()
    {
        FirstStep();
    }

    private async void FirstStep()
    {
        fadeInPlayer.Play();
        animatorComp.Play("Step01");

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        firstStepPlayer.Play();
        m_MoveWithCamera.enabled = false;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
        fadeInPlayer.SetVolumeInSeconds(0.1f, 10);

        await UniTask.Delay(TimeSpan.FromSeconds(18), ignoreTimeScale: false);

        m_InstructionGenerator.GenerateInstruction("如何激活按钮", "五指张开，指向目标，做「捏住-松开」动作");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        StartVideo(pinchGestureClip);
    }

    public async void SecondStep()
    {
        StopVideo();
        m_InstructionGenerator.HideInstruction();
        animatorComp.Play("Step02");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        secondStepPlayer.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);
        
        m_MoveWithCamera.enabled = true;

        await UniTask.Delay(TimeSpan.FromSeconds(13), ignoreTimeScale: false);

        m_InstructionGenerator.GenerateInstruction("如何激活圆球", "伸出食指触碰圆球，并保持2秒");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        StartVideo(pointGestureClip);
    }

    private async void ThirdStep()
    {
        StopVideo();
        m_InstructionGenerator.HideInstruction();
        animatorComp.Play("Step03");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        SceneManager.LoadScene("BeginScene");
    }

    private void StopVideo()
    {
        videoPlayerComp.Stop();
        videoPlayRoot.SetActive(false);
    }

    private void StartVideo(VideoClip clip)
    {
        renderTexture.Release();
        videoPlayerComp.clip = clip; 
        videoPlayerComp.Play();
        videoPlayRoot.SetActive(true);
    }
}
