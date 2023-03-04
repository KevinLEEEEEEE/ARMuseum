using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetUserID : MonoBehaviour
{
    [SerializeField] private GameController_Historical _gameController;

    void Start()
    {
        transform.GetComponent<TextMeshProUGUI>().text = _gameController.UserID;
    }
}
