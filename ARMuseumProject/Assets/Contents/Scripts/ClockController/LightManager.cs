using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LightManager : MonoBehaviour
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
    private bool isLightCompOccupied;
    private float ambientLightIntensity;

    void Start()
    {
        lightComp = GetComponent<Light>();
        lightAnimatorComp = GetComponent<Animator>();

        _clockController.speedModeListener += SpeedModeHandler;
        _clockController.dateMessageListener += DateMessageHandler;
        _clockController.startEventListener += StartEventHandler;
        _clockController.stopEventListener += StopEventHandler;
        _clockController.unloadEventListener += UnloadEventHandler;

        Reset();
    }

    private void Reset()
    {
        lightComp.gameObject.SetActive(false);
        lightComp.intensity = 0;
        isLightCompOccupied = true;
    }

    private void UnloadEventHandler()
    {
        Reset();
    }

    private void StartEventHandler()
    {
        ambientLightIntensity = _gameController.GetAmbientLightIntensity();
        _gameController.SetAmbientLightInSeconds(0.15f, lightFadeDuration);
        lightComp.gameObject.SetActive(true);
        lightComp.DOIntensity(lightIntensityDefault, lightFadeDuration).OnComplete(() => { isLightCompOccupied = false; });
        lightAnimatorComp.SetTrigger("Enter"); 
    }

    private void StopEventHandler()
    {
        isLightCompOccupied = true;
        _gameController.SetAmbientLightInSeconds(ambientLightIntensity, lightFadeDuration);
        lightComp.DOIntensity(0, lightFadeDuration / 2);
        lightAnimatorComp.SetFloat("CycleSpeed", 50);
        lightAnimatorComp.SetTrigger("Exit");
    }

    private void DateMessageHandler(int date, float progress)
    {
        if(!isLightCompOccupied)
        {
            lightComp.intensity = lightIntensityDefault * lightIntensityMultiplier.Evaluate(progress);
        }
    }

    private void SpeedModeHandler(SpeedMode mode)
    {
        lightAnimatorComp.SetFloat("CycleSpeed", mode == SpeedMode.Normal ? cycleSpeedNormal : cycleSpeedAccelerated);
    }
}
