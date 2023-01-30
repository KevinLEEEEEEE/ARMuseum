using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController_S2 : MonoBehaviour
{
    public Scene1 scene1;
    public Scene2 scene2;
    public Transform planeDetector;
    public AudioSource env_Wind;
    public float ambientSoundBasicVolume;

    private EventAnchor confirmedEventAnchor;
    private float ambientSoundVolume;
    private HandEnum domainHand = HandEnum.RightHand;

    void Start()
    {
        NRInput.RaycastersActive = false;

        //scene1.StartAct(new Vector3(0, 0, 1));

        scene2.StartScene(new Vector3(0, 0, 0), new Vector3(0, 1, 0));
    }

    public void UpdateEventAnchor(EventAnchor anchor)
    {
        planeDetector.GetComponent<PlaneDetector>().LockTargetPlane(anchor.GetHitObject());
        confirmedEventAnchor = anchor;
    }

    public void NextScene()
    {
        scene2.StartScene(confirmedEventAnchor.GetHitPoint(), confirmedEventAnchor.GetHitDirection());
    }

    public void PlayAmbientSound()
    {
        env_Wind.Play();
        env_Wind.volume = 0;
        UpdateAmbientSoundVolume(0);
    }

    public void StopAmbientSound()
    {
        env_Wind.Stop();
        env_Wind.volume = 0;
        UpdateAmbientSoundVolume(0);
    }

    public void UpdateAmbientSoundVolume(float volume)
    {
        ambientSoundVolume = volume + ambientSoundBasicVolume;
    }

    private void UpdateAmbientSoundVolume()
    {
        float delta = ambientSoundVolume - env_Wind.volume;
        delta *= Time.deltaTime;
        env_Wind.volume += delta;
    }

    public HandState GetDomainHandState()
    {
        return NRInput.Hands.GetHandState(domainHand);
    }

    public Pose getHandJointPose(HandJointID jointID)
    {
        return GetDomainHandState().GetJointPose(jointID);
    }

    public bool getHandTrackingState()
    {
        return GetDomainHandState().isTracked;
    }

    public void StartPlaneHint()
    {
        foreach (Transform plane in planeDetector)
        {
            plane.GetComponent<Animation>().Play();
        }
    }

    public void StopPlaneHint()
    {
        foreach (Transform plane in planeDetector)
        {
            plane.GetComponent<Animation>().Stop();
        }
    }

    void Update()
    {
        if(env_Wind.isPlaying)
        {
            UpdateAmbientSoundVolume();
        }
    }
}
