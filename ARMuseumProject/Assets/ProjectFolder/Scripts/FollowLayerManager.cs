using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLayerManager : MonoBehaviour
{
    public GameObject OnBoardingObj;
    public GameObject SwitchModeObj;
    public GameObject PinchOnboardingObj;

    // Start is called before the first frame update
    void Start()
    {
        OnBoardingObj.SetActive(false);
        SwitchModeObj.SetActive(false);
        PinchOnboardingObj.SetActive(false);
    }

    public void ShowOnboarding()
    {
        OnBoardingObj.SetActive(true);
    }

    public void HideOnboarding()
    {
        OnBoardingObj.SetActive(false);
    }

    public void ShowSwitchModeProgress()
    {
        SwitchModeObj.SetActive(true);
    }
    public void HideSwitchModeProgress()
    {
        SwitchModeObj.SetActive(false);
    }

    public void ShowPinchOnboarding()
    {
        PinchOnboardingObj.SetActive(true);
    }

    public void HidePinchOnboarding()
    {
        PinchOnboardingObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
