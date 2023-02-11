using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NRKernal.NRExamples;
using NRKernal;

public class GameController_S2 : MonoBehaviour
{
    public AnchorController anchorController;
    public Scene2 scene2;
    public ShellController shell;
    public Transform[] eventAnchorListener;
    public GameObject planeDetector;
    public GameObject groundMask;
    public AudioSource env_Wind;
    public float ambientSoundBasicVolume;

    private EventAnchor confirmedEventAnchor;
    private float ambientSoundVolume;
    private HandEnum domainHand = HandEnum.RightHand;

    void Start()
    {
        NRInput.RaycastersActive = false;

        anchorController.StartAct(new Vector3(0, 0, 1));

        //scene2.StartScene(new Vector3(0, 0, 1), new Vector3(0, 0, 10));

        //shell.InitShell();
    }

    public void SetEventAnchor(EventAnchor anchor)
    {
        planeDetector.GetComponent<PlaneDetector>().LockTargetPlane(anchor.GetHitObject());

        Vector3 position = anchor.GetCorrectedHitPoint();
        Vector3 forward = anchor.GetHitDirection();

        foreach (Transform trans in eventAnchorListener)
        {
            trans.position = position;
            trans.forward = forward;
        }

        groundMask.SetActive(true);
    }

    public void NextScene()
    {
        scene2.StartScene(confirmedEventAnchor.GetCorrectedHitPoint(), confirmedEventAnchor.GetHitDirection());
    }

    public void FinshModeling()
    {
        shell.SetTransform(confirmedEventAnchor.GetCorrectedHitPoint(), confirmedEventAnchor.GetHitDirection());
        shell.InitShell();
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

    public bool GetHandTrackingState()
    {
        return GetDomainHandState().isTracked;
    }

    public bool GetHandPinchState()
    {
        return GetDomainHandState().isPinching;
    }

    public void StartPlaneHint()
    {
        foreach (Transform plane in planeDetector.transform)
        {
            plane.GetComponent<Animation>().Play();
        }
    }

    public void StopPlaneHint()
    {
        foreach (Transform plane in planeDetector.transform)
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
