using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fraktalia.VoxelGen.Modify;
using UnityEngine;
using UnityEngine.Serialization;
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
    public VoxelBoundary voxelBoundary;
    public GameObject fingerTorchPrefab;
    public GameObject fingerHitVisualizerPrefab;
    public float maxVisualDistance;
    public float minVisualDistance;
    public AudioClip audioClip_voxelOperation;
    public AnimationCurve Distance_IDCurve;

    private readonly HandEnum addVoxelHand = HandEnum.RightHand;
    private readonly HandGesture addVoxelHandGesture = HandGesture.Point;
    private readonly HandEnum subVoxelHand = HandEnum.LeftHand;
    private readonly HandGesture subVoxelHandGesture = HandGesture.Point;
    private readonly int layerMask = 1 << 13;
    private HandState addVoxelHandState;
    private HandState subVoxelHandState;
    private GameObject addFingerTorch;
    private GameObject subFingerTorch;
    private GameObject addVisualizer;
    private GameObject subVisualizer;
    private VoxelModifier modifier;
    private AudioGenerator audioSource_voxelOperation;
    private bool isFirstModify;
    private bool canAddVoxel;
    private bool canSubVoxel;
    private bool canModify;
    private Vector3 addPrePosition = new(0, 0, 0);
    private Vector3 subPrePosition = new(0, 0, 0);
    private int defaultIDValue;

    private void Start()
    {
        modifier = GetComponent<VoxelModifier>();
        audioSource_voxelOperation = new AudioGenerator(gameObject, audioClip_voxelOperation, true);

        leftHandColliderEvent.triggerEnterListener += LeftIndexTipTrigger;
        rightHandColliderEvent.triggerEnterListener += RightIndexTipTrigger;
        voxelBoundary.leftHandExitBoundaryListener += LeftIndexTipExitBoundary;
        voxelBoundary.rightHandExitBoundaryListener += RightIndexTipExitBoundary;

        addFingerTorch = Instantiate(fingerTorchPrefab, transform);
        subFingerTorch = Instantiate(fingerTorchPrefab, transform);
        addVisualizer = Instantiate(fingerHitVisualizerPrefab, transform);
        subVisualizer = Instantiate(fingerHitVisualizerPrefab, transform);

        addVoxelHandState = NRInput.Hands.GetHandState(addVoxelHand);
        subVoxelHandState = NRInput.Hands.GetHandState(subVoxelHand);

        Reset();
    }

    private void Reset()
    {
        isFirstModify = true;
        canAddVoxel = false;
        canSubVoxel = false;
        canModify = false;
        defaultIDValue = modifier.ID;
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

    private void LeftIndexTipExitBoundary()
    {
        canSubVoxel = false;
    }

    private void RightIndexTipTrigger()
    {
        canAddVoxel = true;
    }

    private void RightIndexTipExitBoundary()
    {
        canAddVoxel = false;
    }


    private void ModifyAtPosition(VoxelModifyMode mode, Vector3 pos, float id, float radius = 0.2f)
    {
        if(isFirstModify)
        {
            voxelController.FirstModify();
            isFirstModify = false;
        }

        if (modifier.Mode != mode)
        { 
            modifier.SetModificationMode(mode);
        }

        modifier.SetID(id);
        modifier.SetRadius(radius);
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

        if (addVoxelHandState.currentGesture == addVoxelHandGesture)
        {
            Vector3 addPosition = addVoxelHandState.GetJointPose(HandJointID.IndexTip).position;
            NearestHitResult addResult = GenerateSphereRaycast(addPosition, maxVisualDistance, layerMask);
            UpdateHitVisualizer(addVisualizer, addResult, canAddVoxel);
            UpdateFingerTorch(addFingerTorch, addPosition);

            if (canAddVoxel)
            {
                float distance = Vector3.Distance(addPrePosition, addPosition);
                float id = Distance_IDCurve.Evaluate(distance) * defaultIDValue;
                ModifyAtPosition(VoxelModifyMode.Additive, addPosition, id, 0.2f);
            }

            addPrePosition = addPosition;
        } else
        {
            canAddVoxel = false;
            addPrePosition = Vector3.zero;
            addVisualizer.SetActive(false);
            addFingerTorch.SetActive(false);
        }

        if (subVoxelHandState.currentGesture == subVoxelHandGesture)
        {
            Vector3 subPosition = subVoxelHandState.GetJointPose(HandJointID.IndexTip).position;
            NearestHitResult subResult = GenerateSphereRaycast(subPosition, maxVisualDistance, layerMask);
            UpdateHitVisualizer(subVisualizer, subResult, canSubVoxel);
            UpdateFingerTorch(subFingerTorch, subPosition);

            if (canSubVoxel)
            {
                float distance = Vector3.Distance(subPrePosition, subPosition);
                float id = Distance_IDCurve.Evaluate(distance) * defaultIDValue;

                // 删除的尺寸比添加的稍大一些，能够将原有轨迹擦得更干净，符合预期
                ModifyAtPosition(VoxelModifyMode.Subtractive, subPosition, id, 0.25f);
            }

            subPrePosition = subPosition;
        } else
        {
            canSubVoxel = false;
            subVisualizer.SetActive(false);
            subFingerTorch.SetActive(false);
        }

        // Play audio if operating

        if(canAddVoxel || canSubVoxel)
        {
            audioSource_voxelOperation.Play();
        } else
        {
            audioSource_voxelOperation.Stop();
        }
    }
}
