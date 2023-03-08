using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;

public class Progress : MonoBehaviour
{
    [SerializeField] private HandEnum followHand;
    [SerializeField] private HandJointID followJoint;
    [SerializeField] private float forwardDistance;
    [SerializeField] private float upwardDistance;

    private HandState targetHandState;
    private Image progressUI;
    private Animation animePlayer;
    private bool isActive = false;

    private void Start()
    {
        targetHandState = NRInput.Hands.GetHandState(followHand);
        animePlayer = transform.GetComponent<Animation>();
        progressUI = transform.GetChild(0).GetComponent<Image>();
        ResetRadialProgress();
    }

    public void StartRadialProgress()
    {
        progressUI.gameObject.SetActive(true);
        animePlayer.Play();
        isActive = true;
    }

    public void ResetRadialProgress()
    {
        progressUI.gameObject.SetActive(false);
        animePlayer.Stop();
        isActive = false;
    }

    private void Update()
    {
        if(isActive)
        {
            Pose jointPose = targetHandState.GetJointPose(followJoint);
            transform.position = jointPose.position + Vector3.up * upwardDistance + jointPose.up * forwardDistance;
        }
    }
}
