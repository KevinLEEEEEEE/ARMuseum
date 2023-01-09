using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class TrackingItemsController : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public NRPointerRaycaster _Raycaster;
    public GrabbableController _GrabbableItemsController;
    public CornerObjController _CornerObjController;
    public GameObject DisplayItemsLayer;
    public GameObject HandCoach_Point;

    public AudioClip ShowExhibits;
    public AudioClip HoverExhibits;
    public AudioClip SelectExhibits;
    private AudioSource AudioPlayer;

    private bool isFirstUse = true;
    private GameObject NearestObjectOnPointerDown = null;
    private bool isNavigating = false;
    private bool isPointerEnter = false;
    private bool isFirstClickAfterRaycastStart = false;
    private bool isNearestObjectStateRecovered = true;
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

    public void StartNavigating()
    {
        DisplayItemsLayer.SetActive(true);
        _CornerObjController.ActiveCornerObjects();
        isNavigating = true;
        PlaySound(ShowExhibits);
    }

    public void StopNavigating()
    {
        DisplayItemsLayer.SetActive(false);
        _CornerObjController.InactiveCornerObjects();
        isNavigating = false;
        AudioPlayer.Stop();
        ResetAll();
    }

    public void StartRaycastDetection()
    {
        canDetectRaycast = true;
        isNearestObjectStateRecovered = false;
    }

    public void StopRayastDetection()
    {
        canDetectRaycast = false;
        isFirstClickAfterRaycastStart = true;
        RestoreAllObjectState();
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
        NearestObject = null;
    }

    public void RestoreAllObjectState()
    {
        foreach (Transform child in DisplayItemsLayer.transform)
        {
            child.GetComponent<DisplayObjectController>().ChangeToDefaultState();
        }
    }

    public void ResetAll()
    {
        foreach(Transform child in DisplayItemsLayer.transform)
        {
            child.gameObject.SetActive(true);
            child.GetComponent<DisplayObjectController>().ResetAll();
        }

        NearestObject = null;
        InitializingCount = 0;
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

                if(currObject != NearestObject)
                {
                    distance += 0.03f;
                }

                if (distance < currNearsetDistance)
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
        if (isPointerEnter && canDetectRaycast && !isInitializing)
        {
            GameObject currNearestObject = FindNearestObject();

            if (isFirstClickAfterRaycastStart && NearestObject == NearestObjectOnPointerDown && !isNearestObjectStateRecovered)
            {
                NearestObject.GetComponent<DisplayObjectController>().ChangeToHoverState();
                PlaySound(HoverExhibits);
                isNearestObjectStateRecovered = true;
            }

            if (NearestObject != currNearestObject)
            {
                if (NearestObject != null)
                {
                    RestoreNearestObject();
                }

                if (currNearestObject != null)
                {
                    NearestObject = currNearestObject;
                    NearestObject.GetComponent<DisplayObjectController>().ChangeToHoverState();
                    PlaySound(HoverExhibits);
                }
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
            if(isFirstClickAfterRaycastStart)
            {
                isFirstClickAfterRaycastStart = false;

                if(NearestObject == NearestObjectOnPointerDown)
                {
                    return;
                }
            }

            if(isFirstUse)
            {
                isFirstUse = false;
                HandCoach_Point.SetActive(true);
            }

            Debug.Log("[Player] Pointer click, the nearest object is: " + NearestObject.name);
            _GrabbableItemsController.ActiveGrabbableItem(NearestObject);
            NearestObject.SetActive(false);
            PlaySound(SelectExhibits);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!isFirstClickAfterRaycastStart)
        {
            NearestObjectOnPointerDown = NearestObject;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isNavigating)
        {
            isPointerEnter = true;
            
        }

        _CornerObjController.HightlightCornerObjects();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _CornerObjController.RestoreCornerObjects();

        if (isNavigating)
        {
            isPointerEnter = false;
            
            RestoreAllObjectState();
        }
    }
}
