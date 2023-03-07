using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class OrbNearby : MonoBehaviour
{
    private HandState rightHandState;
    private HandState leftHandState;
    private Material _material;
    private Material CurrentMaterial
    {
        get
        {
            if (_material == null)
            {
                _material = transform.GetComponent<MeshRenderer>().material;
            }
            return _material;
        }
    }
    private const float MaxDistance = 0.12f;
    private const float MinDistance = 0.05f;
    private const float MaxRimPower = 4f;
    private const float MinRimPower = 2.5f;

    private void OnEnable()
    {
        ResetAll();
    }

    private void OnDisable()
    {
        ResetAll();
    }

    private void Start()
    {
        rightHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        leftHandState = NRInput.Hands.GetHandState(HandEnum.LeftHand);
    }

    public void ResetAll()
    {
        CurrentMaterial.SetFloat("_RimLight", MaxRimPower);
    }

    void Update()
    {
        Vector3 rightHandIndexPosition = rightHandState.GetJointPose(HandJointID.IndexTip).position;
        Vector3 leftHandIndexPosition = leftHandState.GetJointPose(HandJointID.IndexTip).position;
        float rightHandIndexDistance = Vector3.Distance(rightHandIndexPosition, transform.position);
        float leftHandIndexDistance = Vector3.Distance(leftHandIndexPosition, transform.position);

        float nearestDistance = Mathf.Min(rightHandIndexDistance, leftHandIndexDistance);

        if (nearestDistance <= MaxDistance)
        {
            float x = nearestDistance - MinDistance;
            float a = (MaxRimPower - MinRimPower) / (MaxDistance - MinDistance);

            CurrentMaterial.SetFloat("_RimPower", a * x + MinRimPower);
        }
    }
}
