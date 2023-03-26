using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private HandEnum handEnum;
    [SerializeField] private HandGesture rotatingGesture = HandGesture.Grab;
    private HandRotator m_HandRotator;
    private HandState handState;
    private bool isEntering;
    private bool isRotatingCondition;

    void Start()
    {
        m_HandRotator = transform.GetComponentInParent<HandRotator>();
        handState = NRInput.Hands.GetHandState(handEnum);
    }

    private void Reset()
    {
        isEntering = false;
        isRotatingCondition = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ����ʱ����Ѿ��������״̬�����ܼ���Enter������Ҫ�Ƚ������
        isEntering = true && !IsRotatingGesture();
    }

    private void OnTriggerExit(Collider other)
    {
        isEntering = false;
    }

    private bool IsRotatingGesture()
    {
        return handState.currentGesture == rotatingGesture;
    }

    void Update()
    {
        // ��ʼ��ת���������Ӵ����塢��������ת״̬�Ҵ�����״̬
        if (isEntering)
        {
            if(!isRotatingCondition && IsRotatingGesture())
            {
                isRotatingCondition = true;
                m_HandRotator.StartRotating(handEnum);
            }
        }

        // ������ת������������ת������������һ�̲�������״̬
        if(isRotatingCondition && !IsRotatingGesture())
        {
            isRotatingCondition = false;
            m_HandRotator.StopRotating(handEnum);
        }
    }
}
