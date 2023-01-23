using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GlowingOrb : MonoBehaviour
{
    public GameObject orbPrefab;
    public AnimationCurve speedCurve;
    public AnimationCurve startSizeCurve;
    public GameController_S2 gameController;

    private GameObject orb;
    private Rigidbody orbRigidbody;
    private ParticleSystem orbParticle;
    private int planeMask = 1 << 8;
    private float maxRayDistance = 0.24f;
    private float orbFloatDistance = 0.04f;
    public enum OrbTarget
    {
        centerCamera,
        handJoint,
        planeAnchor,
    }
    private OrbTarget currentOrbTarget = OrbTarget.centerCamera;

    public void InitOrb(Vector3 position)
    {
        if (!orb)
        {
            orb = Instantiate(orbPrefab, position, new Quaternion(0, 0, 0, 0), transform); // 需要优化位置     
            orbRigidbody = orb.GetComponent<Rigidbody>();
            orbParticle = orb.transform.GetChild(0).GetComponent<ParticleSystem>();
        }      
    }

    public void SetOrbTarget(OrbTarget target)
    {
        currentOrbTarget = target;
    }

    public void DestoryOrb()
    {
        if (orb)
        {
            Destroy(orb);
            orb = null;
            orbRigidbody = null;
            orbParticle = null;
        }  
    }

    private Vector3 GetTargetLocation(HandState domainHandState) // 暂时使用该位置，后期需要设定更加自然、灵动的跟随策略
    {
        if (currentOrbTarget == OrbTarget.centerCamera || !domainHandState.isTracked)
        {
            Transform anchor = NRSessionManager.Instance.CenterCameraAnchor;
            return anchor.position + anchor.transform.forward * 0.6f;
        }
        else if (currentOrbTarget == OrbTarget.handJoint)
        {
            Pose palmPose = domainHandState.GetJointPose(HandJointID.MiddleProximal);
            return palmPose.position + Vector3.up * orbFloatDistance;
        }
        else
        {
            return new Vector3(0, 0, 0); // 目标位置
        }
    }

    private void UpdateOrbForce(HandState domainHandState)
    {
        Vector3 start = orb.transform.position;
        Vector3 end = GetTargetLocation(domainHandState);
        float ratio = speedCurve.Evaluate(Vector3.Distance(start, end));
        orbRigidbody.AddForce((end - start) * ratio);
    }

    private void UpdateOrbParticle(HandState domainHandState)
    {
        float targetSize = 0.8f;

        if(domainHandState.isTracked)
        {
            if (Physics.Raycast(new Ray(orb.transform.position, Vector3.down), out var hitResult, maxRayDistance, planeMask))
            {
                targetSize = startSizeCurve.Evaluate(hitResult.distance);
            }
        }

        var main = orbParticle.main;
        main.startSize = targetSize;
    }

    void Update()
    {
        if (!orb) return; // visualizer 效果需要整体更新

        HandState domainHandState = gameController.GetDomainHandState();

        UpdateOrbForce(domainHandState);

        UpdateOrbParticle(domainHandState);
    }
}
