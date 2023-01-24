using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GameController_S2 : MonoBehaviour
{
    public Act1 act1;
    public PlaneDetector planeDetector;

    // Start is called before the first frame update
    void Start()
    {
        NRInput.RaycastersActive = false;

        act1.StartAct();
    }

    public void SetStoryAnchor(RaycastHit hit)
    {
        planeDetector.LockTargetPlane(hit.collider.gameObject);
        //ConfirmAnchoredPlane(hit.point);
    }

    public HandState GetDomainHandState()
    {
        return NRInput.Hands.GetHandState(HandEnum.RightHand);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
