using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using NRKernal.NRExamples;

public class PlaneDetector : MonoBehaviour
{
    [SerializeField] private GameObject DetectedPlanePrefab;
    [SerializeField] private float verticalOffset = 0;
    private List<NRTrackablePlane> m_NewPlanes = new();
    private bool canAddNewPlane = true;

    public void LockTargetPlane(GameObject targetPlane)
    {
        NRDebugger.Info("[PlaneDetector] Target plane locked.");

        canAddNewPlane = false;

        foreach(Transform plane in transform)
        {
            if(!ReferenceEquals(plane.gameObject, targetPlane))
            {
                plane.gameObject.SetActive(false);
            }
        }
    }

    public void ReleaseTargetPlane()
    {
        NRDebugger.Info("[PlaneDetector] Target plane released.");

        canAddNewPlane = true;

        foreach (Transform plane in transform)
        {
            plane.gameObject.SetActive(true);
        }
    }

    public void StartPlaneHint()
    {
        foreach (Transform plane in transform)
        {
            plane.GetComponent<Animator>().SetTrigger("Breath");
        }
    }

    public void StopPlaneHint()
    {
        foreach (Transform plane in transform)
        {
            plane.GetComponent<Animator>().SetTrigger("FadeOut");
        }
    }

    void Update()
    {
        if (!canAddNewPlane) return;

        NRFrame.GetTrackables<NRTrackablePlane>(m_NewPlanes, NRTrackableQueryFilter.New);
        for (int i = 0; i < m_NewPlanes.Count; i++)
        {
            // Instantiate a plane visualization prefab and set it to track the new plane. The transform is set to
            // the origin with an identity rotation since the mesh for our prefab is updated in Unity World coordinates.
            GameObject planeObject = Instantiate(DetectedPlanePrefab, Vector3.zero, Quaternion.identity, transform);
            planeObject.GetComponent<PolygonPlaneVisualizer>().SetOffset(verticalOffset);
            planeObject.GetComponent<NRTrackableBehaviour>().Initialize(m_NewPlanes[i]);
        }
    }
}
