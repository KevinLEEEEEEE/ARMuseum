using NRKernal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private HandEnum handEnum;
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
        isEntering = true && !handState.isPinching;
    }

    private void OnTriggerExit(Collider other)
    {
        isEntering = false;
    }

    void Update()
    {
        // ��ʼ��ת���������Ӵ����塢��������ת״̬�Ҵ�����״̬
        if (isEntering)
        {
            if(!isRotatingCondition && handState.isPinching)
            {
                isRotatingCondition = true;
                m_HandRotator.StartRotating(handEnum);
            }
        }

        // ������ת������������ת������������һ�̲�������״̬
        if(isRotatingCondition && !handState.isPinching)
        {
            isRotatingCondition = false;
            m_HandRotator.StopRotating(handEnum);
        }
    }
}
