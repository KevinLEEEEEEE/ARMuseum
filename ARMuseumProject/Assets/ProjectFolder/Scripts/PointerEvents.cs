using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class PointerEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject LaserRaycaster;
    public GameObject DisplayItemsLayer;
    public GameObject GrabbableItemsLayer;
    public Material[] RealisticMaterials;
    public Material[] DefaultMaterials;

    private NRPointerRaycaster _Raycaster;
    private GrabbableController _GrabbableItemsController;
    private bool isPointerEnter = false;
    private GameObject NearestObject = null;
    private Vector3[] ObjectPositionArray;
    private int ObjectCount = 0;

    void Start()
    {
        _Raycaster = LaserRaycaster.GetComponent<NRPointerRaycaster>();
        _GrabbableItemsController = GrabbableItemsLayer.GetComponent<GrabbableController>();

        ObjectCount = DisplayItemsLayer.transform.childCount;
        ObjectPositionArray = new Vector3[ObjectCount];

        UpdateObjectPosition();
    }

    private void UpdateObjectPosition()
    {
        for(int i = 0; i < ObjectCount; i++)
        {
            GameObject child = DisplayItemsLayer.transform.GetChild(i).gameObject;

            ObjectPositionArray[i] = transform.InverseTransformPoint(child.transform.position);
        }
    }

    void Update()
    {
        if(isPointerEnter == true && ObjectPositionArray != null && ObjectPositionArray.Length > 0)
        {
            UpdateNearestObject();
        }
    }

    private void UpdateNearestObject()
    {
        GameObject LatestNearestObject = FindNearestObject();

        if (NearestObject != LatestNearestObject)
        {
            if (NearestObject != null) {
                RestoreNearestObject();
            }

            NearestObject = LatestNearestObject; // 实时计算“最近物体”，并更新样式
            ActiveNearestObject();
        }
    }

    private void RestoreNearestObject()
    {
        int index = NearestObject.transform.GetSiblingIndex();

        NearestObject.GetComponent<Renderer>().material = DefaultMaterials[index];
    }

    private void ActiveNearestObject()
    {
        if(NearestObject == null)
        {
            return;
        }

        int index = NearestObject.transform.GetSiblingIndex();

        NearestObject.GetComponent<Renderer>().material = RealisticMaterials[index];

        //Debug.Log(NearestObject.GetComponent<Renderer>().bounds.size);

        //Vector3 currPosition = NearestObject.transform.position;

        ////NearestObject.transform.position = Vector3.MoveTowards(currPosition, new Vector3(currPosition.x, currPosition.y + 0.1f, currPosition.z), 10);
    }

    public void ResetAll()
    {
        // 恢复所有物体的材质

        int DisplayItemsCount = DisplayItemsLayer.transform.childCount;

        for (int i = 0; i < DisplayItemsCount; i++)
        {
            DisplayItemsLayer.transform.GetChild(i).GetComponent<Renderer>().material = DefaultMaterials[i];
        }

        NearestObject = null;

        // 恢复所有物体的位置
    }

    private GameObject FindNearestObject()
    {
        RaycastResult firstHit = _Raycaster.FirstRaycastResult();
        Vector3 hitPointPosition = transform.InverseTransformPoint(firstHit.worldPosition);
        float NearsetDistance = float.MaxValue;
        GameObject NearestObject = null;

        for (int i = 0; i < ObjectCount; i++)
        {
            if(DisplayItemsLayer.transform.GetChild(i).gameObject.activeSelf == false)
            {
                continue;
            }

            float distance = Vector3.Distance(hitPointPosition, ObjectPositionArray[i]);

            if (NearsetDistance > distance)
            {
                // 如果当前物体距离hitpoint更近，则当前物体为新的NearestObject
                NearestObject = DisplayItemsLayer.transform.GetChild(i).gameObject;
                NearsetDistance = distance;
            }
        }

        return NearestObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(NearestObject == null)
        {
            Debug.Log("Pointer click, no object left");

            return;
        }

        Debug.Log("Pointer click, the nearest object is: " + NearestObject.name);

        // notify

        _GrabbableItemsController.ActiveGrabbableItem(NearestObject);

        NearestObject.SetActive(false);

        UpdateObjectPosition();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer enter.");

        isPointerEnter = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exit.");

        isPointerEnter = false;

        ResetAll();
    }
}
