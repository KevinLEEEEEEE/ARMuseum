using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Timeline : MonoBehaviour
{
    [SerializeField] private ClockController _clockController;
    [SerializeField] private TextMeshProUGUI dynastyComp;
    [SerializeField] private TextMeshProUGUI dateComp;
    [SerializeField] private float fadeInDuration;
    private static readonly Dictionary<int, string> dynastyChronology = new()
    {
        { -2500, "�Ϲ�" },
        { -2070, "��" },
        { -1600, "��" },
        { -770, "����" },
        { -475, "ս��" },
        { -221, "��" },
        { -202, "��" },
        { 220, "����" },
        { 265, "��" },
        { 304, "ʮ����" },
        { 420, "�ϱ���" },
        { 581, "��" },
        { 618, "��" },
        { 907, "���ʮ��-��" },
        { 960, "��" },
        { 1038, "����" },
        { 1115, "��" },
        { 1271, "Ԫ" },
        { 1279, "Ԫ" },
        { 1368, "��" },
        { 1644, "��" },
        { 1912, "�л����" },
        { 1949, "�л����񹲺͹�" },
    };

    void Start()
    {
        _clockController.clockListener += UpdateTimeline;
        _clockController.startEventListener += StartTimer;
        _clockController.stopEventListener += StopTimer;
    }
    private void StartTimer()
    {
        StartCoroutine(fadeInDuration.Tweeng((t) =>
        {
            dynastyComp.color = new Color(1, 1, 1, t);
            dateComp.color = new Color(1, 1, 1, t);
        }, 0f, 1f));
    }

    private void StopTimer()
    {
    }

    private void UpdateTimeline(int date, float percentage)
    {
        UpdateDate(date);
        UpdateDynasty(date);
    }

    private void UpdateDate(int date)
    {
        dateComp.text = string.Format("{0}{1}��", date < 0 ? "��Ԫǰ" : "��Ԫ", Mathf.Abs(date));
    }

    private void UpdateDynasty(int date)
    {
        if(dynastyChronology.Count > 0)
        {
            int key = dynastyChronology.First().Key;

            if (key <= date)
            {
                dynastyComp.text = dynastyChronology[key];
                dynastyChronology.Remove(key);

                if(key == 1949)
                {

                }
            }
        }
    }
}
