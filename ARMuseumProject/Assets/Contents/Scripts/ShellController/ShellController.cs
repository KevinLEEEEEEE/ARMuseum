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
    [SerializeField] private ShellMatchManager _matchManager;
    [SerializeField] private AudioClip clip_shellFadeIn;
    [SerializeField] private AudioClip clip_shellBurning;
    [SerializeField] private AudioClip clip_shellCasting;
    [SerializeField] private float objectDetectionFrequency;
    [SerializeField] private bool lockToBurningState;
    [SerializeField] private GameObject[] roots;

    private CameraManager cameraManager;
    private Animator animatorComp;
    private HandState rightHandState;
    private HandState leftHnadState;
    private ImageRecognition imageRecognition;
    private ImageRecogResult latestRecogResult;
    private AudioGenerator source_shellFadeIn;
    private AudioGenerator source_shellBurning;
    private AudioGenerator source_shellCasting;
    private bool canDetectObject;
    private bool hasObjectDetectedBefore;

    void Start()
    {
        imageRecognition = transform.GetComponent<ImageRecognition>();
        cameraManager = transform.GetComponent<CameraManager>();
        animatorComp = transform.GetComponent<Animator>();
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHnadState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        source_shellFadeIn = new AudioGenerator(gameObject, clip_shellFadeIn);
        source_shellBurning = new AudioGenerator(gameObject, clip_shellBurning, true, false, 0);
        source_shellCasting = new AudioGenerator(gameObject, clip_shellCasting);
        source_shellCasting.source.pitch = 0.92f;

        Reset();
    }

    private void Reset()
    {
        canDetectObject = true;
        hasObjectDetectedBefore = false;
        HideRoots();
    }

    private void ShowRoots()
    {
        foreach(GameObject obj in roots)
        {
            obj.SetActive(true);
        }
    }

    private void HideRoots()
    {
        foreach (GameObject obj in roots)
        {
            obj.SetActive(false);
        }
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        _gameController.SetAmbientLightInSeconds(0.1f, 2);
        _gameController.SetEnvLightIntensityInSeconds(0.2f, 2);

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        // 广播开始事件，播放容器生成动画
        ShowRoots();
        source_shellFadeIn.Play();
        animatorComp.Play("ShellFadeIn");

        // 等待生成动画结束
        await UniTask.Delay(TimeSpan.FromSeconds(7.5), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("铸模已形成......");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("划开火柴，加速炉温提升");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // 给出下一步操作提示
        instructionGenerator.GenerateInstruction("划开火柴", "划开火柴「加速」炉温上升");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        // 启动蒙层，遮挡火焰底部
        _gameController.ShowGroundMask();

        // 启动燃烧动画
        animatorComp.Play("ShellBurning");
        source_shellBurning.Play();
        source_shellBurning.SetVolumeInSeconds(0.4f, 15);

        // 启动物体识别服务
        StartObjectDetectionLoop();
    }

    private async void StartObjectDetectionLoop()
    {
        while (canDetectObject)
        {
            if (rightHandState.isPinching || leftHnadState.isPinching)
            {
                NRDebugger.Info("[ShellController] Detected pinch gesture, start taking photo.");

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

        // 将动画速度缓步调整至快速
        FastSpeedMode();

        // 恢复光源
        _gameController.SetAmbientLightInSeconds(1.5f, 7);
        _gameController.SetEnvLightIntensityInSeconds(1.3f, 7);

        source_shellBurning.SetVolumeInSeconds(0, 4);
        source_shellCasting.Play();
        animatorComp.Play("ShellCasting");
    }

    public async void ShellCastingFinish()
    {
        dialogGenerator.GenerateDialog("青铜器浇筑完成");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        Reset();

        source_shellFadeIn.Unload();
        source_shellBurning.Unload();
        source_shellCasting.Unload();

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
        if(!hasObjectDetectedBefore)
        {
            instructionGenerator.HideInstruction();
            hasObjectDetectedBefore = true;
        }

        FastSpeedMode();
    }

    private void TargetObjectLost()
    {
        NormalSpeedMode();
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
