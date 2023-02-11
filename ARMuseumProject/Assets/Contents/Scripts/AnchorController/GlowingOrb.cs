using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GlowingOrb : MonoBehaviour
{
    public GameObject orbBody;
    public GameObject orbTrail;
    public ParticleSystem curveEffect;
    public AudioClip audioClip_orbActive;
    public AnimationCurve speedCurve;

    private AudioGenerator audioSource_orbActive;
    private Rigidbody orbRigidbody;
    private Animation orbAnimation;
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
        orbAnimation = transform.GetComponent<Animation>();
        audioSource_orbActive = new AudioGenerator(gameObject, audioClip_orbActive);
        HideBody();
        HideTrail();
    }

    public void ShowBody()
    {
        orbBody.SetActive(true);
    }

    public void HideBody()
    {
        orbBody.SetActive(false);
    }

    public void ShowTrail()
    {
        orbTrail.SetActive(true);
    }

    public void HideTrail()
    {
        orbTrail.SetActive(false);
    }

    public void SetOrbTarget(OrbTarget target)
    {
        currentOrbTarget = target;
    }

    public void SetEventAnchor(EventAnchor anchor)
    {
        planeAnchor = anchor.GetCorrectedHitPoint();
    }

    public void FadeOut()
    {
        audioSource_orbActive.Play();
        orbAnimation.Play();
        curveEffect.Play();
    }

    private Vector3 GetTargetLocation() // ��ʱʹ�ø�λ�ã�������Ҫ�趨������Ȼ���鶯�ĸ������
    {
        if (currentOrbTarget == OrbTarget.centerCamera)
        {
            Transform anchor = NRSessionManager.Instance.CenterCameraAnchor;
            return anchor.position + anchor.transform.forward * 0.5f + Vector3.down * 0.02f;
        }
        else
        {
            return planeAnchor + Vector3.up * 0.12f; // Ŀ��λ��
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
