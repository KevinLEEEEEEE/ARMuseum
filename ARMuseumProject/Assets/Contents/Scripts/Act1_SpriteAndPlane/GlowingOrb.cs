using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class GlowingOrb : MonoBehaviour
{
    public GameObject orbPrefab;
    public AnimationCurve speedCurve;

    private GameObject orb;
    private Rigidbody orbRigidbody;
    private bool canFollow;

    // Start is called before the first frame update
    void Start()
    {
        ResetAll();
        InvokeRepeating("AddRandomForce", 2f, 0.2f);
    }

    private void OnDisable()
    {
        CancelInvoke("AddRandomForce");
    }

    public void ResetAll()
    {
        canFollow = false;
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
            rigid.mass = 3;
            rigid.drag = 2f;
        }      
    }

    public void StartFollow()
    {
        canFollow = true;
    }

    public void StopFollow()
    {
        canFollow = false;
    }

    private Vector3 GetFollowingPoint() // 暂时使用该位置，后期需要设定更加自然、灵动的跟随策略
    {
        HandState domainHandState = NRInput.Hands.GetHandState(HandEnum.RightHand);

        if (domainHandState.currentGesture != HandGesture.Point)
        {
            Transform anchor = NRSessionManager.Instance.CenterCameraAnchor;
            return anchor.position + anchor.transform.forward * 0.6f;
        }else
        {
            return domainHandState.GetJointPose(HandJointID.IndexTip).position + Vector3.up * 0.05f;
        } 
    }

    private void AddRandomForce()
    {
        if (!orbRigidbody) return;

        orbRigidbody.AddForce(Random.onUnitSphere / 3);
    }

    void Update()
    {
        if(orb && canFollow)
        {
            Vector3 start = orb.transform.position;
            Vector3 end = GetFollowingPoint();
            float ratio = speedCurve.Evaluate(Vector3.Distance(start, end));
            orbRigidbody.AddForce((end - start) * ratio);
        }
    }
}
