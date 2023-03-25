using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using DG.Tweening;

public enum MatchState
{
    Suspend,
    Default,
    Burning,
}

public class ShellMatchManager : MonoBehaviour
{
    [SerializeField] private ShellController _shellController;
    [SerializeField] private Light lightComp;
    [SerializeField] private ParticleSystem haloParticleComp;
    [SerializeField] private ParticleSystem darknessParticleComp;
    [SerializeField] private float maskOffset;
    [SerializeField] private float maskMoveSpeed;

    private Collider colliderComp;
    private HandState rightHandState;
    private HandState leftHandState;
    private HandState holdingHand;
    private MatchState currentState;
    private float defaultLightIntensity;

    private void Start()
    {
        //_shellController.shellStateListener += ShellStateHandler;
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        colliderComp = GetComponent<Collider>();
        defaultLightIntensity = lightComp.intensity;

        Reset();
    }

    public void Reset()
    {  
        currentState = MatchState.Suspend;
        colliderComp.enabled = false;
        lightComp.intensity = 0;
        lightComp.gameObject.SetActive(false);
        haloParticleComp.gameObject.SetActive(false);
    }

    //private void ShellStateHandler(ShellNode node)
    //{
    //    if(node == ShellNode.ToBurn)
    //    {
    //        currentState = MatchState.Default;
    //        lightComp.gameObject.SetActive(true);
    //        haloParticleComp.gameObject.SetActive(true);
    //    } else if(node == ShellNode.Burning)
    //    {
    //        Reset();
    //    }
    //}

    public void PutOutMatch()
    {
        if(currentState == MatchState.Burning)
        {
            currentState = MatchState.Default;
            holdingHand = null;
            colliderComp.enabled = false;
            lightComp.DOIntensity(0, 1);
            haloParticleComp.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            darknessParticleComp.gameObject.transform.DOScale(0, 0.3f);
        }
    }

    public void LightUpMatch()
    {
        if(currentState == MatchState.Default)
        {
            currentState = MatchState.Burning;
            holdingHand = rightHandState.isPinching ? rightHandState : leftHandState;
            MoveMatchToTarget(false);
            colliderComp.enabled = true;
            lightComp.DOIntensity(defaultLightIntensity, 1);
            haloParticleComp.Play();
            darknessParticleComp.gameObject.transform.DOScale(1.5f, 0.3f);
        }
    }

    private void MoveMatchToTarget(bool smooth)
    {
        if (holdingHand == null) return;

        Vector3 direction = (holdingHand.GetJointPose(HandJointID.ThumbDistal).up + holdingHand.GetJointPose(HandJointID.IndexDistal).up * 1.5f) / 2;
        Vector3 point = holdingHand.GetJointPose(HandJointID.ThumbTip).position;
        Vector3 target = point + direction * maskOffset;

        if (smooth)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * maskMoveSpeed);
        }
        else
        {
            transform.position = target;
        }

        transform.up = direction;
    }

    private void Update()
    {
        if(currentState == MatchState.Burning)
        {
            MoveMatchToTarget(true);
        }
    }
}
