using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NRKernal;

public class InstructionGenerator : MonoBehaviour
{
    [SerializeField] private GameObject instruction;
    [SerializeField] private GameObject trail;
    private Instruction insComp;
    private ParticleSystem trailParticleComp;

    private void Start()
    {
        trailParticleComp = trail.GetComponent<ParticleSystem>();
        insComp = instruction.GetComponent<Instruction>();
        insComp.AnimationEndEventListener += AnimationEndEventHandler;

        Reset();
    }

    private void Reset()
    {
        instruction.SetActive(false);
    }

    public Action GenerateInstruction(string title, string content)
    {
        instruction.SetActive(true);
        insComp.StartInstruction(title, content);

        return () =>
        {
            insComp.EndInstruction();
        };
    }

    public async void GenerateInstruction(string title, string content, float duration)
    {
        instruction.SetActive(true);
        insComp.StartInstruction(title, content);

        await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: false);

        insComp.EndInstruction();
    }

    public void GenerateTrail(Vector3 endPoint, float moveDuration, float residenceDuration)
    {
        if(!trailParticleComp)
        {
            NRDebugger.Error("[InstructionGenerator] Cannot find trail particle component");
            return;
        }

        trailParticleComp.Play();
        trail.transform.position = instruction.transform.position;

        trail.transform.DOMove(endPoint, moveDuration).OnComplete(async () =>  {
            await UniTask.Delay(TimeSpan.FromSeconds(residenceDuration), ignoreTimeScale: false);
            trailParticleComp.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        });
    }

    private void AnimationEndEventHandler()
    {
        Reset();
    }
}
