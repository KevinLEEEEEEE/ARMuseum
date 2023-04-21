using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSetting : MonoBehaviour
{
    [SerializeField] private bool enableLog = false;

    void Start()
    {
        Debug.unityLogger.logEnabled = enableLog;
    }
}
