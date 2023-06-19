using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class USV : Agent
{
    // 에이전트 Inspector
    public float HP { get; set; }
    public float moveSpeed;
    public float turnSpeed;

    // 에이전트 레이더
    RayPerceptionSensorComponent3D raycomponent;
    RayPerceptionInput rayinput;
    RayPerceptionOutput rayperceive;
    float rayperceptionsensorLength;

    // 에이전트 레이더 관측값
    bool isAbleToAttackTarget;
    public bool isAbleToAttackUSV { get; set; }
    float targetDistance;
    Vector3 targetPositionVector;


    // 미사일
    
    public GameObject bullet { get; set; }
    public GameObject bulletPrefab;
    public int bulletCnt { get; set; }
    public int cooltime { get; set; }
    public int cool { get; set; }
    public float bulletSpeed;
    public GameObject explosionPrefab;

    //미사일 거리
    public float rader_size;
    public float obs_rader_size;
    public int numRays;

    // 타겟
    public Transform target;
    public Transform attack_target;
    public Queue<KeyValuePair<string, float>> target_queue;

    // Stage 참조
    private int Max_Step;

    // rewardUI 사용
    private float lastReward = 0.0f;

    /// <summary>
    /// 트레이닝 시작할 때 한번만 호출
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        // 초기화
        raycomponent = this.GetComponent<RayPerceptionSensorComponent3D>();
        rayinput = raycomponent.GetRayPerceptionInput();
        rayperceptionsensorLength = rayinput.RayLength * transform.localScale.x;    //비율에 따른 rayperceptionsensor3D의 길이 상수화

        MaxStep = 0;
    }

    /// <summary>
    /// 매 에피소드 시작 시 호출 함수
    /// </summary>
    public override void OnEpisodeBegin()
    {
        target = null;
        attack_target = null;
        target_queue = new Queue<KeyValuePair<string, float>>();
        targetDistance = TargetObservationDistance();
        targetPositionVector = TargetObservationVector();
        isAbleToAttackTarget = isAttack();
        Max_Step = transform.parent.GetComponent<StageManager>().MaxStep_;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*        if (target)
                    Debug.Log("target : " + target.gameObject.name);
                else
                    Debug.Log("target이 측정되지 않음!!");
                Debug.Log(
                     "\ntarget과의 거리 : " + targetDistance +
                     "\ntarget의 상대 위치 : " + targetPositionVector +
                     "\nTarget이 닿았냐? : " + isAbleToAttackTarget
                     );*/


        //거리 관측 -> 가장 가까운 타겟 지정 -> 가장 가까운 타겟 상대 위치 파악 -> 발사 ray에 닿으면 공격

        //360도 ray에서 target과의 거리(1)
        sensor.AddObservation(targetDistance);
        //Target의 상대 위치(2)
        sensor.AddObservation(targetPositionVector.x);
        sensor.AddObservation(targetPositionVector.z);
        //Target이 USV의 발사 ray에 닿았는지(1)
        sensor.AddObservation(isAbleToAttackTarget);
        //Target의 HP(1)
        if (target)
            sensor.AddObservation(target.GetComponent<Target>().HP);
        else
            sensor.AddObservation(999);
    }


    private void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action1 = act[0];
        var action2 = act[1];

        switch (action1)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
        }

        switch (action2)
        {
            case 1:
                rotateDir = transform.up;
                break;
            case 2:
                rotateDir = -transform.up;
                break;
        }

        transform.position += dirToGo * moveSpeed * Time.fixedDeltaTime;
        transform.Rotate(rotateDir, turnSpeed * Time.fixedDeltaTime);
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        //행동
        MoveAgent(actions.DiscreteActions);

        //관찰값
        targetDistance = TargetObservationDistance();
        targetPositionVector = TargetObservationVector();
        if (isAbleToAttackTarget = isAttack())
            AddReward(2f / Max_Step);

        if (isCloser() && IsLocatedToSight())   //가까이가고 시야각 60도 안에 존재할 때
            AddReward(1.5f / Max_Step);

        //보상
        AddReward(-1f / Max_Step);   //페널티 1) 메 스텝
        lastReward = GetCumulativeReward();
    }

    // 마지막 리워드를 반환하는 함수
    public float GetLastReward()
    {
        return lastReward;
    }

    /// <summary>
    /// 수동으로 조작
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;

        action.Clear();

        if (Input.GetKey(KeyCode.W)) action[0] = 1;
        if (Input.GetKey(KeyCode.S)) action[0] = 2;
        if (Input.GetKey(KeyCode.A)) action[1] = 2;
        if (Input.GetKey(KeyCode.D)) action[1] = 1;
    }

    /// <summary>
    /// 지형이나 데드존이나 타겟이나 USV에 부딪혔을 때
    /// </summary>
    /// <param name="coll"></param>
    void OnCollisionEnter(Collision coll)
    {
        if (coll.collider.CompareTag("Ground"))
        {
            Debug.Log($"<color=red>***************Collision by</color> <color=orange> GROUND</color><color=red>***************</color>");
            Explode();
            AddReward(-1f);
            gameObject.SetActive(false);
        }

        if (coll.collider.CompareTag("DEAD_ZONE"))
        {
            Debug.Log($"<color=red>***************Collision by</color> <color=blue>  DEAD_ZONE</color><color=red>***************</color>");
            Explode();
            AddReward(-1f);
            gameObject.SetActive(false);
        }

        if (coll.collider.CompareTag("Target"))
        {
            Debug.Log($"<color=red>***************Collision by</color> <color=green>  Target</color><color=red>***************</color>");
            Explode();
            AddReward(-1f);
            gameObject.SetActive(false);
        }

        if (coll.collider.CompareTag("Agent"))
        {
            Debug.Log($"<color=red>***************Collision by</color> <color=green>  other Agent</color><color=red>***************</color>");
            Explode();
            AddReward(-1f);
            gameObject.SetActive(false);        //자신 USV false
        }
    }

    /// <summary>
    /// 포탄 맞을 시
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Target_Bullet"))
        {
            //Debug.Log($"<color=orange>***************{name} before attack HP: {HP}***************</color>");
            HP -= 80;
            //AddReward(-5f);
            //Debug.Log($"<color=orange>***************{name} after attack HP: {HP}***************</color>");
            if (HP <= 0)
            {
                Explode();
                //AddReward(-5f);
                Destroy(coll.gameObject);
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 적 관측 후 가장 가까운 타겟간의 거리 return
    /// </summary>
    public float TargetObservationDistance()
    {
        float distance;
        float minDistance = obs_rader_size;

        for (int i = 0; i < numRays; i++)
        {
            float angle = i * 2 * Mathf.PI / numRays;
            Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            Ray ray = new Ray(transform.position, direction * obs_rader_size);

            RaycastHit hit;

            //Debug.DrawRay(transform.position, direction * obs_rader_size, Color.red);

            if (Physics.Raycast(ray, out hit, obs_rader_size))
            {
                if (hit.collider.gameObject.CompareTag("Target"))
                {
                    distance = Vector3.Distance(hit.collider.transform.position, transform.position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        target = hit.transform;
                    }

                }
            }
        }

        if (minDistance == obs_rader_size) target = null;

        else
        {
            KeyValuePair<string, float> pair = new KeyValuePair<string, float>(target.name, minDistance);   //<이름, 최단거리>의 pair로 저장하여 isCloser 함수에서 사용
            target_queue.Enqueue(pair);
        }

        return minDistance;
    }

    /// <summary>
    /// 보는 방향에 60도안에 적이 있는지
    /// </summary>
    /// <returns></returns>
    public bool IsLocatedToSight()
    {
        if (!target)
            return false;

        Vector3 TargetVector = (target.localPosition - transform.localPosition).normalized; //타겟과 USV간의 위치벡터
        //Debug.Log("위치벡터 : " + TargetVector);
        float dot = Vector3.Dot(transform.forward, TargetVector);   //위치벡터와 자신의 방향벡터의 내적
        //Debug.Log("내적값 : " + dot);
        float angle = (float)Math.Cos(30*Mathf.Deg2Rad);  //보는 시야각
        //Debug.Log("cos30도 : " + angle);
        if (dot < angle)    //보는 시야각보다 큰 곳에 적이 위치할 때
            return false;

        return true;
    }

    /// <summary>
    /// <이름, 최단거리> pair를 비교하여 관측한 가장 가까운 적이랑 점점 가까워지면 true를 return하는 함수
    /// </summary>
    /// <returns></returns>
    public bool isCloser()
    {
        if (target_queue.Count > 1)
        {
            KeyValuePair<string, float> firstTarget = target_queue.Dequeue();
            KeyValuePair<string, float> secondTarget = target_queue.Peek();

            double FtDistance = firstTarget.Value;
            double StDistance = secondTarget.Value;

            if (firstTarget.Key == secondTarget.Key && FtDistance > StDistance)
                return true;
        }

        return false;
    }

    private Vector3 TargetObservationVector()
    {
        if (target) return (target.localPosition - transform.localPosition).normalized;
        else return  Vector3.zero;
    }


    /// <summary>
    /// 미사일 발사
    /// </summary>
    bool isAttack()
    {
        if (attack_target || (target && (targetDistance < rader_size)))   //공격 대상이 지정 되었거나 관측된 타겟이 존재하고 타겟이 공격 범위 안에 있을 때
        {
            if ((bulletCnt > 0) && cool == 0)    //bullet이 남아있고 쿨타임이 끝날 때
            {
                attack_target = target;
                Vector3 direction = (attack_target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                bullet = Instantiate(bulletPrefab, transform.position, lookRotation);
                bullet.name = this.name + "_bullet";
                AgentBulletManager bulletscript = bullet.GetComponent<AgentBulletManager>();
                bulletscript.Launch(attack_target, bulletSpeed);

                bulletCnt--;

                //AddReward(1);  //보상 2) 포탄을 쏠 경우
            }

            if (++cool == cooltime)    //쿨타임이 8초가 되면 다시 쏠 준비 완료
            {
                attack_target = null;
                cool = 0;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 폭발 이펙트
    /// </summary>
    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
