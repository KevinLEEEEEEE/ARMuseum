using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal.NRExamples;
using System;

public enum Env
{
    InEditor,
    OutsideEditor
}

public class HandModelManager : MonoBehaviour
{
    [Serializable]
    public class HandModelsGroup
    {
        public GameObject rightHandModel;
        public GameObject leftHandModel;
        public Env activeEnv;

        public void SetEnv(Env currentEnv)
        {
            SetActive(activeEnv == currentEnv);
        }

        public void SetActive(bool isActive)
        {
            if (leftHandModel)
            {
                leftHandModel.SetActive(isActive);
            }
            if (rightHandModel)
            {
                rightHandModel.SetActive(isActive);
            }
        }
    }

    public HandModelsGroup[] modelsGroups;

    void Start()
    {
        UpdateHandModel(Application.isEditor ? Env.InEditor : Env.OutsideEditor);
    }

    private void UpdateHandModel(Env env)
    {
        for (int i = 0; i < modelsGroups.Length; i++)
        {
            var group = modelsGroups[i];
            if (group == null)
                continue;
            group.SetEnv(env);
        }
    }
}
