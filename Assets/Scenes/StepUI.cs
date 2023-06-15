using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StepUI : MonoBehaviour
{
    public TextMeshPro stopwatchText;
    private StageManager stagemanager;

    private int step;
    private void Start()
    {
        stopwatchText = this.GetComponent<TextMeshPro>();
        stagemanager = GameObject.Find("test").GetComponent<StageManager>();
        step = stagemanager.step_;
    }

    void Update()
    {
        DisplayStep(step);
    }

    void DisplayStep(float stepToDisplay)
    {
        stopwatchText.text = string.Format("Step : {0}", stepToDisplay);
        step = stagemanager.step_;
    }
}
