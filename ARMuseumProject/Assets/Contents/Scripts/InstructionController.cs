using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstructionController : MonoBehaviour
{
    //[SerializeField] private GameObject SwitchModeInstruction;
    //[SerializeField] private GameObject SwitchModeProgress;
    //[SerializeField] private GameObject PinchGestureInstruction;
    private AudioSource AudioPlayer;

    void Start()
    {
        AudioPlayer = transform.GetComponent<AudioSource>();

        //ResetAll();
    }

    //public void ShowSwitchModeInstruction()
    //{
    //    SwitchModeInstruction.SetActive(true);
    //    AudioPlayer.Play();
    //}

    //public void HideSwitchModeInstruction()
    //{
    //    SwitchModeInstruction.SetActive(false);
    //}

    //public void ShowSwitchModeProgress()
    //{
    //    SwitchModeProgress.SetActive(true);
    //    AudioPlayer.Play();
    //}

    //public void HideSwitchModeProgress()
    //{
    //    SwitchModeProgress.SetActive(false);
    //}

    //public void ShowPinchGestureInstruction()
    //{
    //    PinchGestureInstruction.SetActive(true);
    //    AudioPlayer.Play();
    //}

    //public void HidePinchGestureInstruction()
    //{
    //    PinchGestureInstruction.SetActive(false);
    //}

    //public void ResetAll()
    //{
    //    HideSwitchModeInstruction();
    //    HideSwitchModeProgress();
    //    HidePinchGestureInstruction();
    //}
}
