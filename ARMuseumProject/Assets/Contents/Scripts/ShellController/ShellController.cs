using System.Collections;
using System.Collections.Generic;
using AmazingAssets.AdvancedDissolve;
using UnityEngine;
using NRKernal;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class ShellController : MonoBehaviour
{
    public ImageRecogResultListener imageRecogResultListener;

    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private DialogGenerator dialogGenerator;
    [SerializeField] private InstructionGenerator instructionGenerator;
    [SerializeField] private ShellMatchManager m_matchManager;
    [SerializeField] private AudioClip clip_AncientAmbient;
    [SerializeField] private AudioClip clip_shellFadeIn;
    [SerializeField] private AudioClip clip_shellBurning;
    [SerializeField] private AudioClip clip_shellCasting;
    [SerializeField] private float objectDetectionFrequency;
    [SerializeField] private bool lockToBurningState;

    private CameraManager cameraManager;
    private Animator animatorComp;
    private HandState rightHandState;
    private HandState leftHnadState;
    private ImageRecognition imageRecognition;
    private ImageRecogResult latestRecogResult;
    private AudioGenerator source_shellFadeIn;
    private AudioGenerator source_shellBurning;
    private AudioGenerator source_AncientAmbient;
    private AudioGenerator source_shellCasting;
    private bool canDetectObject;
    private bool hasObjectDetectedBefore;
    private bool isObjectFound;

    void Awake()
    {
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHnadState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        imageRecognition = transform.GetComponent<ImageRecognition>();
        cameraManager = transform.GetComponent<CameraManager>();
        animatorComp = transform.GetComponent<Animator>();

        Reset();
    }

    private void Reset()
    {
        isObjectFound = false;
        canDetectObject = true;
        hasObjectDetectedBefore = false;
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

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        source_shellFadeIn = new AudioGenerator(gameObject, clip_shellFadeIn);
        source_shellBurning = new AudioGenerator(gameObject, clip_shellBurning, true, false, 0);
        source_AncientAmbient = new AudioGenerator(gameObject, clip_AncientAmbient, true);
        source_shellCasting = new AudioGenerator(gameObject, clip_shellCasting);
        source_AncientAmbient.SetPinch(0.9f);
        source_shellCasting.SetPinch(0.92f);

        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

        // 播放容器生成动画
        SetRootsActive(true);
        SetAnimatorEnable(true);
        source_shellFadeIn.Play();
        animatorComp.Play("ShellFadeIn");

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        _gameController.SetAmbientLightInSeconds(0.1f, 4);
        _gameController.SetEnvLightIntensityInSeconds(0.25f, 4);

        // 等待生成动画结束
        await UniTask.Delay(TimeSpan.FromSeconds(6), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("铸模已形成......");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("升高温度直至铜水融化");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // 启动蒙层，遮挡火焰底部
        _gameController.ShowGroundMask();

        // 启动燃烧动画
        animatorComp.Play("ShellBurning");
        source_shellBurning.Play();
        source_shellBurning.SetVolumeInSeconds(0.2f, 15);

        await UniTask.Delay(TimeSpan.FromSeconds(4), ignoreTimeScale: false);

        // 给出下一步操作提示
        instructionGenerator.GenerateInstruction("加速升温", "拾起火柴并划开，\n可以「加速」升温");

        // 启动物体识别服务
        StartObjectDetectionLoop();
    }

    private async void StartObjectDetectionLoop()
    {
        while (canDetectObject)
        {
            if (rightHandState.isPinching || leftHnadState.isPinching)
            {
                if(lockToBurningState)
                {
                    TargetObjectFound();
                } else
                {
                    imageRecognition.TakePhotoAndAnalysis(UpdateImageRecogResult);
                }
            } else
            {
                TargetObjectLost();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(objectDetectionFrequency), ignoreTimeScale: false);
        }
    }

    public void ShellCastingStart()
    {
        NRDebugger.Info("[ShellController] Shell start casting");

        // 结束图像识别服务
        cameraManager.Close();
        canDetectObject = false;

        // 将动画速度缓步调整至快速并关闭火柴模组
        FastSpeedMode();
        m_matchManager.PutOutMatch();
        StopAncientAmbientSound();

        // 恢复skebox环境光，但不需要恢复ambientlight，下一个章节不需要该光源
        _gameController.SetEnvLightIntensityInSeconds(1.3f, 7);

        source_shellBurning.SetVolumeInSeconds(0, 6);
        source_shellCasting.Play();
        animatorComp.Play("ShellCasting");
    }

    public async void ShellCastingFinish()
    {
        dialogGenerator.GenerateDialog("青铜器浇筑完成");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        Reset();

        _gameController.HideGroundMask();
        _gameController.NextScene();
    }

    public void UpdateImageRecogResult(ImageRecogResult res)
    {
        if (!canDetectObject)
            return;

        if (latestRecogResult != null && res.GetStartTime() < latestRecogResult.GetStartTime())
        {
            NRDebugger.Info("[ShellController] Earlier recog result received, skip it.");
            return;
        }

        if (res.IsSuccessful())
        {
            // 如果请求成功，则根据结果决定火柴组件是否启用
            if (res.ContainLabel("burning"))
            {
                NRDebugger.Info(string.Format("[ShellController] Match detected, Receive result in {0} ms.", res.GetCostTime()));
                TargetObjectFound();
            }
            else
            {
                NRDebugger.Info(string.Format("[ShellController] Match undetected, Receive result in {0} ms.", res.GetCostTime()));
                TargetObjectLost();
            }

            latestRecogResult = res;
        }
        else
        {
            // 如果请求失败，则保持先前的火柴状态
            NRDebugger.Info("[ShellController] Image detection request failed.");
        }

        // 如果debug系统监听了图像识别结果事件，则同步并显示结果
        imageRecogResultListener?.Invoke(res);
    }

    private void TargetObjectFound()
    {
        if (isObjectFound)
            return;

        if(!hasObjectDetectedBefore)
        {
            instructionGenerator.HideInstruction();
            hasObjectDetectedBefore = true;
        }

        isObjectFound = true;
        FastSpeedMode();
        PlayAncientAmbientSound();
        m_matchManager.LightUpMatch();
    }

    private void TargetObjectLost()
    {
        if (!isObjectFound)
            return;

        isObjectFound = false;
        NormalSpeedMode();
        PauseAncientAmbientSound();
        m_matchManager.PutOutMatch();
    }

    private void PlayAncientAmbientSound()
    {
        source_AncientAmbient.Play();
        source_AncientAmbient.SetVolumeInSeconds(1, 2);
    }

    private void PauseAncientAmbientSound()
    {
        source_AncientAmbient.SetVolumeInSeconds(0, 2).OnComplete(() => {
            if(!isObjectFound)
                source_AncientAmbient.Pause();
        });
    }

    private void StopAncientAmbientSound()
    {
        source_AncientAmbient.SetVolumeInSeconds(0, 2).OnComplete(() => {
            source_AncientAmbient.Stop();
        });
    }

    private void FastSpeedMode()
    {
        DOTween.To(() => animatorComp.GetFloat("SpeedMultiplier"), (t) => {
            animatorComp.SetFloat("SpeedMultiplier", t);
        }, 1, 0.6f);
    }

    private void NormalSpeedMode()
    {
        DOTween.To(() => animatorComp.GetFloat("SpeedMultiplier"), (t) => {
            animatorComp.SetFloat("SpeedMultiplier", t);
        }, 0.1f, 0.6f);
    }

    public delegate void ImageRecogResultListener(ImageRecogResult result);
}
