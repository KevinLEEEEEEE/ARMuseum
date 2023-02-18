using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;

public class LogSetting : MonoBehaviour
{
    public LogLevel logLevel;

    // Start is called before the first frame update
    void Start()
    {
        NRDebugger.logLevel = logLevel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
