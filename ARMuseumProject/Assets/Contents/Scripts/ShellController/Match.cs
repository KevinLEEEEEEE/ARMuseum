using System.Collections;
using System.Collections.Generic;
using AmazingAssets.AdvancedDissolve;
using UnityEngine;
using NRKernal;
using System;

public class Match : MonoBehaviour
{
    public AdvancedDissolveGeometricCutoutController cutout_front;
    public AdvancedDissolveGeometricCutoutController cutout_back;
    public GameController_Historical gameController;
    public Action BurningEventListener;
    public Action BurnoutEventListener;
    public GameObject lightBulb;
    public AudioClip audioClip_shellMatchTrigger;
    public AudioClip audioClip_shellBurning;
    public float burningDuration;
    public float burningBeginRadius;
    public float burningEndRadius;

    private AudioGenerator audioSource_shellMatchTrigger;
    private AudioGenerator audioSource_shellBurning;

    private enum MatchState
    {
        suspend,
        active,
        burning,
        burnout
    }
    private MatchState state = MatchState.suspend;

    private void Start()
    {
        audioSource_shellMatchTrigger = new AudioGenerator(gameObject, audioClip_shellMatchTrigger);
        audioSource_shellBurning = new AudioGenerator(gameObject, audioClip_shellBurning, false, false, 0.8f);
        ResetAll();
    }

    public void ResetAll()
    {
        lightBulb.SetActive(false);
        transform.position = new Vector3(0, 5, 0);

        cutout_front.target1Radius = burningBeginRadius;
        cutout_back.target1Radius = burningBeginRadius;
        cutout_front.ForceUpdateShaderData();
        cutout_back.ForceUpdateShaderData();
        SetADControllerUpdateMode(AdvancedDissolveGeometricCutoutController.UpdateMode.Manual);
    }

    public void DisableMatch()
    {
        if(state == MatchState.active)
        {
            state = MatchState.suspend;
            lightBulb.SetActive(false);
        }
    }

    public void EnableMatch()
    {
        if(state == MatchState.suspend)
        {
            state = MatchState.active;
            lightBulb.SetActive(true);
        }
    }

    private void SetADControllerUpdateMode(AdvancedDissolveGeometricCutoutController.UpdateMode mode)
    {
        cutout_front.updateMode = mode;
        cutout_back.updateMode = mode;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(state == MatchState.active)
        {
            Debug.Log("[Match] Trigger detected, start burning.");
            state = MatchState.burning;
            StartCoroutine(nameof(Burning));
        }
    }

    private IEnumerator Burning()
    {
        BurningEventListener?.Invoke();
        SetADControllerUpdateMode(AdvancedDissolveGeometricCutoutController.UpdateMode.EveryFrame);

        StartCoroutine(burningDuration.Tweeng((r) =>
        {
            cutout_front.target1Radius = r;
            cutout_back.target1Radius = r;
        }, burningBeginRadius, burningEndRadius));

        Light lightOutter = lightBulb.transform.GetChild(0).GetComponent<Light>();
        Light lightInner = lightBulb.transform.GetChild(1).GetComponent<Light>();

        CloseLightInSeconds(lightOutter, 2f);
        CloseLightInSeconds(lightInner, 2f);

        audioSource_shellMatchTrigger.Play();

        yield return new WaitForSeconds(0.7f);

        audioSource_shellBurning.Play();

        yield return new WaitForSeconds(burningDuration - 0.7f);

        BurnOut();
    }

    private void CloseLightInSeconds(Light light, float duration)
    {
        StartCoroutine(duration.Tweeng((r) =>
        {
            light.intensity = r;
        }, light.intensity, 0));
    }

    private void BurnOut()
    {
        state = MatchState.burnout;
        lightBulb.SetActive(false);
        SetADControllerUpdateMode(AdvancedDissolveGeometricCutoutController.UpdateMode.Manual);
        BurnoutEventListener?.Invoke();
    }

    private void Update()
    {
        if(state == MatchState.active)
        {
            transform.position = gameController.GetDomainHandState().GetJointPose(HandJointID.IndexTip).position;
        }
    }
}
