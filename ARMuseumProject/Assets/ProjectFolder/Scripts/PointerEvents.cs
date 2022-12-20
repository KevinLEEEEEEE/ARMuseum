using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class PointerEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject LaserRaycaster;
    private NRPointerRaycaster _Raycaster;
    //public GameObject Laser;
    //private LineRenderer _LaserRenderer;
    public GameObject DisplayItemsLayer;

    void Start()
    {
        _Raycaster = LaserRaycaster.GetComponent<NRPointerRaycaster>();
        //_LaserRenderer = Laser.GetComponent<LineRenderer>();
    }

    void Update()
    {

    }

    private GameObject FindNearestObject()
    {
        RaycastResult firstHit = _Raycaster.FirstRaycastResult();
        Vector3 hitPointPosition = transform.InverseTransformPoint(firstHit.worldPosition);
        float NearsetDistance = float.MaxValue;
        GameObject NearestObject = null;

        foreach (Transform child in DisplayItemsLayer.transform)
        {
            Vector3 displayObjectPosition = transform.InverseTransformPoint(child.gameObject.transform.position);
            float distance = Vector3.Distance(hitPointPosition, displayObjectPosition);

            if (NearsetDistance > distance)
            {
                // �����ǰ�������hitpoint��������ǰ����Ϊ�µ�NearestObject
                NearestObject = child.gameObject;
                NearsetDistance = distance;
            }
        }

        return NearestObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject nearestObject = FindNearestObject();

        nearestObject.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("pointer enter: ");

        // Corner��ɫ�仯����������

        // ��ȡָ��λ��

        //Color laserColor = _LaserRenderer.material.color;
        //laserColor.a = 0.1f;

        //_LaserRenderer.material.color = laserColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("pointer exit");

        // Corner��ʽ�ָ�

        //Color laserColor = _LaserRenderer.material.color;
        //laserColor.a = 1f;

        //_LaserRenderer.material.color = laserColor;
    }
}
