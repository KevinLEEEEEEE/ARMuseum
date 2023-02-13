using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fraktalia.VoxelGen.Modify;
using UnityEngine;
using NRKernal;

public class NearestHitResult
{
    public bool isHitted;
    public RaycastHit hitResult;
    
    public NearestHitResult(bool hitState, RaycastHit hit)
    {
        isHitted = hitState;
        hitResult = hit;
    }
}

public class HandModifier : MonoBehaviour
{
    public ColliderEvent leftHandColliderEvent;
    public ColliderEvent rightHandColliderEvent;
    public VoxelController voxelController;
    public GameObject fingerTorchPrefab;
    public GameObject fingerHitVisualizerPrefab;
    public float maxVisualDistance;
    public float minVisualDistance;

    private readonly HandEnum addVoxelHand = HandEnum.RightHand;
    private readonly HandGesture addVoxelHandGesture = HandGesture.Point;
    private readonly HandEnum subVoxelHand = HandEnum.LeftHand;
    private readonly HandGesture subVoxelHandGesture = HandGesture.Point;
    private readonly int layerMask = 1 << 13;
    private GameObject addFingerTorch;
    private GameObject subFingerTorch;
    private GameObject addVisualizer;
    private GameObject subVisualizer;
    private VoxelModifier modifier;
    private bool isFirstModify;
    private bool canAddVoxel;
    private bool canSubVoxel;
    private bool canModify;

    private void Start()
    {
        modifier = GetComponent<VoxelModifier>();

        leftHandColliderEvent.triggerEnterListener += LeftIndexTipTrigger;
        rightHandColliderEvent.triggerEnterListener += RightIndexTipTrigger;

        addFingerTorch = Instantiate(fingerTorchPrefab, transform);
        subFingerTorch = Instantiate(fingerTorchPrefab, transform);
        addVisualizer = Instantiate(fingerHitVisualizerPrefab, transform);
        subVisualizer = Instantiate(fingerHitVisualizerPrefab, transform);

        isFirstModify = true;
        canAddVoxel = false;
        canSubVoxel = false;
        canModify = false;
    }

    public void EnableModify()
    {
        canModify = true;
    }

    public void DisableModify()
    {
        canModify = false;
        addVisualizer.SetActive(false);
        addFingerTorch.SetActive(false);
        subVisualizer.SetActive(false);
        subFingerTorch.SetActive(false);
    }

    private void LeftIndexTipTrigger()
    {
        canSubVoxel = true;
    }

    private void RightIndexTipTrigger()
    {
        canAddVoxel = true;
    }

    private void ModifyAtPosition(VoxelModifyMode mode, Vector3 pos)
    {
        if(isFirstModify)
        {
            voxelController.StopInstruction();
            isFirstModify = false;
        }

        if (modifier.Mode != mode)
        {
            modifier.SetModificationMode(mode);
        }
        modifier.ModifyAtPos(pos);
    }

    private Vector3[] GetSphereDirections(int numDirections)
    {
        var pts = new Vector3[numDirections];
        var inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        var off = 2f / numDirections;

        foreach (var k in Enumerable.Range(0, numDirections))
        {
            var y = k * off - 1 + (off / 2);
            var r = Mathf.Sqrt(1 - y * y);
            var p = k * inc;
            var x = (float)(Mathf.Cos(p) * r);
            var z = (float)(Mathf.Sin(p) * r);
            pts[k] = new Vector3(x, y, z);
        }
        return pts;
    }

    private NearestHitResult GenerateSphereRaycast(Vector3 pos, float distance, int mask)
    {
        NearestHitResult result = new(false, new RaycastHit());

        foreach (var direction in GetSphereDirections(360))
        {
            if (Physics.Raycast(pos, direction, out var hit, distance, mask))
            {
                if (!result.isHitted || hit.distance < result.hitResult.distance)
                {
                    result = new NearestHitResult(true, hit);
                }
            }
        }

        return result;
    }

    private void UpdateFingerTorch(GameObject torch, Vector3 pos)
    {
        if (!torch.activeSelf)
        {
            torch.SetActive(true);
        }

        torch.transform.position = pos;
    }

    private void UpdateHitVisualizer(GameObject visualizer, NearestHitResult result, bool isModifying)
    {
        if(result.isHitted && result.hitResult.distance >= minVisualDistance && !isModifying)
        {
            if(visualizer.activeSelf)
            {
                visualizer.transform.position = Vector3.Lerp(visualizer.transform.position, result.hitResult.point, 0.05f);
                visualizer.transform.forward = Vector3.Lerp(visualizer.transform.forward, result.hitResult.normal, 0.05f);
            } else
            {
                visualizer.transform.position = result.hitResult.point;
                visualizer.transform.forward = result.hitResult.normal;
                visualizer.SetActive(true);
            }
        } else
        {
            visualizer.SetActive(false);
        }
    }

    void Update()
    {
        if(!canModify)
        {
            return;
        }

        Vector3 addPosition = NRInput.Hands.GetHandState(addVoxelHand).GetJointPose(HandJointID.IndexTip).position;
        Vector3 subPosition = NRInput.Hands.GetHandState(subVoxelHand).GetJointPose(HandJointID.IndexTip).position;

        if (NRInput.Hands.GetHandState(addVoxelHand).currentGesture == addVoxelHandGesture)
        {
            NearestHitResult addResult = GenerateSphereRaycast(addPosition, maxVisualDistance, layerMask);
            UpdateHitVisualizer(addVisualizer, addResult, canAddVoxel);
            UpdateFingerTorch(addFingerTorch, addPosition);

            if (canAddVoxel)
            {
                ModifyAtPosition(VoxelModifyMode.Additive, addPosition);
            }
        } else
        {
            canAddVoxel = false;
            addVisualizer.SetActive(false);
            addFingerTorch.SetActive(false);
        }

        if (NRInput.Hands.GetHandState(subVoxelHand).currentGesture == subVoxelHandGesture)
        {
            NearestHitResult subResult = GenerateSphereRaycast(subPosition, maxVisualDistance, layerMask);
            UpdateHitVisualizer(subVisualizer, subResult, canSubVoxel);
            UpdateFingerTorch(subFingerTorch, subPosition);

            if (canSubVoxel)
            {
                ModifyAtPosition(VoxelModifyMode.Additive, subPosition);
            }
        } else
        {
            canSubVoxel = false;
            subVisualizer.SetActive(false);
            subFingerTorch.SetActive(false);
        }
    }
}
