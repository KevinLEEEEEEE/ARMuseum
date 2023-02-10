using System.Collections;
using System.Collections.Generic;
using AmazingAssets.AdvancedDissolve;
using UnityEngine;
using NRKernal;

public class Match : MonoBehaviour
{
    public AdvancedDissolveGeometricCutoutController cutout_front;
    public AdvancedDissolveGeometricCutoutController cutout_back;
    public GameController_S2 gameController;
    public float burningDuration;
    public float burningBeginRadius;
    public float burningEndRadius;
    private GameObject lightObject;
    private Light lightComponent;

    private enum MatchState
    {
        suspend,
        active,
        burning,
        burnout
    }
    private MatchState state;

    private void Start()
    {
        lightObject = transform.GetChild(0).gameObject;
        lightComponent = lightObject.GetComponent<Light>();
        lightObject.SetActive(false);
        state = MatchState.suspend;

        cutout_front.target1Radius = burningBeginRadius;
        cutout_back.target1Radius = burningBeginRadius;
    }

    public void HideMatch()
    {
        if(state == MatchState.active)
        {
            state = MatchState.suspend;
            lightObject.SetActive(false);
        }
    }

    public void ShowMatch()
    {
        if(state == MatchState.suspend)
        {
            state = MatchState.active;
            lightObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(state == MatchState.active)
        {
            Debug.Log("[Player] match start burning");
            state = MatchState.burning;
            StartCoroutine(nameof(Burning));
        }
    }

    private IEnumerator Burning()
    {
        StartCoroutine(burningDuration.Tweeng((r) =>
        {
            cutout_front.target1Radius = r;
            cutout_back.target1Radius = r;
        }, burningBeginRadius, burningEndRadius));

        StartCoroutine(2f.Tweeng((r) =>
        {
            lightComponent.intensity = r;
        }, lightComponent.intensity, 0));

        yield return new WaitForSeconds(burningDuration);

        BurnOut();
    }

    private void BurnOut()
    {
        state = MatchState.burnout;
        lightObject.SetActive(false);

        SendMessageUpwards("BurnOutMessage");
    }

    private void Update()
    {
        if(state == MatchState.active)
        {
            transform.position = gameController.getHandJointPose(HandJointID.IndexTip).position;
        }
    }
}
