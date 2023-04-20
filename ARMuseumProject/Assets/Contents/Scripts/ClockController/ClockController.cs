using Cysharp.Threading.Tasks;
using NRKernal;
using System;
using UnityEngine;
using UnityEngine.Video;
using AmazingAssets.AdvancedDissolve;
using DG.Tweening;

public enum ClockState
{
    Stop,
    Pause,
    Playing,
    Fading,
}

public class ClockController : MonoBehaviour
{
    [SerializeField] private GameController_Historical m_GameController;
    [SerializeField] private DialogGenerator m_DialogGenerator;
    [SerializeField] private InstructionGenerator m_InstructionGenerator;
    [SerializeField] private AdvancedDissolvePropertiesController m_DissolveController;
    [SerializeField] private GameObject fractureRoot;
    [SerializeField] private RenderTexture renderTexComp;
    [SerializeField] private VideoPlayer videoPlayerComp;
    [SerializeField] private AudioSource audioSourceComp;
    [SerializeField] private AudioClip fadeOutClip01;
    [SerializeField] private AudioClip fadeOutClip02;

    private AudioGenerator fadeOutPlayer01;
    private AudioGenerator fadeOutPlayer02;
    private HandState rightHandState;
    private HandState leftHandState;
    private Animator animatorComp;
    private ClockState state;
    private float defaultVolume;

    void Awake()
    {
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        animatorComp = transform.GetComponent<Animator>();

        Reset();
    }

    public void Reset()
    {
        state = ClockState.Stop;
        defaultVolume = audioSourceComp.volume;
        renderTexComp.Release();

        SetRootsActive(false);
        SetAnimatorEnable(false);
    }

    private void SetRootsActive(bool state)
    {
        foreach (Transform trans in transform)
            trans.gameObject.SetActive(state);
    }

    private void SetAnimatorEnable(bool state)
    {
        animatorComp.enabled = state;
    }

    public async void Init()
    {
        fadeOutPlayer01 = new AudioGenerator(gameObject, fadeOutClip01);
        fadeOutPlayer02 = new AudioGenerator(gameObject, fadeOutClip02);

        await UniTask.NextFrame();

        SetRootsActive(true);
        SetAnimatorEnable(true);

        m_DialogGenerator.GenerateDialog("它将见证华夏文明千年历程");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        m_InstructionGenerator.GenerateInstruction("握拳暂停", "伸手握拳「暂停」\n松开拳头「恢复」", 7);

        await UniTask.Delay(TimeSpan.FromSeconds(6), ignoreTimeScale: false);

        state = ClockState.Playing;
        videoPlayerComp.Play();
        animatorComp.Play("ClockFadeIn");
    }

    public async void ClockFadeInComplete()
    {
        state = ClockState.Stop;

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        fadeOutPlayer01.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(1.4), ignoreTimeScale: false);

        m_DialogGenerator.GenerateDialog("青铜的光辉已然暗淡");
        m_GameController.SetEnvLightIntensityInSeconds(0.2f, 7f);

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration - 0.5f), ignoreTimeScale: false);

        fadeOutPlayer02.Play();

        await UniTask.Delay(TimeSpan.FromSeconds(1.45), ignoreTimeScale: false);

        m_DialogGenerator.GenerateDialog("亘古的记忆化为尘土");
        m_GameController.ShowGroundMask();
        animatorComp.Play("ClockFadeOut");

        // 1s后碎块逐渐消散
        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

        DOTween.To((t) =>
        {
            m_DissolveController.cutoutStandard.clip = t;
        }, 0, 1, 6);
    }

    public async void ClockFadeOutComplete()
    {
        m_GameController.StopAmbientSound();
        m_GameController.NextScene();

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        // 等待粒子消散
        Reset();
    }

    private void Update()
    {
        if(state != ClockState.Stop && state != ClockState.Fading)
        {
            if(leftHandState.currentGesture == HandGesture.Grab || rightHandState.currentGesture == HandGesture.Grab)
            {
                if(state == ClockState.Playing)
                {
                    state = ClockState.Fading;
                    audioSourceComp.DOFade(0, 0.5f).OnComplete(() =>
                    {
                        state = ClockState.Pause;
                        videoPlayerComp.Pause();
                        animatorComp.speed = 0;
                    });
                }
            } else
            {
                if (state == ClockState.Pause)
                {
                    state = ClockState.Fading;
                    videoPlayerComp.Play();
                    animatorComp.speed = 1;
                    audioSourceComp.DOFade(defaultVolume, 1).OnComplete(() =>
                    {
                        state = ClockState.Playing;
                    });
                }
            }
        }
    }
}
