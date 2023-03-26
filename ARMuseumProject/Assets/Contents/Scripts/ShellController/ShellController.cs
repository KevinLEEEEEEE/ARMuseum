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

    void Start()
    {
        imageRecognition = transform.GetComponent<ImageRecognition>();
        cameraManager = transform.GetComponent<CameraManager>();
        animatorComp = transform.GetComponent<Animator>();
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHnadState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        source_shellFadeIn = new AudioGenerator(gameObject, clip_shellFadeIn);
        source_shellBurning = new AudioGenerator(gameObject, clip_shellBurning, true, false, 0);
        source_AncientAmbient = new AudioGenerator(gameObject, clip_AncientAmbient);
        source_shellCasting = new AudioGenerator(gameObject, clip_shellCasting);
        source_shellCasting.source.pitch = 0.92f;

        Reset();
    }

    private void Reset()
    {
        isObjectFound = false;
        canDetectObject = true;
        hasObjectDetectedBefore = false;
        HideRoots();
    }

    private void ShowRoots()
    {
        foreach(Transform trans in transform)
        {
            trans.gameObject.SetActive(true);
        }
    }

    private void HideRoots()
    {
        foreach (Transform trans in transform)
        {
            trans.gameObject.SetActive(false);
        }
    }

    public void Init()
    {
        OpeningScene();
    }

    private async void OpeningScene()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        // �����������ɶ���
        ShowRoots();
        source_shellFadeIn.Play();
        animatorComp.Play("ShellFadeIn");

        await UniTask.Delay(TimeSpan.FromSeconds(3), ignoreTimeScale: false);

        _gameController.SetAmbientLightInSeconds(0.1f, 4);
        _gameController.SetEnvLightIntensityInSeconds(0.25f, 4);

        // �ȴ����ɶ�������
        await UniTask.Delay(TimeSpan.FromSeconds(6), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("��ģ���γ�......");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration + 1), ignoreTimeScale: false);

        dialogGenerator.GenerateDialog("������񣬼���¯������");

        await UniTask.Delay(TimeSpan.FromSeconds(DialogGenerator.dialogDuration), ignoreTimeScale: false);

        // ������һ��������ʾ
        instructionGenerator.GenerateInstruction("�������", "������񡸼��١�¯������");

        await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

        // �����ɲ㣬�ڵ�����ײ�
        _gameController.ShowGroundMask();

        // ����ȼ�ն���
        animatorComp.Play("ShellBurning");
        source_shellBurning.Play();
        source_shellBurning.SetVolumeInSeconds(0.2f, 15);

        // ��������ʶ�����
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

        // ����ͼ��ʶ�����
        cameraManager.Close();
        canDetectObject = false;

        // �������ٶȻ������������ٲ��رջ��ģ��
        FastSpeedMode();
        m_matchManager.PutOutMatch();
        StopAncientAmbientSound();

        // �ָ���Դ
        _gameController.SetAmbientLightInSeconds(1.5f, 7);
        _gameController.SetEnvLightIntensityInSeconds(1.3f, 7);

        source_shellBurning.SetVolumeInSeconds(0, 6);
        source_shellCasting.Play();
        animatorComp.Play("ShellCasting");
    }

    public async void ShellCastingFinish()
    {
        dialogGenerator.GenerateDialog("��ͭ���������");

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
            // �������ɹ�������ݽ�������������Ƿ�����
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
            // �������ʧ�ܣ��򱣳���ǰ�Ļ��״̬
            NRDebugger.Info("[ShellController] Image detection request failed.");
        }

        // ���debugϵͳ������ͼ��ʶ�����¼�����ͬ������ʾ���
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
        source_AncientAmbient.source.DOFade(1f, 2f);
    }

    private void PauseAncientAmbientSound()
    {
        source_AncientAmbient.source.DOFade(0, 2f).OnComplete(() => {
            if(!isObjectFound)
                source_AncientAmbient.source.Pause();
        });
    }

    private void StopAncientAmbientSound()
    {
        source_AncientAmbient.source.DOFade(0, 2f).OnComplete(() => {
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
