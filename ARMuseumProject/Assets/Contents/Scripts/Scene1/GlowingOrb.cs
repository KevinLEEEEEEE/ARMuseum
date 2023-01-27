using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GlowingOrb : MonoBehaviour
{
    public AnimationCurve speedCurve;
    public AnimationCurve startSizeCurve;
    public GameController_S2 gameController;
    public ParticleSystem curveEffect;

    private Rigidbody orbRigidbody;
    private Vector3 planeAnchor;
    public enum OrbTarget
    {
        centerCamera,
        planeAnchor,
    }
    private OrbTarget currentOrbTarget = OrbTarget.centerCamera;

    private void Start()
    {
        orbRigidbody = transform.GetComponent<Rigidbody>();
    }

    public void InitOrb(Vector3 position)
    {
        transform.position = position;   
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void SetOrbTarget(OrbTarget target)
    {
        currentOrbTarget = target;
    }

    public void SetPlaneAnchor(EventAnchor anchor)
    {
        planeAnchor = anchor.GetCorrectedHitPoint();
    }

    public void DestoryOrb()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void FadeOut()
    {
        transform.GetComponent<Animation>().Play();
        transform.GetComponent<AudioSource>().Play();
        curveEffect.Play();
    }

    private Vector3 GetTargetLocation() // 暂时使用该位置，后期需要设定更加自然、灵动的跟随策略
    {
        if (currentOrbTarget == OrbTarget.centerCamera)
        {
            Transform anchor = NRSessionManager.Instance.CenterCameraAnchor;
            return anchor.position + anchor.transform.forward * 0.5f + Vector3.down * 0.02f;
        }
        else
        {
            return planeAnchor + Vector3.up * 0.12f; // 目标位置
        }
    }

    private void UpdateOrbForce()
    {
        Vector3 start = transform.position;
        Vector3 end = GetTargetLocation();
        float ratio = speedCurve.Evaluate(Vector3.Distance(start, end));

        if (currentOrbTarget == OrbTarget.planeAnchor)
        {
            ratio /= 1.5f;
        }

        orbRigidbody.AddForce((end - start) * ratio);
    }

    void Update()
    {
        UpdateOrbForce();
    }
}
