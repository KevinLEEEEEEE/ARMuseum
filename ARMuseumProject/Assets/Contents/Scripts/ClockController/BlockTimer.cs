using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTimer : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private GameObject rustBlock;
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


    void Start()
    {
        _clockController.clockListener += UpdateBlock;

        rendererComp = rustBlock.GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    private void UpdateBlock(int date, float percentage)
    {
        UpdateMaterial(percentage);

        UpdateRotation(percentage);
    }

    private void UpdateRotation(float percentage)
    {
        float angleY = rotationAngle * rotationCurve.Evaluate(percentage);

        rustBlock.transform.rotation = Quaternion.Euler(0, angleY, 0);
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
        float value = Mathf.Lerp(min, max, t);

        return value >= 0 ? value : 0;
    }
}
