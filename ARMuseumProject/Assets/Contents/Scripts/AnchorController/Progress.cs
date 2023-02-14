using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NRKernal;

public class Progress : MonoBehaviour
{
    public GameController_Historical gameController;
    public HandJointID followJoint;
    public float forwardDistance;
    public float upwardDistance;

    private Image progressUI;
    private Animation animePlayer;
    private bool isActive = false;
    //private float timer = 0.0f;

    private void Start()
    {
        animePlayer = transform.GetComponent<Animation>();
        progressUI = transform.GetChild(0).GetComponent<Image>();
        ResetRadialProgress();

        //StartCoroutine(RunInSeconds(2));
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
        //SetValue(0);
        isActive = false;
    }

    //public void SetValue(float percentage)
    //{
    //    progressUI.fillAmount = percentage;
    //}

    //private IEnumerator RunInSeconds(float duration)
    //{
    //    float t = 0;

    //    while(t < duration)
    //    {
    //        t += Time.deltaTime;
            

    //        Debug.Log(t / duration);

    //        yield return null;
    //    }
    //}

    private void Update()
    {
        if(isActive)
        {
            Pose jointPose = gameController.GetDomainHandState().GetJointPose(followJoint);
            transform.position = jointPose.position + Vector3.up * upwardDistance + jointPose.up * forwardDistance;
        }
    }
}
