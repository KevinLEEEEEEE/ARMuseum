using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class ShellMatchManager : MonoBehaviour
{
    [SerializeField] private ShellController _shellController;
    [SerializeField] private GameObject lightBulb;
    [SerializeField] private GameObject flameMask;
    [SerializeField] private float matchPositionOffset;

    private SphereCollider sphereColliderComp;
    private HandState rightHandState;
    private HandState leftHandState;
    private bool canActiveMatch;

    private void Start()
    {
        _shellController.shellStateListener += ShellStateHandler;
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
        sphereColliderComp = GetComponent<SphereCollider>();

        Reset();
    }

    public void Reset()
    {
        DisableMatch();
        canActiveMatch = false;
        sphereColliderComp.enabled = false;
        transform.position = new Vector3(0, 5, 0);
    }

    private void ShellStateHandler(ShellNode node)
    {
        if(node == ShellNode.ToBurn)
        {
            canActiveMatch = true;
            sphereColliderComp.enabled = true;
        } else if(node == ShellNode.Burning)
        {
            Reset();
        }
    }

    public void DisableMatch()
    {
        lightBulb.SetActive(false);
        flameMask.SetActive(false); 
    }

    public void EnableMatch()
    {
        if(canActiveMatch)
        {
            lightBulb.SetActive(true);
            flameMask.SetActive(true);
        }
    }

    private void Update()
    {
        if(canActiveMatch)
        {
            HandState holdingHand = rightHandState.isPinching ? rightHandState : leftHandState;
            Pose indexTipPose = holdingHand.GetJointPose(HandJointID.IndexTip);

            transform.position = indexTipPose.position + indexTipPose.up * matchPositionOffset;
        }
    }
}
