using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class ShellEffect
{
    [SerializeField] private ShellNode startNode;
    [SerializeField] private ShellNode endNode;
    [SerializeField] private ParticleSystem particleRoot;
    [SerializeField] private GameObject lightRoot;
    [SerializeField] private float lightIntensity;
    [SerializeField] private float lightFadeInDuration;
    [SerializeField] private float lightFadeOutDuration;

    public void Reset()
    {
        if (lightRoot)
        {
            lightRoot.SetActive(false);
            SetIntensityOfRoot(0);
        }
    }

    public void Trigger(ShellNode node)
    {
        if(node == startNode)
        {
            StartEffect();
        } else if(node == endNode)
        {
            EndEffect();
        }
    }

    public void StartEffect()
    {
        if(lightRoot)
        {
            lightRoot.SetActive(true);
            SetIntensityOfRootInSeconds(lightIntensity, lightFadeInDuration, () => { });
        }
            
        if(particleRoot)
        {
            particleRoot.Play();
        }
    }

    public void EndEffect()
    {
        if (lightRoot)
        {
            SetIntensityOfRootInSeconds(0, lightFadeOutDuration, () =>
            {
                lightRoot.SetActive(false);
            });
        }

        if (particleRoot)
        {
            particleRoot.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void SetIntensityOfRoot(float intensity)
    {
        foreach (Transform light in lightRoot.transform)
        {
            light.GetComponent<Light>().intensity = intensity;
        }
    }

    private void SetIntensityOfRootInSeconds(float intensity, float duration, TweenCallback callback)
    {
        foreach (Transform light in lightRoot.transform)
        {
            light.GetComponent<Light>().DOIntensity(intensity, duration).OnComplete(callback);
        }
    }
}

public class ShellEffectManager : MonoBehaviour
{
    [SerializeField] private ShellController _shellController;
    [SerializeField] private ShellEffect[] shellEffects;

    void Start()
    {
        _shellController.shellStateListener += ShellStateHandler;

        Reset();
    }

    private void Reset()
    {
        for (int i = 0; i < shellEffects.Length; i++)
        {
            shellEffects[i].Reset();
        }
    }

    private void ShellStateHandler(ShellNode node)
    {
        for (int i = 0; i < shellEffects.Length; i++)
        {
            shellEffects[i].Trigger(node);
        }
    }
}
