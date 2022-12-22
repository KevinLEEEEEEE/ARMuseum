using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionController : MonoBehaviour
{
    [SerializeField]
    private GameObject SwitchModeInstruction;
    [SerializeField]
    private GameObject SwitchModeProgress;
    [SerializeField]
    private GameObject PinchGestureInstruction;

    // Start is called before the first frame update
    void Start()
    {
        ResetAll();
    }

    public void ShowSwitchModeInstruction()
    {
        SwitchModeInstruction.SetActive(true);
    }

    public void HideSwitchModeInstruction()
    {
        SwitchModeInstruction.SetActive(false);
    }

    public void ShowSwitchModeProgress()
    {
        SwitchModeProgress.SetActive(true);
    }

    public void UpdateSwitchModeProgress(int progress)
    {
        // 传入[0,1]的值，并更新对应的进度条信息
    }

    public void HideSwitchModeProgress()
    {
        SwitchModeProgress.SetActive(false);
    }

    public void ShowPinchGestureInstruction()
    {
        PinchGestureInstruction.SetActive(true);
    }

    public void HidePinchGestureInstruction()
    {
        PinchGestureInstruction.SetActive(false);
    }

    public void ResetAll()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
