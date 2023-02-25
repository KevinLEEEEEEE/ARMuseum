using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LightTimer : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private int cycleSpeedNormal;
    [SerializeField] private int cycleSpeedAccelerated;
    [SerializeField] private float lightIntensityDefault;
    [SerializeField] private float lightFadeDuration;
    private Animator lightAnimatorComp;
    private Light lightComp;

    void Start()
    {
        lightComp = GetComponent<Light>();
        lightAnimatorComp = GetComponent<Animator>();

        _clockController.speedModeListener += UpdateState;
        _clockController.startEventListener += StartTimer;
        _clockController.stopEventListener += StopTimer;
    }

    private void StartTimer()
    {
        lightComp.DOIntensity(lightIntensityDefault, lightFadeDuration);
        lightAnimatorComp.SetTrigger("Enter");
    }

    private void StopTimer()
    {
        lightComp.DOIntensity(0, lightFadeDuration);
        lightAnimatorComp.SetFloat("CycleSpeed", cycleSpeedAccelerated);
        lightAnimatorComp.SetTrigger("Exit");
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
