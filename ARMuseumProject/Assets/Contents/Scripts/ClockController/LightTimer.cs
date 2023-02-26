using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LightTimer : MonoBehaviour
{
    [SerializeField] private GameController_Historical _gameController;
    [SerializeField] private ClockController _clockController;
    [SerializeField] private int cycleSpeedNormal;
    [SerializeField] private int cycleSpeedAccelerated;
    [SerializeField] private float lightIntensityDefault;
    [SerializeField] private AnimationCurve lightIntensityMultiplier;
    [SerializeField] private float lightFadeDuration;
    private Animator lightAnimatorComp;
    private Light lightComp;
    private bool canMultiply;

    void Start()
    {
        lightComp = GetComponent<Light>();
        lightAnimatorComp = GetComponent<Animator>();

        _clockController.speedModeListener += UpdateState;
        _clockController.clockListener += UpdateIntensity;
        _clockController.startEventListener += StartTimer;
        _clockController.stopEventListener += StopTimer;

        Reset();
    }

    private void Reset()
    {
        canMultiply = false;
    }

    private void StartTimer()
    {
        _gameController.SetAmbientLightInSeconds(0, lightFadeDuration);
        lightComp.DOIntensity(lightIntensityDefault, lightFadeDuration).OnComplete(() => { canMultiply = true; });
        lightAnimatorComp.SetTrigger("Enter");
    }

    private void StopTimer()
    {
        _gameController.SetAmbientLightInSeconds(1.2f, lightFadeDuration);
        lightAnimatorComp.SetFloat("CycleSpeed", 50);
        lightComp.DOIntensity(0, lightFadeDuration);
        lightAnimatorComp.SetTrigger("Exit");
    }

    private void UpdateIntensity(int date, float progress)
    {
        if(canMultiply)
        {
            lightComp.intensity = lightIntensityDefault * lightIntensityMultiplier.Evaluate(progress);
        }
    }

    private void UpdateState(SpeedMode mode)
    {
        if (mode == SpeedMode.Normal)
        {
            lightAnimatorComp.SetFloat("CycleSpeed", cycleSpeedNormal);
        } else
        {
            lightAnimatorComp.SetFloat("CycleSpeed", cycleSpeedAccelerated);
        }
    }
}
