/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using UnityEngine;

namespace NRKernal.NRExamples
{
    public class MoveWithCamera : MonoBehaviour
    {
        /// <summary> The origin distance. </summary>
        private float originDistance;
        /// <summary> True to use relative. </summary>
        [SerializeField]
        private bool useRelative = true;

        [SerializeField]
        private bool moveSmoothly = false;

        [SerializeField]
        private float step = 5f;

        private Transform m_CenterCamera;
        private Transform CenterCamera
        {
            get
            {
                if (m_CenterCamera == null)
                {
                    if (NRSessionManager.Instance.CenterCameraAnchor != null)
                    {
                        m_CenterCamera = NRSessionManager.Instance.CenterCameraAnchor;
                    }
                    else if (Camera.main != null)
                    {
                        m_CenterCamera = Camera.main.transform;
                    }
                }
                return m_CenterCamera;
            }
        }

        private void Start()
        {
            originDistance = useRelative ? Vector3.Distance(transform.position, CenterCamera == null ? Vector3.zero : CenterCamera.position) : 0;
        }

        public void ResetTransform()
        {
            transform.SetPositionAndRotation(GetTargetPos(), GetTargetRotation());
        }

        private Vector3 GetTargetPos()
        {
            return CenterCamera.transform.position + CenterCamera.transform.forward * originDistance;
        }

        private Quaternion GetTargetRotation()
        {
            return CenterCamera.transform.rotation;
        }

        /// <summary> Late update. </summary>
        void LateUpdate()
        {
            if (CenterCamera != null)
            {
                if (moveSmoothly)
                {
                    transform.SetPositionAndRotation(Vector3.Lerp(transform.position, GetTargetPos(), step), Quaternion.Lerp(transform.rotation, GetTargetRotation(), Time.deltaTime * step));
                } else
                {
                    ResetTransform();
                }
            }
        }
    }
}
