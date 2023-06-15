using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class RewardUI : MonoBehaviour
{
    public TextMeshPro stopwatchText;
    private StageManager stagemanager;

    private GameObject Test;
    private List<Transform> Agent;
    private string rewardText; // 텍스트 내용을 저장할 전역 변수

    private void Start()
    {
        stopwatchText = this.GetComponent<TextMeshPro>();
        Test = GameObject.Find("test");
        stagemanager = Test.GetComponent<StageManager>();
        Agent = new List<Transform>();

        int count = Test.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = Test.transform.GetChild(i);
            if (child.name.StartsWith("Agent"))
                Agent.Add(child);
        }
        rewardText = ""; // 초기화
    }

    void FixedUpdate()
    {
        UpdateRewardText(Agent);
        stopwatchText.text = rewardText; // 텍스트 갱신
    }

    void UpdateRewardText(List<Transform> Agent)
    {
        rewardText = ""; // 텍스트 초기화
        foreach (var agent in Agent)
        {
            string name = agent.name;
            float reward = agent.GetComponent<USV>().GetCumulativeReward();

            // 에이전트가 active하지 않은 경우, 마지막 리워드 정보로 업데이트
            if (!agent.gameObject.activeSelf)
                reward = agent.GetComponent<USV>().GetLastReward();

            rewardText += string.Format("{0} : {1}\n", name, reward);
        }
    }
}
