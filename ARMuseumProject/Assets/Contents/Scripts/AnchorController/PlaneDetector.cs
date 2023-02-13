using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class PlaneDetector : MonoBehaviour
{
    public GameObject DetectedPlanePrefab;
    private List<NRTrackablePlane> m_NewPlanes = new List<NRTrackablePlane>();
    private bool canAddNewPlane = true;

    public void LockTargetPlane(GameObject targetPlane)
    {
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
        canAddNewPlane = true;

        foreach (Transform plane in transform)
        {
            plane.gameObject.SetActive(true);
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
            planeObject.GetComponent<NRTrackableBehaviour>().Initialize(m_NewPlanes[i]);
        }
    }
}
