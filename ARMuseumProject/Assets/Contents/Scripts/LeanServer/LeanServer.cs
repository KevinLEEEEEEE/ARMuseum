using UnityEngine;
using LeanCloud;
using LeanCloud.Storage;
using Cysharp.Threading.Tasks;
using System;
using NRKernal;

public class LeanServer : MonoBehaviour
{
    [SerializeField] private string APP_ID;
    [SerializeField] private string APP_KEY;
    [SerializeField] private string Server;
    [SerializeField] private bool debugMode;
    [SerializeField] private bool saveUserVoxelData;

    void Start()
    {
        LCApplication.Initialize(APP_ID, APP_KEY, Server);

        if (debugMode) EnableDebugger();
    }

    public void EnableDebugger()
    {
        LCLogger.LogDelegate += (level, info) =>
        {
            switch (level)
            {
                case LCLogLevel.Debug:
                    Debug.Log($"[DEBUG] {DateTime.Now} {info}\n");
                    break;
                case LCLogLevel.Warn:
                    Debug.LogWarning($"[WARNING] {DateTime.Now} {info}\n");
                    break;
                case LCLogLevel.Error:
                    Debug.LogError($"[ERROR] {DateTime.Now} {info}\n");
                    break;
                default:
                    Debug.Log(info);
                    break;
            }
        };
    }

    public async void SaveVoxel(string key, byte[] bytes)
    {
        if (!saveUserVoxelData) return;

        await UniTask.NextFrame();

        NRDebugger.Info("[LeanServer] Start saving voxel data.");

        LCObject userModel = new("UserModelTest2");
        userModel["ID"] = key;
        userModel["Model"] = Convert.ToBase64String(bytes);

        await userModel.Save();

        NRDebugger.Info("[LeanServer] Voxel data has been saved.");
    }
}
