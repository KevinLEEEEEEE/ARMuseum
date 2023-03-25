using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Description : MonoBehaviour
{
    [SerializeField] private GameController m_GameController;
    private GameObject root;

    void Start()
    {
        root = transform.GetChild(0).gameObject;

        m_GameController.BeginTourEvent += BeginTourEventHandler;
        m_GameController.EndTourEvent += EndTourEventHandler;

        EndTourEventHandler();
    }

    private void BeginTourEventHandler()
    {
        root.SetActive(true);
    }

    private void EndTourEventHandler()
    {
        root.SetActive(false);
    }
}
