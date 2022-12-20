using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class CornerObjController : MonoBehaviour
{
    public GameObject LeftTopCorner;
    public GameObject LeftBottomCorner;
    public GameObject RightTopCorner;
    public GameObject RightBottomCorner;

    private List<NRTrackableImage> m_TempTrackingImages = new List<NRTrackableImage>();

    private bool isNavigating;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        StartNavigating();
#endif
    }

    public void StartNavigating()
    {
        isNavigating = true;

        LeftTopCorner.SetActive(true);
        LeftBottomCorner.SetActive(true);
        RightTopCorner.SetActive(true);
        RightBottomCorner.SetActive(true);
    }

    public void StopNavigating()
    {
        isNavigating = false;

        LeftTopCorner.SetActive(false);
        LeftBottomCorner.SetActive(false);
        RightTopCorner.SetActive(false);
        RightBottomCorner.SetActive(false);
    }

    //private void CornerReposition()
    //{
    //    // Get updated augmented images for this frame.
    //    NRFrame.GetTrackables<NRTrackableImage>(m_TempTrackingImages, NRTrackableQueryFilter.All);

    //    // Create visualizers and anchors for updated augmented images that are tracking and do not previously
    //    // have a visualizer. Remove visualizers for stopped images.
    //    foreach (var image in m_TempTrackingImages)
    //    {
    //        if (image.GetTrackingState() != TrackingState.Stopped)
    //        {
    //            // Create an anchor to ensure that NRSDK keeps tracking this augmented image.
    //            float halfWidth = image.ExtentX / 2;
    //            float halfHeight = image.ExtentZ / 2;

    //            LeftTopCorner.transform.localPosition = (halfWidth / 100 * Vector3.left) + (halfWidth / 100 * Vector3.back);
    //            //LeftTopCorner.transform.position = (halfWidth * Vector3.left) + (halfHeight * Vector3.back);
    //            //LeftBottomCorner.transform.position = (halfWidth * Vector3.right) + (halfHeight * Vector3.back);
    //            //RightTopCorner.transform.localPosition = (halfWidth * Vector3.left) + (halfHeight * Vector3.forward);
    //            //RightBottomCorner.transform.localPosition = (halfWidth * Vector3.right) + (halfHeight * Vector3.forward);
    //        }
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        //if (isNavigating == true)
        //{
        //    CornerReposition();
        //}
    }
}
