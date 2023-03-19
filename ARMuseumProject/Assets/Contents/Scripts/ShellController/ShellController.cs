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

        // �㲥�����¼�
        loadEventListener?.Invoke();
        shellStateListener?.Invoke(ShellNode.Load);
        ADController.SetActive(true);

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        // �㲥��ʼ�¼��������������ɶ���
        startEventListener?.Invoke();
        shellStateListener?.Invoke(ShellNode.Start);

        // �ȴ����ɶ�������
        await UniTask.Delay(TimeSpan.FromSeconds(8), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("��ģ���γ�......");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("���ڣ���ȼ����֮��");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // ������һ��������ʾ
        hideMatchInstruction = instructionGenerator.GenerateInstruction("��ȼ���", "�����硸����������������ģ��");

        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);

        // ��������ʶ�����
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
        // ע�⣬�˴����˼�����
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

        // ����ͼ��ʶ�����
        cameraManager.Close();
        canDetectObject = false;
        boxCollierComp.enabled = false;

        // ����ʹ�û����ʾ
        hideMatchInstruction();
        hideMatchInstruction = null;

        // �����ɲ㣬�ڵ�����ײ�
        _gameController.ShowGroundMask();

        // �㲥ȼ��״̬
        shellStateListener?.Invoke(ShellNode.Burning);

        // ����ȼ�ն���
        BurningAtPoint(point);

        // �ȴ�����ȼ�����
        await UniTask.Delay(TimeSpan.FromSeconds(burningDuration), ignoreTimeScale: false);

        NRDebugger.Info("[ShellController] Shell burnout");

        // �㲥ȼ�ս���״̬
        shellStateListener?.Invoke(ShellNode.Burnout);

        await UniTask.NextFrame();

        dialogGenerator.GenerateDialog("��ͭ���������");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // ����
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
            // �������ɹ�������ݽ�������������Ƿ�����
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
            // �������ʧ�ܣ��򱣳���ǰ�Ļ��״̬
            NRDebugger.Info("[ShellController] Image detection request failed.");
        }

        // ���debugϵͳ������ͼ��ʶ�����¼�����ͬ������ʾ���
        imageRecogResultListener?.Invoke(res);
    }

    public delegate void StartEventListener();
    public delegate void StopEventListener();
    public delegate void LoadEventListener();
    public delegate void UnloadEventListener();
    public delegate void ShellStateListener(ShellNode state);
    public delegate void ImageRecogResultListener(ImageRecogResult result);
}
