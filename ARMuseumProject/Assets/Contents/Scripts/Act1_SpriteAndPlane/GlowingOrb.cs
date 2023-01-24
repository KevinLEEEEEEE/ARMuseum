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
    public GameObject orbEffect;
    public GameObject groundEffect;

    public GameObject orb;
    //private GameObject orbBody;
    private Rigidbody orbRigidbody;
    private ParticleSystem orbParticle;
    private Animation orbAnimation;
    private Vector3 planeAnchor;
    private bool isPlaneAnchorConfirmed = false;
    private bool isReadyActive = false;
    private int planeMask = 1 << 8;
    private float maxRayDistance = 0.24f;
    private float orbFloatDistance = 0.04f;
    public enum OrbTarget
    {
        centerCamera,
        handJoint,
        planeAnchor,
        spaceAnchor,
    }
    private OrbTarget currentOrbTarget = OrbTarget.centerCamera;

    private void Start()
    {
        orbRigidbody = orb.GetComponent<Rigidbody>();
        orbParticle = orb.transform.GetChild(0).GetComponent<ParticleSystem>();
        orbAnimation = orb.transform.GetComponent<Animation>();
    }

    public void InitOrb(Vector3 position)
    {
        orb.transform.position = position;
        orb.SetActive(true);   
    }

    public void SetOrbTarget(OrbTarget target)
    {
        currentOrbTarget = target;

        if(target != OrbTarget.planeAnchor)
        {
            isPlaneAnchorConfirmed = false;
        }
    }

    public void SetPlaneAnchor(Vector3 anchor, bool isConfirmed)
    {
        if(isConfirmed)
        {
            isPlaneAnchorConfirmed = true;
            groundEffect.transform.position = anchor;
        }

        if(!isPlaneAnchorConfirmed)
        {
            planeAnchor = anchor;
        }
    }

    public void DestoryOrb()
    {
        orb.SetActive(false);
        orb = null;
        orbRigidbody = null;
        orbParticle = null; 
    }

    public void ActiveOrb()
    {
        var main = orbParticle.main;
        main.startSize = 0.8f;
    }

    public void FadeOut()
    {
        StartCoroutine("OrbFadeOut");
    }

    private IEnumerator OrbFadeOut()
    {
        orbEffect.SetActive(true);
        transform.GetComponent<Animation>().Play();

        yield return new WaitForSeconds(5f);

        groundEffect.SetActive(true);

        yield return new WaitForSeconds(2f);
    }

    private Vector3 GetTargetLocation() // 暂时使用该位置，后期需要设定更加自然、灵动的跟随策略
    {
        HandState domainHandState = gameController.GetDomainHandState();

        if (currentOrbTarget == OrbTarget.centerCamera || !domainHandState.isTracked)
        {
            Transform anchor = NRSessionManager.Instance.CenterCameraAnchor;
            return anchor.position + anchor.transform.forward * 0.6f;
        }
        else if (currentOrbTarget == OrbTarget.handJoint)
        {
            Pose palmPose = domainHandState.GetJointPose(HandJointID.IndexTip);
            return palmPose.position + Vector3.up * orbFloatDistance;
        }
        else
        {
            return planeAnchor + Vector3.up * 0.05f; // 目标位置
        }
    }

    private void UpdateOrbForce()
    {
        Vector3 start = orb.transform.position;
        Vector3 end = GetTargetLocation();
        float ratio = speedCurve.Evaluate(Vector3.Distance(start, end));
        orbRigidbody.AddForce((end - start) * ratio);
    }

    private void UpdateOrbParticle()
    {
        HandState domainHandState = gameController.GetDomainHandState();
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

        if(currentOrbTarget == OrbTarget.centerCamera || currentOrbTarget == OrbTarget.handJoint)
        {
            UpdateOrbForce();
        }

        if(currentOrbTarget == OrbTarget.planeAnchor)
        {
            orb.transform.position = Vector3.MoveTowards(orb.transform.position, GetTargetLocation(), 0.03f * Time.deltaTime);
        }

        if(currentOrbTarget == OrbTarget.handJoint)
        {
            UpdateOrbParticle();
        } 
    }
}
