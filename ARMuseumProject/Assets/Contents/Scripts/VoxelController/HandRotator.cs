using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRotator : MonoBehaviour
{
    [SerializeField] private Transform rotator_R;
    [SerializeField] private Transform rotator_L;
    [SerializeField] private Transform voxelBlock;
    [SerializeField] private HandJointID bindJoint;
    [SerializeField] private float anglePerMeter;
    [SerializeField] private int angleStep;
    [SerializeField] private AudioClip audioClip_startRotating;
    [SerializeField] private AudioClip audioClip_stopRotating;

    private AudioGenerator audioSource_startRotating;
    private AudioGenerator audioSource_stopRotating;
    private bool canRotate;
    private bool isRotating;
    private HandEnum rotatingHand;
    private Vector3 startPosition;
    private float startRotationY;

    void Start()
    {
        audioSource_startRotating = new AudioGenerator(gameObject, audioClip_startRotating);
        audioSource_stopRotating = new AudioGenerator(gameObject, audioClip_stopRotating);

        Reset();
    }

    private void Reset()
    {
        canRotate = false;
        isRotating = false;
        rotator_L.gameObject.SetActive(false);
        rotator_R.gameObject.SetActive(false);
    }

    public void EnableRotator()
    {
        canRotate = true;

        audioSource_startRotating.Reload();
        audioSource_stopRotating.Reload();
    }

    public void DisableRotator()
    {
        Reset();

        audioSource_startRotating.Unload();
        audioSource_stopRotating.Unload();
    }

    public void StartRotating(HandEnum handEnum)
    {
        if (!isRotating)
        {
            isRotating = true;
            rotatingHand = handEnum;
            startPosition = NRInput.Hands.GetHandState(rotatingHand).GetJointPose(bindJoint).position;
            startRotationY = voxelBlock.localEulerAngles.y;
            audioSource_startRotating.Play();
        }
    }

    public void StopRotating(HandEnum handEnum)
    {
        if (isRotating && rotatingHand == handEnum)
        {
            isRotating = false;
            rotatingHand = HandEnum.None;
            audioSource_stopRotating.Play();
        }
    }

    private void UpdateVoxelRotation()
    {
        Vector3 endPosition = NRInput.Hands.GetHandState(rotatingHand).GetJointPose(bindJoint).position;
        float eulerAngle = GetEulerAngle(startPosition, endPosition);
        bool isLeft = IsLeft(startPosition, transform.position, endPosition);
        float finalAngle = startRotationY + (isLeft ? eulerAngle : -eulerAngle);

        voxelBlock.localEulerAngles = new Vector3(0, finalAngle, 0);
    }

    private int GetEulerAngle(Vector3 start, Vector3 end)
    {
        float distance = Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z)) / angleStep;

        return Mathf.FloorToInt(distance * anglePerMeter) * angleStep;
    }

    private bool IsLeft(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) > 0;
    }

    private void UpdateRotator(HandEnum handEnum, Transform rotator)
    {
        if (rotator == null)
            return;

        var handState = NRInput.Hands.GetHandState(handEnum);
        if (handState == null || !handState.isTracked)
        {
            rotator.gameObject.SetActive(false);
            StopRotating(handEnum);
            return;
        }

        rotator.gameObject.SetActive(true);
        var grabPose = handState.GetJointPose(bindJoint);
        rotator.transform.SetPositionAndRotation(grabPose.position, grabPose.rotation);
    }

    void Update()
    {
        if (!canRotate) return;

        UpdateRotator(HandEnum.RightHand, rotator_R);
        UpdateRotator(HandEnum.LeftHand, rotator_L);

        if(isRotating)
        {
            UpdateVoxelRotation();
        }
    }
}
