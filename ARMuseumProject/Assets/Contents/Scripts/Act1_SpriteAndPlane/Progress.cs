using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;

public class Progress : MonoBehaviour
{
    public Image progressUI;
    public GameController_S2 gameController;
    private Animation animePlayer;
    private bool isActive;

    private void Start()
    {
        animePlayer = transform.GetComponent<Animation>();
        HideProgress();
    }

    public void StartRadialProgress()
    {
        if(animePlayer.isPlaying)
        {
            animePlayer.Stop();
        }
        animePlayer.Play();
    }

    public void ShowProgress()
    {
        progressUI.gameObject.SetActive(true);
        isActive = true;
    }

    public void HideProgress()
    {
        progressUI.gameObject.SetActive(false);
        animePlayer.Stop();
        isActive = false;
    }

    private void Update()
    {
        HandState domainHandState = gameController.GetDomainHandState();

        if(domainHandState.isTracked && isActive)
        {
            progressUI.gameObject.SetActive(true);
        } else
        {
            progressUI.gameObject.SetActive(false);
        }

        if (isActive)
        {
            Pose jointPose = domainHandState.GetJointPose(HandJointID.IndexTip);
            transform.position = jointPose.position + Vector3.up * 0.015f + jointPose.up * 0.01f;
        }
    }
}
