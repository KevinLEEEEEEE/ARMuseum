using AmazingAssets.AdvancedDissolve;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private AdvancedDissolvePropertiesController dissolveController;
    [SerializeField] private GameObject rustBlock_Transition;
    [SerializeField] private GameObject rustBlock_Dissolve;
    [SerializeField] private ParticleSystem dissolveParticleComp;

    private Renderer rendererComp;
    private MaterialPropertyBlock propertyBlock;

    private const string rustIntensity = "_RustIntensity";
    private const string rustMaskWeight = "_RustMaskWeight";
    private const string rustSecondPattern = "_RustSecondPattern";
    private const string dirtIntensity = "_DirtIntensity";

    [SerializeField] private float rustIntensityMin;
    [SerializeField] private float rustIntensityMax;

    [SerializeField] private float rustMaskWeightMin;
    [SerializeField] private float rustMaskWeightMax;

    [SerializeField] private float rustSecondPatternMin;
    [SerializeField] private float rustSecondPatternMax;

    [SerializeField] private float dirtIntensityMin;
    [SerializeField] private float dirtIntensityMax;
    [SerializeField] private AnimationCurve dirtIntensityCurve;

    [SerializeField] private int rotationAngle;
    [SerializeField] private AnimationCurve rotationCurve;

    [SerializeField] private float clipStart;
    [SerializeField] private float clipEnd;
    [SerializeField] private float clipDuration;

    void Start()
    {
        _clockController.dateMessageListener += DateMessageHandler;
        _clockController.loadEventListener += LoadEventHandler;
        _clockController.unloadEventListener += UnloadEventHandler;

        rendererComp = rustBlock_Transition.GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        Reset();
    }

    public void Reset()
    {
        rustBlock_Transition.SetActive(false);
        rustBlock_Dissolve.SetActive(false);
        dissolveController.cutoutStandard.clip = clipStart;
        UpdateMaterial(0);
        UpdateRotation(0);
    }

    private void LoadEventHandler()
    {
        rustBlock_Transition.SetActive(true);
    }

    private async void UnloadEventHandler()
    {
        rustBlock_Transition.SetActive(false);
        rustBlock_Dissolve.SetActive(true);

        dissolveParticleComp.Play();

        StartCoroutine(clipDuration.Tweeng((r) =>
        {
            dissolveController.cutoutStandard.clip = r;
        }, clipStart, clipEnd));

        await UniTask.Delay(TimeSpan.FromSeconds(clipDuration), ignoreTimeScale: false);

        Reset();
    }

    private void DateMessageHandler(int date, float percentage)
    {
        UpdateMaterial(percentage);
        UpdateRotation(percentage);
    }

    private void UpdateRotation(float percentage)
    {
        float angleY = rotationAngle * rotationCurve.Evaluate(percentage);

        rustBlock_Transition.transform.rotation = Quaternion.Euler(0, angleY, 0);
    }

    private void UpdateMaterial(float percentage)
    {
        rendererComp.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(rustIntensity, CalcValue(rustIntensityMin, rustIntensityMax, percentage));
        propertyBlock.SetFloat(rustMaskWeight, CalcValue(rustMaskWeightMin, rustMaskWeightMax, percentage));
        propertyBlock.SetFloat(rustSecondPattern, CalcValue(rustSecondPatternMin, rustSecondPatternMax, percentage));
        propertyBlock.SetFloat(dirtIntensity, CalcValue(dirtIntensityMin, dirtIntensityMax, dirtIntensityCurve.Evaluate(percentage)));

        rendererComp.SetPropertyBlock(propertyBlock);
    }

    private float CalcValue(float min, float max, float t)
    {
        return Mathf.Max(Mathf.Lerp(min, max, t), 0);
    }
}
