using AmazingAssets.AdvancedDissolve;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellBlockManager : MonoBehaviour
{
    [SerializeField] private ShellController _shellController;
    [SerializeField] private AdvancedDissolvePropertiesController dissolveController;
    [SerializeField] private GameObject clayBlock;
    [SerializeField] private GameObject metalBlock;
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private Material geoCutoutMaterial;
    [SerializeField] private float blockFdeInDuration;

    private Renderer rendererComp;

    void Start()
    {
        rendererComp = clayBlock.GetComponent<Renderer>();

        _shellController.loadEventListener += LoadEventHandler;
        _shellController.unloadEventListener += UnloadEventHandler;
        _shellController.startEventListener += StartEventHandler;
        _shellController.stopEventListener += StopEventHandler;
        _shellController.shellStateListener += ShellStateHandler;

        Reset();
    }

    private void Reset()
    {
        clayBlock.SetActive(false);
        metalBlock.SetActive(false);
        rendererComp.material = dissolveMaterial;
        dissolveController.cutoutStandard.clip = 1;
    }

    private void LoadEventHandler()
    {
        clayBlock.SetActive(true);
        metalBlock.SetActive(true);
    }

    private void UnloadEventHandler()
    {
        Reset();
    }

    private void StartEventHandler()
    {
        DOTween.To(() => dissolveController.cutoutStandard.clip, r => {
            dissolveController.cutoutStandard.clip = r;
        }, 0, blockFdeInDuration);
    }

    private void ShellStateHandler(ShellNode node)
    {
        if(node == ShellNode.ToBurn)
        {
            rendererComp.material = geoCutoutMaterial;
        }
    }

    private void StopEventHandler()
    {
        Reset();
    }
}
