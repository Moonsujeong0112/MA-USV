using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

public class USV : Agent
{
    // ������Ʈ Inspector
    public float HP { get; set; }
    public float moveSpeed;
    public float turnSpeed;

    // ������Ʈ ���̴�
    RayPerceptionSensorComponent3D raycomponent;
    RayPerceptionInput rayinput;
    RayPerceptionOutput rayperceive;
    float rayperceptionsensorLength;

    // ������Ʈ ���̴� ������
    bool isAbleToAttackTarget;
    public bool isAbleToAttackUSV { get; set; }
    float targetDistance;
    Vector3 targetPositionVector;


    // �̻���
    
    public GameObject bullet { get; set; }
    public GameObject bulletPrefab;
    public int bulletCnt { get; set; }
    public int cooltime { get; set; }
    public int cool { get; set; }
    public float bulletSpeed;
    public GameObject explosionPrefab;

    //�̻��� �Ÿ�
    public float rader_size;
    public float obs_rader_size;
    public int numRays;

    // Ÿ��
    public Transform target;
    public Transform attack_target;
    public Queue<KeyValuePair<string, float>> target_queue;

    // Stage ����
    private int Max_Step;

    // rewardUI ���
    private float lastReward = 0.0f;

    /// <summary>
    /// Ʈ���̴� ������ �� �ѹ��� ȣ��
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        // �ʱ�ȭ
        raycomponent = this.GetComponent<RayPerceptionSensorComponent3D>();
        rayinput = raycomponent.GetRayPerceptionInput();
        rayperceptionsensorLength = rayinput.RayLength * transform.localScale.x;    //������ ���� rayperceptionsensor3D�� ���� ���ȭ

        MaxStep = 0;
    }

    /// <summary>
    /// �� ���Ǽҵ� ���� �� ȣ�� �Լ�
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
                    Debug.Log("target�� �������� ����!!");
                Debug.Log(
                     "\ntarget���� �Ÿ� : " + targetDistance +
                     "\ntarget�� ��� ��ġ : " + targetPositionVector +
                     "\nTarget�� ��ҳ�? : " + isAbleToAttackTarget
                     );*/


        //�Ÿ� ���� -> ���� ����� Ÿ�� ���� -> ���� ����� Ÿ�� ��� ��ġ �ľ� -> �߻� ray�� ������ ����

        //360�� ray���� target���� �Ÿ�(1)
        sensor.AddObservation(targetDistance);
        //Target�� ��� ��ġ(2)
        sensor.AddObservation(targetPositionVector.x);
        sensor.AddObservation(targetPositionVector.z);
        //Target�� USV�� �߻� ray�� ��Ҵ���(1)
        sensor.AddObservation(isAbleToAttackTarget);
        //Target�� HP(1)
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
        //�ൿ
        MoveAgent(actions.DiscreteActions);

        //������
        targetDistance = TargetObservationDistance();
        targetPositionVector = TargetObservationVector();
        if (isAbleToAttackTarget = isAttack())
            AddReward(2f / Max_Step);

        if (isCloser() && IsLocatedToSight())   //�����̰��� �þ߰� 60�� �ȿ� ������ ��
            AddReward(1.5f / Max_Step);

        //����
        AddReward(-1f / Max_Step);   //���Ƽ 1) �� ����
        lastReward = GetCumulativeReward();
    }

    // ������ �����带 ��ȯ�ϴ� �Լ�
    public float GetLastReward()
    {
        return lastReward;
    }

    /// <summary>
    /// �������� ����
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
    /// �����̳� �������̳� Ÿ���̳� USV�� �ε����� ��
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
            gameObject.SetActive(false);        //�ڽ� USV false
        }
    }

    /// <summary>
    /// ��ź ���� ��
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
    /// �� ���� �� ���� ����� Ÿ�ٰ��� �Ÿ� return
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
            KeyValuePair<string, float> pair = new KeyValuePair<string, float>(target.name, minDistance);   //<�̸�, �ִܰŸ�>�� pair�� �����Ͽ� isCloser �Լ����� ���
            target_queue.Enqueue(pair);
        }

        return minDistance;
    }

    /// <summary>
    /// ���� ���⿡ 60���ȿ� ���� �ִ���
    /// </summary>
    /// <returns></returns>
    public bool IsLocatedToSight()
    {
        if (!target)
            return false;

        Vector3 TargetVector = (target.localPosition - transform.localPosition).normalized; //Ÿ�ٰ� USV���� ��ġ����
        //Debug.Log("��ġ���� : " + TargetVector);
        float dot = Vector3.Dot(transform.forward, TargetVector);   //��ġ���Ϳ� �ڽ��� ���⺤���� ����
        //Debug.Log("������ : " + dot);
        float angle = (float)Math.Cos(30*Mathf.Deg2Rad);  //���� �þ߰�
        //Debug.Log("cos30�� : " + angle);
        if (dot < angle)    //���� �þ߰����� ū ���� ���� ��ġ�� ��
            return false;

        return true;
    }

    /// <summary>
    /// <�̸�, �ִܰŸ�> pair�� ���Ͽ� ������ ���� ����� ���̶� ���� ��������� true�� return�ϴ� �Լ�
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
    /// �̻��� �߻�
    /// </summary>
    bool isAttack()
    {
        if (attack_target || (target && (targetDistance < rader_size)))   //���� ����� ���� �Ǿ��ų� ������ Ÿ���� �����ϰ� Ÿ���� ���� ���� �ȿ� ���� ��
        {
            if ((bulletCnt > 0) && cool == 0)    //bullet�� �����ְ� ��Ÿ���� ���� ��
            {
                attack_target = target;
                Vector3 direction = (attack_target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                bullet = Instantiate(bulletPrefab, transform.position, lookRotation);
                bullet.name = this.name + "_bullet";
                AgentBulletManager bulletscript = bullet.GetComponent<AgentBulletManager>();
                bulletscript.Launch(attack_target, bulletSpeed);

                bulletCnt--;

                //AddReward(1);  //���� 2) ��ź�� �� ���
            }

            if (++cool == cooltime)    //��Ÿ���� 8�ʰ� �Ǹ� �ٽ� �� �غ� �Ϸ�
            {
                attack_target = null;
                cool = 0;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// ���� ����Ʈ
    /// </summary>
    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
