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
    private string rewardText; // �ؽ�Ʈ ������ ������ ���� ����

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
        rewardText = ""; // �ʱ�ȭ
    }

    void FixedUpdate()
    {
        UpdateRewardText(Agent);
        stopwatchText.text = rewardText; // �ؽ�Ʈ ����
    }

    void UpdateRewardText(List<Transform> Agent)
    {
        rewardText = ""; // �ؽ�Ʈ �ʱ�ȭ
        foreach (var agent in Agent)
        {
            string name = agent.name;
            float reward = agent.GetComponent<USV>().GetCumulativeReward();

            // ������Ʈ�� active���� ���� ���, ������ ������ ������ ������Ʈ
            if (!agent.gameObject.activeSelf)
                reward = agent.GetComponent<USV>().GetLastReward();

            rewardText += string.Format("{0} : {1}\n", name, reward);
        }
    }
}
