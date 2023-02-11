using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogGenerator : MonoBehaviour
{
    public GameObject dialogPrefab;
    public const float dialogDuration = 6.5f;

    public void GenerateDialog(string content)
    {
        GameObject dialog = Instantiate(dialogPrefab, transform, false);  
        dialog.GetComponent<Dialog>().SetContent(content);
        StartCoroutine(StartDialogAndDestory(dialog));
    }

    private IEnumerator StartDialogAndDestory(GameObject dialog)
    {
        dialog.GetComponent<Dialog>().StartDialog();

        yield return new WaitForSeconds(dialogDuration);

        Destroy(dialog);
    }
}
