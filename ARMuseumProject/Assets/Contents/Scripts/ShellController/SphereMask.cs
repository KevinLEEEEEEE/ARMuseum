using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class SphereMask : MonoBehaviour
{
    public GameController_S2 gameController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = gameController.getHandJointPose(HandJointID.IndexTip).position;
    }
}
