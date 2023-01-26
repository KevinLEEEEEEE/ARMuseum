using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController_S2 : MonoBehaviour
{
    public Act1 act1;
    //public Act2 act2;
    public Transform planeDetector;
    //public PlaneDetector planeDetector;

    // Start is called before the first frame update
    void Start()
    {
        NRInput.RaycastersActive = false;

        act1.StartAct();
    }

    public void SetStoryAnchor(RaycastHit hit)
    {
        //planeDetector.LockTargetPlane(hit.collider.gameObject);
        //ConfirmAnchoredPlane(hit.point);
    }

    public void NextAct()
    {
        Debug.Log("NextAct");
    }

    public HandState GetDomainHandState()
    {
        return NRInput.Hands.GetHandState(HandEnum.RightHand);
    }

    public void StartPlaneHint()
    {
        InvokeRepeating("RunHintAnimation", 0, 3.5f);
    }

    private void RunHintAnimation()
    {
        foreach (Transform plane in planeDetector)
        {
            plane.GetComponent<Animation>().Play();
        }
    }

    public void StopPlaneHint()
    {
        CancelInvoke("RunHintAnimation");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
