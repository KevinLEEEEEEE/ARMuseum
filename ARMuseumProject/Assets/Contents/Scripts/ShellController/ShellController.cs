using System.Collections;
using System.Collections.Generic;
using AmazingAssets.AdvancedDissolve;
using UnityEngine;
using NRKernal;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public enum ShellNode
{
    Load,
    Start,
    ToBurn,
    Burning,
    Burnout,
    Stop,
}

public class ShellController : MonoBehaviour
{
    public LoadEventListener loadEventListener;
    public UnloadEventListener unloadEventListener;
    public StartEventListener startEventListener;
    public ShellStateListener shellStateListener;
    public StopEventListener stopEventListener;
    public ImageRecogResultListener imageRecogResultListener;

    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private DialogGenerator dialogGenerator;
    [SerializeField] private InstructionGenerator instructionGenerator;
    [SerializeField] private ShellMatchManager _matchManager;

    [SerializeField] private AdvancedDissolveGeometricCutoutController cutoutController_front;
    [SerializeField] private AdvancedDissolveGeometricCutoutController cutoutController_back;
    [SerializeField] private GameObject ADController;
    
    [SerializeField] private GameObject burningPoint;
    [SerializeField] private float burningDuration;
    [SerializeField] private float burnoutRadius;
    [SerializeField] private AnimationCurve burningRadiusCurve;

    [SerializeField] private float objectDetectionFrequency;
    [SerializeField] private bool lockToBurningState;

    private HandState rightHandState;
    private HandState leftHnadState;
    private BoxCollider boxCollierComp;
    private CameraManager cameraManager;
    private ImageRecognition imageRecognition;
    private ImageRecogResult latestRecogResult;
    private Action hideMatchInstruction;
    private bool canDetectObject;

    void Start()
    {
        imageRecognition = transform.GetComponent<ImageRecognition>();
        cameraManager = transform.GetComponent<CameraManager>();
        boxCollierComp = transform.GetComponent<BoxCollider>();
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHnadState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        Reset();
    }

    private void Reset()
    {
        canDetectObject = true;
        boxCollierComp.enabled = false;
        burningPoint.transform.position = new Vector3(0, 10, 0);
        ADController.SetActive(false);
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        // 广播加载事件
        loadEventListener?.Invoke();
        shellStateListener?.Invoke(ShellNode.Load);
        ADController.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        // 广播开始事件，播放容器生成动画
        startEventListener?.Invoke();
        shellStateListener?.Invoke(ShellNode.Start);

        // 等待生成动画结束
        await UniTask.Delay(TimeSpan.FromSeconds(8), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("铸模已形成......");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("现在，点燃熔融之火");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // 给出下一步操作提示
        hideMatchInstruction = instructionGenerator.GenerateInstruction("引燃火柴", "将火苗「缓慢」靠近并触碰模型");

        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

        // 启动物体识别服务
        boxCollierComp.enabled = true;
        shellStateListener?.Invoke(ShellNode.ToBurn);
        StartObjectDetection();
    }

    private async void StartObjectDetection()
    {
        while (canDetectObject)
        {
            if (rightHandState.isPinching || leftHnadState.isPinching)
            {
                NRDebugger.Info("[ShellController] Detected pinch gesture, start taking photo.");

                if(lockToBurningState)
                {
                    _matchManager.LightUpMatch();
                } else
                {
                    imageRecognition.TakePhotoAndAnalysis(UpdateImageRecogResult);
                }
            } else
            {
                _matchManager.PutOutMatch();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(objectDetectionFrequency), ignoreTimeScale: false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 注意，此处绑定了检测对象
        if (other.gameObject.name == _matchManager.gameObject.name)
        {
            StartBurningProgress(other.bounds.ClosestPoint(transform.position));
        } else
        {
            NRDebugger.Info("[ShellController] Trigger enter but that target is not Match");
        }
    }

    private async void StartBurningProgress(Vector3 point)
    {
        NRDebugger.Info("[ShellController] Shell start burning");

        // 结束图像识别服务
        cameraManager.Close();
        canDetectObject = false;
        boxCollierComp.enabled = false;

        // 结束使用火柴提示
        hideMatchInstruction();
        hideMatchInstruction = null;

        // 启动蒙层，遮挡火焰底部
        _gameController.ShowGroundMask();

        // 广播燃烧状态
        shellStateListener?.Invoke(ShellNode.Burning);

        // 启动燃烧动画
        BurningAtPoint(point);

        // 等待容器燃烧完成
        await UniTask.Delay(TimeSpan.FromSeconds(burningDuration), ignoreTimeScale: false);

        NRDebugger.Info("[ShellController] Shell burnout");

        // 广播燃烧结束状态
        shellStateListener?.Invoke(ShellNode.Burnout);

        await UniTask.NextFrame();

        dialogGenerator.GenerateDialog("青铜器浇筑完成");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // 结束
        stopEventListener?.Invoke();
        shellStateListener?.Invoke(ShellNode.Stop);
        Reset();

        _gameController.HideGroundMask();
        _gameController.NextScene();
    }

    private void BurningAtPoint(Vector3 point)
    {
        burningPoint.transform.position = point;

        DOTween.To(() => 0, r => {
            float radius = burningRadiusCurve.Evaluate(r / burnoutRadius) * burnoutRadius;
            cutoutController_front.target1Radius = radius;
            cutoutController_back.target1Radius = radius;
        }, burnoutRadius, burningDuration);
    }

    public void UpdateImageRecogResult(ImageRecogResult res)
    {
        if (latestRecogResult != null && res.GetStartTime() < latestRecogResult.GetStartTime())
        {
            NRDebugger.Info("[ShellController] Earlier recog result received, skip it.");
            return;
        }

        if (res.IsSuccessful())
        {
            // 如果请求成果，则根据结果决定火柴组件是否启用
            if (res.ContainLabel("burning"))
            {
                NRDebugger.Info(string.Format("[ShellController] Match detected, Receive result in {0} ms.", res.GetCostTime()));
                _matchManager.LightUpMatch();
            }
            else
            {
                NRDebugger.Info(string.Format("[ShellController] Match undetected, Receive result in {0} ms.", res.GetCostTime()));
                _matchManager.PutOutMatch();
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

    public delegate void StartEventListener();
    public delegate void StopEventListener();
    public delegate void LoadEventListener();
    public delegate void UnloadEventListener();
    public delegate void ShellStateListener(ShellNode state);
    public delegate void ImageRecogResultListener(ImageRecogResult result);
}
