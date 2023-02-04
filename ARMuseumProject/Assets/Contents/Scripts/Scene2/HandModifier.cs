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
    public HandEnum addVoxelHand;
    public HandGesture addVoxelHandGesture;
    public HandEnum subVoxelHand;
    public HandGesture subVoxelHandGesture;
    public GameObject addFingerTorch;
    public GameObject subFingerTorch;
    public GameObject addVisualizer;
    public GameObject subVisualizer;
    public float maxVisualDistance;
    public float minVisualDistance;
    public float addTriggerDistance;
    private GameObject follower;
    private VoxelModifier modifier;
    private int layerMask = 1 << 13;
    private bool isAddActive = false;
    private bool isFirstAdd = true;
    private bool isFirstSub = true;

    private void Start()
    {
        modifier = GetComponent<VoxelModifier>();
        follower = new GameObject("Follower");
    }

    private void ModifyAtPosition(VoxelModifyMode mode, Vector3 pos)
    {
        if(mode == VoxelModifyMode.Additive || isFirstAdd)
        {
            SendMessageUpwards("AddVoxel");
            isFirstAdd = false;
        } else if (mode == VoxelModifyMode.Subtractive || isFirstSub)
        {
            SendMessageUpwards("SubVoxel");
            isFirstSub = true;
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
        NearestHitResult result = new NearestHitResult(false, new RaycastHit());

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

    private void UpdateFingerTorch(GameObject torch, Vector3 pos, bool isGesture)
    {
        if (isGesture)
        {
            if (!torch.activeSelf)
            {
                torch.SetActive(true);
            }
            torch.transform.position = pos;
        }
        else
        {
            torch.SetActive(false);
        }
    }

    private void UpdateHitVisualizer(GameObject visualizer, NearestHitResult result)
    {
        if(result.isHitted && result.hitResult.distance >= minVisualDistance)
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

    private void UpdateFollower(Vector3 pos, bool isInstant)
    {
        follower.transform.position = isInstant ? pos : Vector3.MoveTowards(follower.transform.position, pos, 0.04f);
    }

    void Update()
    {
        HandState addVoxelHandState = NRInput.Hands.GetHandState(addVoxelHand);
        HandState subVoxelHandState = NRInput.Hands.GetHandState(subVoxelHand);
        bool isAddGesture = addVoxelHandState.currentGesture == addVoxelHandGesture;
        bool isSubGesture = subVoxelHandState.currentGesture == subVoxelHandGesture;
        Vector3 addPosition = addVoxelHandState.GetJointPose(HandJointID.IndexTip).position;
        Vector3 subPosition = subVoxelHandState.GetJointPose(HandJointID.IndexTip).position;

        UpdateFingerTorch(addFingerTorch, addPosition, isAddGesture);
        UpdateFingerTorch(subFingerTorch, subPosition, isSubGesture);

        if(isAddGesture)
        {
            if(isAddActive)
            {  
                ModifyAtPosition(VoxelModifyMode.Additive, follower.transform.position);
                UpdateFollower(addPosition, false);
            } else
            {
                NearestHitResult addResult = GenerateSphereRaycast(addPosition, maxVisualDistance, layerMask);

                UpdateHitVisualizer(addVisualizer, addResult);

                if (addResult.isHitted && addResult.hitResult.distance < addTriggerDistance)
                {
                    isAddActive = true; // 触碰后开启 Add 能力，直至下次手势变动取消 Add，确保连贯性
                    addVisualizer.SetActive(false);
                    UpdateFollower(addResult.hitResult.point, true);
                }
            }
        } else
        {
            addVisualizer.SetActive(false);
            isAddActive = false;
        }

        if (isSubGesture)
        {
            NearestHitResult subResult = GenerateSphereRaycast(subPosition, 0.1f, layerMask);

            UpdateHitVisualizer(subVisualizer, subResult);
            
            ModifyAtPosition(VoxelModifyMode.Subtractive, subPosition);
        } else
        {
            subVisualizer.SetActive(false);
        }
    }
}
