using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeUI : MonoBehaviour
{
    public TextMeshPro stopwatchText;
    private StageManager stagemanager;

    private float elapsedTime;
    private void Start()
    {
        stopwatchText = this.GetComponent<TextMeshPro>();
        stagemanager = GameObject.Find("test").GetComponent<StageManager>();
    }

    void FixedUpdate()
    {
        // 에피소드 종료시 초시계 다시 0으로 설정 
        if (stagemanager.step_ == 1)
            elapsedTime = 0;

        elapsedTime += Time.deltaTime;
        DisplayTime(elapsedTime);
    }

    void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        float fractions = Mathf.FloorToInt((timeToDisplay * 100f) % 100f);

        stopwatchText.text = string.Format("Time : {0:00}:{1:00}:{2:00}", minutes, seconds, fractions);
    }
}