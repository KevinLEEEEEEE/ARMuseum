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
        // 进入时如果已经处于捏的状态，则不能激活Enter，必须要先进入后捏
        isEntering = true && !handState.isPinching;
    }

    private void OnTriggerExit(Collider other)
    {
        isEntering = false;
    }

    void Update()
    {
        // 开始旋转的条件：接触物体、不处于旋转状态且处于捏状态
        if (isEntering)
        {
            if(!isRotatingCondition && handState.isPinching)
            {
                isRotatingCondition = true;
                m_HandRotator.StartRotating(handEnum);
            }
        }

        // 结束旋转条件：满足旋转条件，但是下一刻不满足捏状态
        if(isRotatingCondition && !handState.isPinching)
        {
            isRotatingCondition = false;
            m_HandRotator.StopRotating(handEnum);
        }
    }
}
