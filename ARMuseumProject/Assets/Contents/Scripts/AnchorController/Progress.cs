using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;

public class Progress : MonoBehaviour
{
    public GameController_S2 gameController;

    private Image progressUI;
    private Animation animePlayer;

    private void Start()
    {
        animePlayer = transform.GetComponent<Animation>();
        progressUI = transform.GetChild(0).GetComponent<Image>();
        ResetRadialProgress();
    }

    public void StartRadialProgress()
    {
        progressUI.gameObject.SetActive(true);
        animePlayer.Play();
    }

    public void ResetRadialProgress()
    {
        progressUI.gameObject.SetActive(false);
        animePlayer.Stop();
        progressUI.fillAmount = 0;
    }

    private void Update()
    {
        if(progressUI.gameObject.activeSelf)
        {
            Pose jointPose = gameController.getHandJointPose(HandJointID.IndexTip);
            transform.position = jointPose.position + Vector3.up * 0.015f + jointPose.up * 0.01f;
        }
    }
}
