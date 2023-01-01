using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionController : MonoBehaviour
{
    [SerializeField] private GameObject SwitchModeInstruction;
    [SerializeField] private GameObject SwitchModeProgress;
    [SerializeField] private GameObject PinchGestureInstruction;
    [SerializeField] private SoundController _SoundController;

    // Start is called before the first frame update
    void Start()
    {
        ResetAll();
    }

    public void ShowSwitchModeInstruction()
    {
        SwitchModeInstruction.SetActive(true);

        Debug.Log("show");

        //_SoundController.PlaySound(SoundController.Sounds.UserGuide);
    }

    public void HideSwitchModeInstruction()
    {
        SwitchModeInstruction.SetActive(false);
    }

    public void ShowSwitchModeProgress()
    {
        SwitchModeProgress.SetActive(true);

        //_SoundController.PlaySound(SoundController.Sounds.UserGuide);
    }

    public void HideSwitchModeProgress()
    {
        SwitchModeProgress.SetActive(false);
    }

    public void ShowPinchGestureInstruction()
    {
        PinchGestureInstruction.SetActive(true);

        //_SoundController.PlaySound(SoundController.Sounds.UserGuide);
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
