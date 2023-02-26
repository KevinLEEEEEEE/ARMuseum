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
        { -2500, "上古" },
        { -2070, "夏" },
        { -1600, "商" },
        { -770, "春秋" },
        { -475, "战国" },
        { -221, "秦" },
        { -202, "汉" },
        { 220, "三国" },
        { 265, "晋" },
        { 304, "十六国" },
        { 420, "南北朝" },
        { 581, "隋" },
        { 618, "唐" },
        { 907, "五代十国-辽" },
        { 960, "宋" },
        { 1038, "西夏" },
        { 1115, "金" },
        { 1271, "元" },
        { 1279, "元" },
        { 1368, "明" },
        { 1644, "清" },
        { 1912, "中华民国" },
        { 1949, "中华人民共和国" },
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
        dateComp.text = string.Format("{0}{1}年", date < 0 ? "公元前" : "公元", Mathf.Abs(date));
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
