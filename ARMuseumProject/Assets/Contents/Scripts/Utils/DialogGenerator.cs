using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogGenerator : MonoBehaviour
{
    public GameObject dialogPrefab;
    public const float dialogDuration = 6f;
    private GameObject dialog;

    public void GenerateDialog(string content)
    {
        dialog = Instantiate(dialogPrefab, transform, false);  
        dialog.GetComponent<Dialog>().SetContent(content);
        StartCoroutine("StartDialogAndDestory");
    }

    private IEnumerator StartDialogAndDestory()
    {
        dialog.GetComponent<Dialog>().StartDialog();

        yield return new WaitForSeconds(dialogDuration);

        Destroy(dialog);
    }
}
