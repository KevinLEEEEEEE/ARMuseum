using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NRKernal;
using NRKernal.NRExamples;

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

    private MoveWithCamera m_MoveWithCamera;
    private Instruction insComp;
    private InstructionState state;

    private void Start()
    {
        m_MoveWithCamera = transform.GetComponent<MoveWithCamera>();
        insComp = instruction.GetComponent<Instruction>();

        insComp.FadeOutEventListener += FadeOutEventHandler;
        insComp.FadeInEventListener += FadeInEventHandler;

        Reset();
    }

    private void Reset()
    {
        instruction.SetActive(false);
        m_MoveWithCamera.enabled = false;
        state = InstructionState.Ready;
    }

    public InstructionState GetState()
    {
        return state;
    }

    public Action GenerateInstruction(string title, string content)
    {
        if (state != InstructionState.Ready)
        {
            return () => { };
        } else
        {
            ShowInstruction(title, content);
            return HideInstruction;
        }   
    }

    public async void GenerateInstruction(string title, string content, float duration)
    {
        if (state != InstructionState.Ready) 
            return;

        ShowInstruction(title, content);

        await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: false);

        HideInstruction();
    }

    private void ShowInstruction(string title, string content)
    {
        state = InstructionState.FadeIn;
        m_MoveWithCamera.enabled = true;
        m_MoveWithCamera.ResetTransform();
        instruction.SetActive(true);
        insComp.StartInstruction(title, content);
    }

    public void HideInstruction()
    {
        if(state == InstructionState.Looping)
        {
            insComp.EndInstruction();
            state = InstructionState.FadeOut;
        }
    }

    private void FadeInEventHandler()
    {
        state = InstructionState.Looping;
    }

    private void FadeOutEventHandler()
    {
        Reset();
    }
}
