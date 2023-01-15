using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class ExhibitsPanel : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameController gameController;
    public NRPointerRaycaster raycaster;
    public GrabbablePanel grabbablePanel;
    public Frame frame;
    public HandCoach_Point handCoach_Point;
    public GameObject exhibits;
    public GameObject status_TargetLost;
    public AudioClip showExhibits;
    public AudioClip hoverExhibits;
    public AudioClip selectExhibits;
    private AudioSource audioPlayer;
    private bool isHandCoached_Point = false; // 不需要重置
    private bool isNavigating;
    private bool canDetectRaycast;
    private bool freezeDuringPointerDown;
    private bool isInitializing
    {
        get
        {
            return initializingCount > 0;
        }
    }
    private int initializingCount = 0;
    private GameObject hoverExhibit;
    private GameObject hoverExhibitOnPointerDown;
    private Vector3[] exhibitsLocalPosition; // 不需要重置

    void Start()
    {
        gameController.FoundObserverEvent += FoundObserver;
        gameController.LostObserverEvent += LostObserver;
        gameController.StartRaycastEvent += StartRaycastDetection;
        gameController.StopRaycastEvent += StopRayastDetection;
        gameController.BeginTourEvent += StartNavigating;
        gameController.EndTourEvent += StopNavigating;
        audioPlayer = transform.GetComponent<AudioSource>();

        Initialize();
        SaveOriginalPosition();
        StopNavigating();
    }

    private void Initialize()
    {
        isNavigating = false;
        canDetectRaycast = true;
        freezeDuringPointerDown = false;
        initializingCount = 0;
        hoverExhibit = null;
        hoverExhibitOnPointerDown = null;
    }

    private void SaveOriginalPosition()
    {
        int exhibitsCount = exhibits.transform.childCount;
        exhibitsLocalPosition = new Vector3[exhibitsCount];

        for (int i = 0; i < exhibitsCount; i++)
        {
            exhibitsLocalPosition[i] = exhibits.transform.GetChild(i).localPosition;
        }
    }

    public void BeginInitializing()
    {
        initializingCount++;
    }

    public void FinishInitializing()
    {
        initializingCount--;
    }

    public void StartNavigating()
    {
        exhibits.SetActive(true);
        frame.ActiveFrames();
        isNavigating = true;
        PlaySound(showExhibits);
    }

    public void StopNavigating()
    {
        exhibits.SetActive(false);
        frame.InactiveFrames();
        audioPlayer.Stop();
        ResetAll();
    }

    public void StartRaycastDetection()
    {
        canDetectRaycast = true;
    }

    public void StopRayastDetection()
    {
        canDetectRaycast = false;

        if(hoverExhibitOnPointerDown)
        {
            freezeDuringPointerDown = true;
        }

        ChangeAllExhibitsToDefaultState();
    }

    private void FoundObserver()
    {
        status_TargetLost.SetActive(false);
    }

    private void LostObserver()
    {
        status_TargetLost.SetActive(true);
    }

    private void PlaySound(AudioClip clip)
    {
        audioPlayer.clip = clip;
        audioPlayer.Play();
    }

    // 方法被GrabbableExhibits调用
    public void ActiveExhibit(string name)
    {
        exhibits.transform.Find(name).gameObject.SetActive(true);
    }

    public void ChangeAllExhibitsToDefaultState()
    {
        foreach (Transform child in exhibits.transform)
        {
            child.GetComponent<Exhibits>().ChangeToDefaultState();
        }
    }

    private void ResetHoverExhibit()
    {
        if (hoverExhibit != null)
        {
            hoverExhibit.GetComponent<Exhibits>().ChangeToDefaultState();
            hoverExhibit = null;
        }
    }

    public void ResetAll()
    {
        foreach(Transform child in exhibits.transform)
        {
            child.gameObject.SetActive(true);
            child.GetComponent<Exhibits>().ResetAll();
        }

        Initialize();
    }

    private GameObject GetNearestObject(RaycastResult raycastResult)
    {
        Vector3 raycastHitPosition = transform.InverseTransformPoint(raycastResult.worldPosition);
        float nearsetDistance = float.MaxValue;
        GameObject nearestObject = null;

        for (int i = 0; i < exhibitsLocalPosition.Length; i++)
        {
            GameObject obj = exhibits.transform.GetChild(i).gameObject;

            // 当物体ActiveSelf为False时，其处于Grab激活状态，不纳入计算
            if (obj.activeSelf)
            {
                float distance = Vector3.Distance(raycastHitPosition, exhibitsLocalPosition[i]);

                if(obj != hoverExhibit)
                {
                    distance += 0.03f;
                }

                if (distance < nearsetDistance)
                {
                    nearestObject = obj;
                    nearsetDistance = distance;
                }
            }
        }
        return nearestObject;
    }

    void Update()
    {
        if(!isNavigating || !canDetectRaycast || isInitializing)
        {
            ResetHoverExhibit();
            return;
        }

        RaycastResult firstRaycastResult = raycaster.FirstRaycastResult(); // 当前仅支持右手的选择模式

        if (!firstRaycastResult.isValid || firstRaycastResult.gameObject.name != transform.name)
        {
            ResetHoverExhibit();
            return;
        }

        GameObject nearestObject = GetNearestObject(firstRaycastResult);

        if (hoverExhibit != nearestObject)
        {
            ResetHoverExhibit();

            if (nearestObject != null)
            {
                hoverExhibit = nearestObject;
                hoverExhibit.GetComponent<Exhibits>().ChangeToHoverState();
                PlaySound(hoverExhibits);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(hoverExhibit != null)
        {
            if(freezeDuringPointerDown)
            {
                freezeDuringPointerDown = false;

                if(hoverExhibit == hoverExhibitOnPointerDown)
                {
                    return;
                }
            }

            if (!isHandCoached_Point)
            {
                Debug.Log("active");
                isHandCoached_Point = true;
                handCoach_Point.StartHintLoop();
            }

            Debug.Log("[Player] Select exhibit: " + hoverExhibit.name);
            grabbablePanel.ActiveGrabbableItem(hoverExhibit);
            hoverExhibit.SetActive(false);
            PlaySound(selectExhibits);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        hoverExhibitOnPointerDown = hoverExhibit;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        frame.HightlightFrames();

        if(canDetectRaycast)
        {
            hoverExhibitOnPointerDown = null;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        frame.RestoreFrames();
        ChangeAllExhibitsToDefaultState();

        if (canDetectRaycast)
        {
            hoverExhibitOnPointerDown = null;
        }
    }
}
