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
    [SerializeField] private GameController_Historical m_gameController;
    [SerializeField] private DialogGenerator m_dialogGenerator;
    [SerializeField] private InstructionGenerator m_instructionGenerator;
    [SerializeField] private AdvancedDissolvePropertiesController m_DissolveController;
    [SerializeField] private GameObject fractureRoot;
    [SerializeField] private RenderTexture renderTexComp;
    [SerializeField] private VideoPlayer videoPlayerComp;
    [SerializeField] private AudioSource audioSourceComp;

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
        await UniTask.NextFrame();

        SetRootsActive(true);
        SetAnimatorEnable(true);

        m_dialogGenerator.GenerateDialog("它见证了华夏文明的历程");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        m_instructionGenerator.GenerateInstruction("握拳暂停", "伸手握拳「暂停」\n松开拳头「恢复」", 5);

        await UniTask.Delay(TimeSpan.FromSeconds(6.5), ignoreTimeScale: false);

        state = ClockState.Playing;
        videoPlayerComp.Play();
        animatorComp.Play("ClockFadeIn");
    }

    public async void ClockFadeInComplete()
    {
        state = ClockState.Stop;

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        m_dialogGenerator.GenerateDialog("青铜时代的光辉已然暗淡");
        m_gameController.SetEnvLightIntensityInSeconds(0.2f, 7f);

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        m_dialogGenerator.GenerateDialog("亘古的记忆化为尘土");
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
        m_gameController.NextScene();

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
