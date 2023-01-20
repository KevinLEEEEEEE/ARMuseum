using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GlowingOrb : MonoBehaviour
{
    public GameObject orbPrefab;
    public GameObject hitVisualizer;
    public AnimationCurve speedCurve;
    public AnimationCurve scaleCurve;

    private GameObject orb;
    private Rigidbody orbRigidbody;
    private int planeMask = 1 << 8;
    private float maxRayDistance = 1;
    private float orbFloatDistance = 0.04f;

    void Start()
    {
        ResetAll();
        //InvokeRepeating("AddRandomForce", 2f, 0.2f);
    }

    private void OnDisable()
    {
        //CancelInvoke("AddRandomForce");
    }

    public void ResetAll()
    {
        HideHitVisualizer();
        DestoryOrb();
    }

    public void ShowOrb()
    {
        if (orb) orb.SetActive(true);
    }

    public void HideOrb()
    {
        if (orb) orb.SetActive(true);
    }

    public void DestoryOrb()
    {
        if (orb) Destroy(orb);
        orb = null;
        orbRigidbody = null;
    }

    public void InitOrb(Vector3 position, Quaternion rotation, float scale)
    {
        if (!orb)
        {
            orb = Instantiate(orbPrefab, position, rotation);
            orb.SetActive(false);
            orb.transform.parent = transform;
            orb.transform.localScale = new Vector3(scale, scale, scale);

            Rigidbody rigid = orb.AddComponent<Rigidbody>();
            orbRigidbody = rigid;
            rigid.useGravity = false;
            rigid.mass = 2;
            rigid.drag = 3f;
        }      
    }

    private Vector3 GetTargetLocation(HandState domainHandState) // 暂时使用该位置，后期需要设定更加自然、灵动的跟随策略
    {
        if (!domainHandState.isTracked)
        {
            Transform anchor = NRSessionManager.Instance.CenterCameraAnchor;
            return anchor.position + anchor.transform.forward * 0.6f;
        }else
        {
            Pose palmPose = domainHandState.GetJointPose(HandJointID.Palm);
            return palmPose.position + Vector3.up * orbFloatDistance;
        } 
    }

    private void UpdateHitVisualizer(Vector3 hitPoint, float distance)
    {
        if (!hitVisualizer.activeSelf)
        {
            hitVisualizer.gameObject.SetActive(true);
        }

        float scale = scaleCurve.Evaluate(distance / maxRayDistance);

        hitVisualizer.transform.position = hitPoint;
        hitVisualizer.transform.localScale = new Vector3(scale, 1, scale);
    }

    private void HideHitVisualizer()
    {
        if (hitVisualizer.activeSelf)
        {
            hitVisualizer.gameObject.SetActive(false);
            hitVisualizer.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    //private void AddRandomForce()
    //{
    //    if (!orbRigidbody) return;

    //    orbRigidbody.AddForce(Random.onUnitSphere / 3);
    //}

    void Update()
    {
        if (!orb || !orb.activeSelf) return;

        HandState domainHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);
        Vector3 start = orb.transform.position;
        Vector3 end = GetTargetLocation(domainHandState);
        float ratio = speedCurve.Evaluate(Vector3.Distance(start, end));
        orbRigidbody.AddForce((end - start) * ratio);

        if(domainHandState.isTracked)
        {
            Vector3 raycastStart = start - Vector3.down * orbFloatDistance;

            if (Physics.Raycast(new Ray(raycastStart, Vector3.down), out var hitResult, maxRayDistance, planeMask))
            {
                UpdateHitVisualizer(hitResult.point, hitResult.distance);
            } else
            {
                HideHitVisualizer();
            }
        } else
        {
            HideHitVisualizer();
        }
        
    }
}
