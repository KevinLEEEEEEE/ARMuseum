using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionGenerator : MonoBehaviour
{
    public GameObject instructionPrefab;

    public Action GenerateInstruction(string title, string content)
    {
        GameObject instruction = Instantiate(instructionPrefab, transform, false);
        Instruction ins = instruction.GetComponent<Instruction>();
        ins.SetContent(title, content);
        ins.StartInstruction();

        return () =>
        {
            ins.EndInstruction();
        };
    }
}
