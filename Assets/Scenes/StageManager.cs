using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using Random = UnityEngine.Random;

public class StageManager : MonoBehaviour
{
    private SimpleMultiAgentGroup AgentGroup;
    [SerializeField]
    private List<USV> Agents;
    [SerializeField]
    private List<Target> Targets;

    EnvironmentParameters m_ResetParams;

    int Episode_ = 0;
    public int step_ { get;set; }
    public int MaxStep_;
    public int agent_hp;
    public int agent_bullet_cnt;
    public int agent_cooltime;
    public int target_hp;
    public int target_bullet_cnt;
    public int target_cooltime;

    public GameObject Ground1;
    public GameObject Ground2;
    public GameObject Ground3;
    public GameObject Ground4;
    public GameObject Ground5;

    public List<GameObject> Ground1List;
    public List<GameObject> Ground2List;
    public List<GameObject> Ground3List;
    public List<GameObject> Ground4List;
    public List<GameObject> Ground5List;

    public int AgentCount;
    public int TargetCount;

    public bool USVAttack { get; set; }
    public bool TargetAttacked { get; set; }

    void Start()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        CreateAgents();
        CreateTargets();
        EpisodeBegin();
    }

    /// <summary>
    /// Stage 밑에 지형을 생성
    /// </summary>
    public void SetStageObject()
    {
        int Ground1Count = Random.Range(0, 10);
        int Ground2Count = Random.Range(0, 10);
        int Ground3Count = Random.Range(0, 0);
        int Ground4Count = Random.Range(0, 0);
        int Ground5Count = Random.Range(0, 0);

        foreach (var obj in Ground1List) Destroy(obj);
        foreach (var obj in Ground2List) Destroy(obj);
        foreach (var obj in Ground3List) Destroy(obj);
        foreach (var obj in Ground4List) Destroy(obj);
        foreach (var obj in Ground5List) Destroy(obj);

        for (int i = 0; i < Ground1Count; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-350.0f, 350.0f), 0.05f, Random.Range(-350.0f, 350.0f));
            Quaternion rot = Quaternion.Euler(Vector3.up * Random.Range(0, 360));

            Ground1List.Add(Instantiate(Ground1, transform.position + pos, rot, transform));
        }

        for (int i = 0; i < Ground2Count; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-350.0f, 350.0f), 0.05f, Random.Range(-350.0f, 350.0f));
            Quaternion rot = Quaternion.Euler(Vector3.up * Random.Range(0, 360));

            Ground2List.Add(Instantiate(Ground2, transform.position + pos, rot, transform));
        }

        for (int i = 0; i < Ground3Count; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-350.0f, 350.0f), 0.05f, Random.Range(-350.0f, 350.0f));
            Quaternion rot = Quaternion.Euler(Vector3.up * Random.Range(0, 360));

            Ground3List.Add(Instantiate(Ground3, transform.position + pos, rot, transform));
        }

        for (int i = 0; i < Ground4Count; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-350.0f, 350.0f), 0.05f, Random.Range(-350.0f, 350.0f));
            Quaternion rot = Quaternion.Euler(Vector3.up * Random.Range(0, 360));

            Ground4List.Add(Instantiate(Ground4, transform.position + pos, rot, transform));
        }

        for (int i = 0; i < Ground5Count; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-350.0f, 350.0f), 0.05f, Random.Range(-350.0f, 350.0f));
            Quaternion rot = Quaternion.Euler(Vector3.up * Random.Range(0, 360));

            Ground5List.Add(Instantiate(Ground5, transform.position + pos, rot, transform));
        }
    }

    /// <summary>
    /// Stage 밑에 USV 생성
    /// </summary>
    private void CreateAgents()
    {
        AgentGroup = new SimpleMultiAgentGroup();
        Agents = new List<USV>();

        for (int i = 0; i < AgentCount; i++)
        {
            GameObject Agentprefab = Resources.Load<GameObject>("Agent&Target/Agent");

            if (null == Agentprefab)
                continue;

            GameObject Agent = Instantiate<GameObject>(Agentprefab, parent:transform);    //이 다음에 USV의 초기화부터 시작
            

            if (null == Agent)
                continue;

            Agent.name = string.Format("Agent{0}", i + 1);

            USV AgentIns = Agent.GetComponent<USV>();

            if (AgentIns != null)
            {
                Agents.Add(AgentIns);   //에이전트 리스트에 insert
                AgentGroup.RegisterAgent(AgentIns);     //멀티 에이전트 그룹에 등록
            }
        }
    }

    /// <summary>
    /// Target 생성
    /// </summary>
    private void CreateTargets()
    {
        Targets = new List<Target>();

        for (int i = 0; i < TargetCount; i++)
        {
            GameObject targetprefab = Resources.Load<GameObject>("Agent&Target/Target");

            if (null == targetprefab)
                continue;

            GameObject target = Instantiate<GameObject>(targetprefab, parent:transform);

            if (null == target)
                continue;

            target.name = string.Format("Target{0}", i + 1);
            
            Target targetIns = target.GetComponent<Target>();

            if (null != targetIns)
            {
                targetIns.Init();
                Targets.Add(targetIns);
            }
        }
    }

    /// <summary>
    /// Stage의 Episode 시작을 알림
    /// </summary>
    private void EpisodeBegin()
    {
        Episode_++;
        step_ = 0;
        ResetPostion();
        ResetEnv();
        //SetStageObject();
    }

    /// <summary>
    /// Agent와 Target을 초기화하는 함수
    /// </summary>
    private void ResetEnv()
    {
        for (int i = 0; i < Agents.Count; i++)
        {
            if(Episode_ != 1)
            {
                Agents[i].gameObject.SetActive(false);
                Agents[i].gameObject.SetActive(true);
                //Debug.Log("비활성화 후 활성화 됨 : "+ Agents[i].name);
                if (Agents[i].bullet) Destroy(Agents[i].bullet);
            }
            Agents[i].HP = agent_hp;
            Agents[i].bulletCnt = agent_bullet_cnt;
            Agents[i].cooltime = agent_cooltime * 50;   //1프레임당 50 step
            Agents[i].cool = 0;
            AgentGroup.RegisterAgent(Agents[i]);
        }
        for (int i = 0; i < Targets.Count; i++)
        {
            if(Episode_ != 1)
            {
                Targets[i].gameObject.SetActive(false);
                Targets[i].gameObject.SetActive(true);
                if (Targets[i].bullet) Destroy(Targets[i].bullet);
            }

            Targets[i].HP = target_hp;
            Targets[i].bulletCnt = target_bullet_cnt;
            Targets[i].cooltime = target_cooltime * 50; //1프레임당 50 step
            Targets[i].cool = 0;
            Targets[i].waypoint = 0;
        }
    }

    /// <summary>
    /// Agent와 Target의 위치를 Reset하는 함수
    /// </summary>
    private void ResetPostion()
    {
        for (int i = 0; i < Targets.Count; i++)
        {
            Vector3 pos = new Vector3(-200 + 200*i, 1f,  0f);

            Quaternion rot = Quaternion.Euler(0, 90, 0);
            Targets[i].transform.localPosition = pos;
            Targets[i].transform.rotation = rot;
        }

        for (int i = 0; i < Agents.Count; i++)
        {
            Vector3 pos = new Vector3(-800, 1f, 200.0f - 200f * i);
            Quaternion rot = Quaternion.Euler(0, 90, 0);

            Agents[i].transform.localPosition = pos;

            Agents[i].transform.rotation = rot;
            //Agents[i].transform.LookAt(Targets[i].transform);
        }
    }

    private void FixedUpdate()
    {
        // 아군 전멸 (게임 패배)
        for (int i = 0; i < Agents.Count; i++)
        {
            if (Agents[i].gameObject.activeSelf == false)
            {
                if (i == Agents.Count - 1)
                {
                    Debug.Log($"<color=red>***************Agent Lose!***************</color>");
                    AgentGroup.EndGroupEpisode();
                    EpisodeBegin();
                    break;
                }
                else continue;
            }
            else break;
        }
        // 적 전멸 (게임 승리)
        for (int i = 0; i < Targets.Count; i++)
        {
            if (Targets[i].gameObject.activeSelf == false)
            {
                if (i == Targets.Count - 1)
                {
                    Debug.Log($"<color=blue>***************Agent Win!***************</color>");
                    AgentGroup.AddGroupReward(5f + 5f * (AgentGroup.GetRegisteredAgents().Count - 1)); // 그룹에게 5 + 5 * 생존 에이전트 수 부여
                    //AgentGroup.AddGroupReward(5f); // 그룹에게 5 + 5 * 생존 에이전트 수 부여
                    AgentGroup.EndGroupEpisode();

                    EpisodeBegin();
                    break;
                }
                else continue;
            }
            else break;
        }

        step_++;
        // 에피소드 종료 (MaxStep 초과)
        for (int i = 0; i < Agents.Count; i++)
        {
            if (step_ >= MaxStep_)
            {
                Debug.Log($"<color=orange>***************End with Max Step***************</color>");
                //AgentGroup.AddGroupReward(-5f + -5f * (AgentGroup.GetRegisteredAgents().Count - 1)); // 그룹에게 5 + 5 * 생존 에이전트 수 부여
                AgentGroup.EndGroupEpisode();
                EpisodeBegin();
                break;
            }
        }
    }
}
