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
    [SerializeField] private VoxelController voxelController;
    [SerializeField] private VoxelBoundary voxelBoundary;
    [SerializeField] private GameObject fingerTorchPrefab;
    //[SerializeField] private float maxVisualDistance;
    //[SerializeField] private float minVisualDistance;
    [SerializeField] private float addRadius;
    [SerializeField] private float subRadius;
    [SerializeField] private float defaultIDValue;
    [SerializeField] private AnimationCurve Distance_IDCurve;
    [SerializeField] private AudioClip audioClip_voxelOperation;

    public bool mirrorMode;

    private readonly HandEnum addVoxelHand = HandEnum.RightHand;
    private readonly HandGesture addVoxelHandGesture = HandGesture.Point;
    private readonly HandEnum subVoxelHand = HandEnum.LeftHand;
    private readonly HandGesture subVoxelHandGesture = HandGesture.Point;
    private HandState addVoxelHandState;
    private HandState subVoxelHandState;
    private GameObject addFingerTorch;
    private GameObject subFingerTorch;
    private VoxelModifier modifier;
    private AudioGenerator audioSource_voxelOperation;
    private bool isFirstModify = true;
    private bool canAddVoxel;
    private bool canSubVoxel;
    private bool canModify;
    private Vector3 addPrePosition = new(0, 0, 0);
    private Vector3 subPrePosition = new(0, 0, 0);

    public AudioClip AudioClip_voxelOperation { get => audioClip_voxelOperation; set => audioClip_voxelOperation = value; }

    private void Start()
    {
        modifier = GetComponent<VoxelModifier>();
        audioSource_voxelOperation = new AudioGenerator(gameObject, AudioClip_voxelOperation, true);

        ColliderEvent leftHandColliderEvent = NRInput.AnchorsHelper.GetAnchor(ControllerAnchorEnum.LeftColliderEntity).GetComponent<ColliderEvent>();
        ColliderEvent rightHandColliderEvent = NRInput.AnchorsHelper.GetAnchor(ControllerAnchorEnum.RightColliderEntity).GetComponent<ColliderEvent>();
        addVoxelHandState = NRInput.Hands.GetHandState(addVoxelHand);
        subVoxelHandState = NRInput.Hands.GetHandState(subVoxelHand);

        addFingerTorch = Instantiate(fingerTorchPrefab, transform);
        subFingerTorch = Instantiate(fingerTorchPrefab, transform);

        leftHandColliderEvent.triggerEnterListener += LeftIndexTipTrigger;
        rightHandColliderEvent.triggerEnterListener += RightIndexTipTrigger;
        voxelBoundary.leftHandExitBoundaryListener += LeftIndexTipExitBoundary;
        voxelBoundary.rightHandExitBoundaryListener += RightIndexTipExitBoundary;

        Reset();
    }

    private void Reset()
    {
        canModify = false;
        canAddVoxel = false;
        canSubVoxel = false;
        addPrePosition = Vector3.zero;
        subPrePosition = Vector3.zero;

        addFingerTorch.SetActive(false);
        subFingerTorch.SetActive(false);
    }

    public void EnableModify()
    {
        canModify = true;
        audioSource_voxelOperation.Reload();
    }

    public void DisableModify()
    {
        Reset();
        audioSource_voxelOperation.Unload();
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

        if (modifier.Mode != mode) modifier.SetModificationMode(mode);
        if(modifier.ID != id) modifier.SetID(id);
        if(modifier.Radius != radius) modifier.SetRadius(radius);

        modifier.ModifyAtPos(pos);

        if(mirrorMode)
        {
            modifier.ModifyAtPos(new(2 * transform.position.x - pos.x, pos.y, 2 * transform.position.z - pos.z));
        }
    }

    private void UpdateFingerTorch(GameObject torch, Vector3 pos)
    {
        if (!torch.activeSelf)
        {
            torch.SetActive(true);
        }

        torch.transform.position = pos;
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
            UpdateFingerTorch(addFingerTorch, addPosition);

            if (canAddVoxel)
            {
                // 首次触碰时，初始移动距离设置为0，避免造成一触碰就寄一大坨的效果
                float distance = addPrePosition == Vector3.zero ? 0 : Vector3.Distance(addPrePosition, addPosition);
                float id = Distance_IDCurve.Evaluate(distance) * defaultIDValue;
                ModifyAtPosition(VoxelModifyMode.Additive, addPosition, id, addRadius);
            }

            addPrePosition = addPosition;
        } else
        {
            canAddVoxel = false;
            addPrePosition = Vector3.zero;
            addFingerTorch.SetActive(false);
        }

        if (subVoxelHandState.currentGesture == subVoxelHandGesture)
        {
            Vector3 subPosition = subVoxelHandState.GetJointPose(HandJointID.IndexTip).position;
            UpdateFingerTorch(subFingerTorch, subPosition);

            if (canSubVoxel)
            {
                float distance = subPrePosition == Vector3.zero ? 0 : Vector3.Distance(subPrePosition, subPosition);
                float id = Distance_IDCurve.Evaluate(distance) * defaultIDValue;
                ModifyAtPosition(VoxelModifyMode.Subtractive, subPosition, id, subRadius);
            }

            subPrePosition = subPosition;
        } else
        {
            canSubVoxel = false;
            subPrePosition = Vector3.zero;
            subFingerTorch.SetActive(false);
        }

        // Play audio if operating
        if (canAddVoxel || canSubVoxel)
        {
            audioSource_voxelOperation.Play();
        } else
        {
            audioSource_voxelOperation.Stop();
        }
    }
}




//private Vector3[] GetSphereDirections(int numDirections)
//{
//    var pts = new Vector3[numDirections];
//    var inc = Mathf.PI * (3 - Mathf.Sqrt(5));
//    var off = 2f / numDirections;

//    foreach (var k in Enumerable.Range(0, numDirections))
//    {
//        var y = k * off - 1 + (off / 2);
//        var r = Mathf.Sqrt(1 - y * y);
//        var p = k * inc;
//        var x = (float)(Mathf.Cos(p) * r);
//        var z = (float)(Mathf.Sin(p) * r);
//        pts[k] = new Vector3(x, y, z);
//    }
//    return pts;
//}

//private NearestHitResult GenerateSphereRaycast(Vector3 pos, float distance, int mask)
//{
//    NearestHitResult result = new(false, new RaycastHit());

//    foreach (var direction in GetSphereDirections(360))
//    {
//        if (Physics.Raycast(pos, direction, out var hit, distance, mask))
//        {
//            if (!result.isHitted || hit.distance < result.hitResult.distance)
//            {
//                result = new NearestHitResult(true, hit);
//            }
//        }
//    }

//    return result;
//}


//private void UpdateHitVisualizer(GameObject visualizer, NearestHitResult result, bool isModifying)
//{
//    if(result.isHitted && result.hitResult.distance >= minVisualDistance && !isModifying)
//    {
//        if(visualizer.activeSelf)
//        {
//            visualizer.transform.position = Vector3.Lerp(visualizer.transform.position, result.hitResult.point, 0.05f);
//            visualizer.transform.forward = Vector3.Lerp(visualizer.transform.forward, result.hitResult.normal, 0.05f);
//        } else
//        {
//            visualizer.transform.position = result.hitResult.point;
//            visualizer.transform.forward = result.hitResult.normal;
//            visualizer.SetActive(true);
//        }
//    } else
//    {
//        visualizer.SetActive(false);
//    }
//}
