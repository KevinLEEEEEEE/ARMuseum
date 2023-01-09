using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using TMPro;

public class DeleteOrb : MonoBehaviour
{
    private bool isActive = false;
    private Material _material;
    private Material CurrentMaterial
    {
        get
        {
            if(_material == null)
            {
                _material = transform.GetComponent<MeshRenderer>().material;
            }
            return _material;
        }
    }
    private const float MaxDistance = 0.1f;
    private const float MinDistance = 0.03f;
    private const float MaxRimPower = 3.5f;
    private const float MinRimPower = 1.8f;
    public TextMeshPro _TextMesh;

    private void OnEnable()
    {
        ResetAll();
    }

    public void ResetAll()
    {
        CurrentMaterial.SetFloat("_RimLight", MaxRimPower);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Player] Trigger Delete Orb: " + transform.name);

        if (isActive)
        {
            CancelInvoke("ResetDeleteState");
            ResetDeleteState();
            SendMessageUpwards("DeleteOrb");
            // ³¹µ×É¾³ý
        }
        else
        {
            SendMessageUpwards("OpenOrbMessage");
            _TextMesh.text = "Confirm?";
            Invoke("ResetDeleteState", 2f);
        }

        isActive = !isActive;
    }

    private void ResetDeleteState()
    {
        _TextMesh.text = "Delete";
    }

    void Update()
    {
        HandState rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        HandState leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);

        Vector3 rightHandIndexPosition = rightHandState.GetJointPose(HandJointID.IndexTip).position;
        Vector3 leftHandIndexPosition = leftHandState.GetJointPose(HandJointID.IndexTip).position;
        float rightHandIndexDistance = Vector3.Distance(rightHandIndexPosition, transform.position);
        float leftHandIndexDistance = Vector3.Distance(leftHandIndexPosition, transform.position);

        float nearestDistance = Mathf.Min(rightHandIndexDistance, leftHandIndexDistance);

        if(nearestDistance <= MaxDistance)
        {
            float x = nearestDistance - MinDistance;
            float a = (MaxRimPower - MinRimPower) / (MaxDistance - MinDistance);

            CurrentMaterial.SetFloat("_RimPower", a * x + MinRimPower);
        }
    }
}
