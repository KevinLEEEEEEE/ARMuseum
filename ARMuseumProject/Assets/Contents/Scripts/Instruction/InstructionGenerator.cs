using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NRKernal;

public enum InstructionState
{
    Ready,
    FadeIn,
    Looping,
    FadeOut,
}

public class InstructionGenerator : MonoBehaviour
{
    [SerializeField] private GameObject instruction;
    //[SerializeField] private GameObject trail;
    private Instruction insComp;
    //private ParticleSystem trailParticleComp;
    private InstructionState state;

    private void Start()
    {
        //trailParticleComp = trail.GetComponent<ParticleSystem>();
        insComp = instruction.GetComponent<Instruction>();

        insComp.FadeOutEventListener += FadeOutEventHandler;
        insComp.FadeInEventListener += FadeInEventHandler;

        Reset();
    }

    private void Reset()
    {
        instruction.SetActive(false);
        state = InstructionState.Ready;
    }

    public InstructionState GetState()
    {
        return state;
    }

    public Action GenerateInstruction(string title, string content)
    {
        if (state != InstructionState.Ready) return () => { };

        state = InstructionState.FadeIn;
        instruction.SetActive(true);
        insComp.StartInstruction(title, content);

        return HideInstruction;
    }

    public async void GenerateInstruction(string title, string content, float duration)
    {
        if (state != InstructionState.Ready) return;

        state = InstructionState.FadeIn;
        instruction.SetActive(true);
        insComp.StartInstruction(title, content);

        await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: false);

        HideInstruction();
    }

    public void HideInstruction()
    {
        if(state == InstructionState.Looping)
        {
            insComp.EndInstruction();
            state = InstructionState.FadeOut;
        }
    }
 
    //public void GenerateTrail(Vector3 endPoint, float moveDuration, float residenceDuration)
    //{
    //    if(!trailParticleComp)
    //    {
    //        NRDebugger.Error("[InstructionGenerator] Cannot find trail particle component");
    //        return;
    //    }

    //    trailParticleComp.Play();
    //    trail.transform.position = instruction.transform.position;

    //    trail.transform.DOMove(endPoint, moveDuration).OnComplete(async () =>  {
    //        await UniTask.Delay(TimeSpan.FromSeconds(residenceDuration), ignoreTimeScale: false);
    //        trailParticleComp.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    //    });
    //}

    private void FadeInEventHandler()
    {
        state = InstructionState.Looping;
    }

    private void FadeOutEventHandler()
    {
        Reset();
    }
}
