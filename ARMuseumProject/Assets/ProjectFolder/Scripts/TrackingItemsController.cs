using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class TrackingItemsController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public NRPointerRaycaster _Raycaster;
    public GrabbableController _GrabbableItemsController;
    public CornerObjController _CornerObjController;
    public GameObject DisplayItemsLayer;
    public AudioClip ShowExhibits;
    public AudioClip HoverExhibits;
    public AudioClip SelectExhibits;

    private AudioSource AudioPlayer;
    private bool isNavigating = false;
    private bool isPointerEnter = false;
    private bool canDetectRaycast = true;
    private bool isInitializing
    {
        get
        {
            return InitializingCount > 0;
        }
    }
    private int InitializingCount = 0;
    private GameObject NearestObject = null;
    private Vector3 NearestObjectOriginalPosition;
    private Vector3[] ObjectPositionArray = null;

    void Start()
    {
        int objectCount = DisplayItemsLayer.transform.childCount;
        ObjectPositionArray = new Vector3[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            ObjectPositionArray[i] = DisplayItemsLayer.transform.GetChild(i).localPosition; 
        }

        AudioPlayer = transform.GetComponent<AudioSource>();
        StopNavigating();
    }

    public void BeginInitializing()
    {
        InitializingCount++;
    }

    public void FinishInitializing()
    {
        InitializingCount--;
    }

    public void TrackingImageFound()
    {
        _CornerObjController.ActiveCornerObjects();
    }

    public void TrackingImageLost()
    {
        _CornerObjController.InactiveCornerObjects();
    }

    public void StartNavigating()
    {
        DisplayItemsLayer.SetActive(true);
        isNavigating = true;
        PlaySound(ShowExhibits);
    }

    public void StopNavigating()
    {
        DisplayItemsLayer.SetActive(false);
        isNavigating = false;
        ResetAll();
    }

    public void StartRaycastDetection()
    {
        canDetectRaycast = true;
    }

    public void StopRayastDetection()
    {
        canDetectRaycast = false;
        ResetAll();
    }

    public void RestoreObject(string name)
    {
        Debug.Log("[Player] Grabbable object inactive, restore display object: " + name);

        GameObject restoreObject = DisplayItemsLayer.transform.Find(name).gameObject;

        restoreObject.SetActive(true);
    }

    private void RestoreNearestObject()
    {
        NearestObject.GetComponent<DisplayObjectController>().ChangeToDefaultState();
        NearestObject.transform.localPosition = NearestObjectOriginalPosition;
        NearestObject = null;
    }

    public void ResetAll()
    {
        foreach(Transform child in DisplayItemsLayer.transform)
        {
            child.GetComponent<DisplayObjectController>().ChangeToDefaultState();
        }

        NearestObject = null;
    }

    private GameObject FindNearestObject()
    {
        float currNearsetDistance = float.MaxValue;
        GameObject currNearestObject = null;

        for (int i = 0; i < ObjectPositionArray.Length; i++)
        {
            GameObject currObject = DisplayItemsLayer.transform.GetChild(i).gameObject;

            if (currObject.activeSelf)
            {
                float distance = Vector3.Distance(GetRaycastHitPosition(), ObjectPositionArray[i]);

                if (currNearsetDistance > distance)
                {
                    currNearestObject = currObject;
                    currNearsetDistance = distance;
                }
            }
        }
        return currNearestObject;
    }

    private void PlaySound(AudioClip clip)
    {
        AudioPlayer.clip = clip;
        AudioPlayer.Play();
    }

    void Update()
    {
        if (isPointerEnter && ObjectPositionArray.Length > 0 && canDetectRaycast && !isInitializing)
        {
            GameObject currNearestObject = FindNearestObject();

            if (NearestObject != currNearestObject)
            {
                if (NearestObject != null)
                {
                    RestoreNearestObject();
                }

                if (currNearestObject != null)
                {
                    NearestObject = currNearestObject;
                    NearestObjectOriginalPosition = currNearestObject.transform.localPosition;
                    NearestObject.GetComponent<DisplayObjectController>().ChangeToHoverState();
                    PlaySound(HoverExhibits);
                }
            }

            // Change the position of nearest object for magnetic effect
            if (NearestObject != null)
            {
                NearestObject.transform.localPosition = Vector3.Slerp(NearestObjectOriginalPosition, GetRaycastHitPosition(), 0.1f);
            }
        }
    }

    private Vector3 GetRaycastHitPosition()
    {
        return transform.InverseTransformPoint(_Raycaster.FirstRaycastResult().worldPosition);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isNavigating && canDetectRaycast && !isInitializing && NearestObject != null)
        {
            Debug.Log("[Player] Pointer click, the nearest object is: " + NearestObject.name);
            _GrabbableItemsController.ActiveGrabbableItem(NearestObject);
            NearestObject.SetActive(false);
            PlaySound(SelectExhibits);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isNavigating && canDetectRaycast && !isInitializing)
        {
            isPointerEnter = true;
            _CornerObjController.HightlightCornerObjects();
            NRInput.LaserVisualActive = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (NearestObject != null)
        {
            RestoreNearestObject();
        }

        if (isNavigating && canDetectRaycast && !isInitializing)
        {
            isPointerEnter = false;
            _CornerObjController.RestoreCornerObjects();
            NRInput.LaserVisualActive = true;
            ResetAll();
        }
    }
}
