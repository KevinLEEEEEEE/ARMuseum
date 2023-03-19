using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;
using TMPro;
using System;
using DG.Tweening;

public class OrbButton : MonoBehaviour
{
    public Action orbButtonStartEvent;
    public Action orbButtonStopEvent;
    public Action orbButtonFinishEvent;

    [SerializeField] private string textBeforeActivation;
    [SerializeField] private string textDuringActivation;
    [SerializeField] private Transform orbMesh;
    [SerializeField] private TextMeshPro textMeshComp;
    [SerializeField] private Image imageComp;
    [SerializeField] private AudioClip audioClip_startActivation;
    [SerializeField] private AudioClip audioClip_stopActivation;
    [SerializeField] private float scaleRatio;
    [SerializeField] private float scaleDuration;
    [SerializeField] private bool enableAtStart = false;
    [SerializeField] private bool disableAfterActivated;

    private const float activationDuration = 1f;
    private AudioGenerator audioSource_startActivation;
    private AudioGenerator audioSource_stopActivation;

    private SphereCollider colliderComp;
    private float defaultColliderRadius;
    private Vector3 defaultLocalScale;

    private bool isTriggered;
    private HandState triggeredHand;
    private Tween triggeredTween;

    private void Start()
    {
        audioSource_startActivation = new AudioGenerator(gameObject, audioClip_startActivation);
        audioSource_stopActivation = new AudioGenerator(gameObject, audioClip_stopActivation);
        colliderComp = transform.GetComponent<SphereCollider>();
        defaultColliderRadius = colliderComp.radius;
        defaultLocalScale = orbMesh.localScale;

        Reset();
        if (!enableAtStart) DisableButton();
    }

    private void OnDisable()
    {
        if (isTriggered) CancelProgress();
    }

    public void Reset()
    {
        isTriggered = false;
        triggeredHand = null;
        triggeredTween = null;
        textMeshComp.text = textBeforeActivation;
        UpdateProgress(0);
    }

    public void EnableButton()
    {
        colliderComp.enabled = true;
        orbMesh.gameObject.SetActive(true);
        textMeshComp.gameObject.SetActive(true);
        imageComp.gameObject.SetActive(true);
    }

    public void DisableButton()
    {
        colliderComp.enabled = false;
        orbMesh.gameObject.SetActive(false);
        textMeshComp.gameObject.SetActive(false);
        imageComp.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        ActiveProgress(other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        CancelProgress();
    }

    private void ActiveProgress(string name)
    {
        NRDebugger.Info("[OrnButton] Avtive progress");

        // 改变状态与明确触发对象
        isTriggered = true;
        triggeredHand = GetTriggeredHandState(name);

        // 缩放Mesh和Collider
        ScaleColliderRaduis(scaleRatio);
        ScaleOrbMesh(scaleRatio);

        // 改变文本内容和位置
        textMeshComp.transform.DOLocalMoveY(-0.12f, scaleDuration);
        if (textDuringActivation != null) textMeshComp.text = textDuringActivation;

        // 播放音频
        audioSource_startActivation.Play();

        // 在指定时间内实现由0至1的运行
        triggeredTween = DOTween.To(UpdateProgress, 0, 1, activationDuration).OnComplete(FinishProgress);

        // 激活事件
        orbButtonStartEvent?.Invoke();
    }

    private void CancelProgress()
    {
        NRDebugger.Info("[OrnButton] Cancel progress");

        triggeredTween.Kill();
        triggeredTween = null;

        ScaleColliderRaduis(1f);
        ScaleOrbMesh(1f);

        textMeshComp.transform.DOLocalMoveY(0, scaleDuration);
        
        audioSource_stopActivation.Play();

        Reset();
        orbButtonStopEvent?.Invoke();
    }

    private void FinishProgress()
    {
        NRDebugger.Info("[OrnButton] Finish progress");

        textMeshComp.transform.DOLocalMoveY(0, scaleDuration);

        audioSource_stopActivation.Play();

        Reset();
        if (disableAfterActivated) DisableButton();
        orbButtonFinishEvent?.Invoke();
    }

    private void UpdateProgress(float progress)
    {
        imageComp.fillAmount = progress;
    }

    private HandState GetTriggeredHandState(string name)
    {
        HandEnum triggeredEnum = HandEnum.None;

        if (name == "ColliderEntity_IndexTip_R")
        {
            triggeredEnum = HandEnum.RightHand;
        }
        else if (name == "ColliderEntity_IndexTip_L")
        {
            triggeredEnum = HandEnum.LeftHand;
        }

        return NRInput.Hands.GetHandState(triggeredEnum);
    }

    private void ScaleColliderRaduis(float ratio)
    {
        colliderComp.radius = defaultColliderRadius * ratio;
    }

    private void ScaleOrbMesh(float ratio)
    {
        orbMesh.transform.DOScale(defaultLocalScale * ratio, scaleDuration);
    }

    void Update()
    {
        if(isTriggered && triggeredHand.currentGesture != HandGesture.Point)
        {
            CancelProgress();
        }
    }
}
