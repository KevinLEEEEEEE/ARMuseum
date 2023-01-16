using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using TMPro;

public class Act1 : MonoBehaviour
{
    //public TrackableObserver observer;
    public TextMeshProUGUI text;
    public Transform GlowingOrb; 
    public GameObject detectionHitVisualizer;
    public GameObject detectionLine;
    private LineRenderer detectLineRenderer;
    private HandEnum domainHand = HandEnum.RightHand;
    private float detectionRange = 0.15f;
    private float activationRange = 0.04f;
    private enum state
    {
        Suspend,
        Default,
        Detected,
        Active,
    }
    private state storyPlaneState = state.Suspend;

    void Start()
    {
        //observer.FoundEvent += Found;
        //observer.LostEvent += Lost;

        detectLineRenderer = detectionLine.GetComponent<LineRenderer>();
        PlaneDetectionStart();


        //Instantiate(prefabs[index], hit.point, Quaternion.identity);
    }

    //private void Found(Vector3 pos, Quaternion qua)
    //{
    //    //Debug.Log("[Player] Plane Found");
    //}

    //private void Lost()
    //{
    //    //Debug.Log("[Player] Plane Lost");
    //}

    public void PlaneDetectionStart()
    {
        storyPlaneState = state.Default;
    }

    public void PlaneDetectionStop()
    {
        storyPlaneState = state.Suspend;
    }

    private void UpdateDetectionVisualizer(Vector3 start, Vector3 end)
    {
        detectLineRenderer.SetPosition(0, start);
        detectLineRenderer.SetPosition(1, end);
        detectionHitVisualizer.transform.position = end;
    }

    private void ShowDetectionVisualizer()
    {
        detectionLine.SetActive(true);
        detectionHitVisualizer.SetActive(true);
    }

    private void HideDetectionVisualizer()
    {
        detectionLine.SetActive(false);
        detectionHitVisualizer.SetActive(false);
    }


    void Update()
    {
        HandState domainHandState = NRInput.Hands.GetHandState(domainHand);

        if (storyPlaneState == state.Suspend)
        {
            HideDetectionVisualizer();
            return;
        }

        Vector3 laserAnchor = domainHandState.GetJointPose(HandJointID.IndexTip).position;

        if (domainHandState.currentGesture != HandGesture.Point)
        {
            GlowingOrb.transform.position = laserAnchor + Vector3.up * 0.05f;
        }

        Debug.DrawRay(laserAnchor, Vector3.down, Color.blue);

        int layerMask = 1 << 12;

        if (Physics.Raycast(new Ray(laserAnchor, Vector3.down), out var hitResult, 2, ~layerMask))
        {
            GameObject hit = hitResult.collider.gameObject;
            
            if (hit != null && hit.GetComponent<NRTrackableBehaviour>() != null)
            {
                if (hitResult.distance <= detectionRange && hitResult.distance > activationRange)
                {
                    if (storyPlaneState != state.Detected)
                    {
                        Debug.Log("[Player] Enter detection range");
                        ShowDetectionVisualizer();
                        text.text = "detect range";
                        storyPlaneState = state.Detected;
                    }
                    UpdateDetectionVisualizer(laserAnchor, hitResult.point);

                }
                else if (hitResult.distance <= activationRange)
                {
                    if (storyPlaneState != state.Active)
                    {
                        Debug.Log("[Player] Enter activation Range");
                        ShowDetectionVisualizer();
                        text.text = "active range";
                        storyPlaneState = state.Active;
                    }
                    UpdateDetectionVisualizer(laserAnchor, hitResult.point);
                }
                else
                {
                    HideDetectionVisualizer();
                    storyPlaneState = state.Default;
                }

            }
            else
            {
                HideDetectionVisualizer();
            }
        } else
        {
            HideDetectionVisualizer();
        }
    }
}
