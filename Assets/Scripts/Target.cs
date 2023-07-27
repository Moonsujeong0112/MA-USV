// tartget patrol waypoint 8, 800 * 800, ��ü �պ�
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float HP { get; set; }
    public float speed;
    public int random_pos;
    

    Vector3 destination1 = new Vector3(-800, 1.0f, 800);
    Vector3 destination2 = new Vector3(-800, 1.0f, -150);
    Vector3 destination3 = new Vector3(-250, 1.0f, -150);
    Vector3 destination4 = new Vector3(-250, 1.0f, 250);
    Vector3 destination5 = new Vector3(250, 1.0f, 250);
    Vector3 destination6 = new Vector3(250, 1.0f, -150);
    Vector3 destination7 = new Vector3(350, 1.0f, -150);
    Vector3 destination8 = new Vector3(800, 1.0f, 800);
    Vector3 destination9 = new Vector3(-800, 1.0f, 600);
    Vector3 destination10 = new Vector3(800, 1.0f, 600);

    int way1 = -810;
    int way2 = 810;

    //ranway
    public int waypoint = 0;

    // �Ѿ�
    public int cooltime { get; set; }
    public int cool { get; set; }
    public GameObject bullet { get; set; }  //stagemanager���� ����ϱ� ���ؼ� ������Ƽ ��� (���Ǽҵ� ���� �� bullet�� �����ϸ� �����ϴ� ����)
    public GameObject bulletPrefab;
    public int bulletCnt;
    public float bulletSpeed;

    // �߻� ray
    public float obs_rader_size;
    public float attack_rader_size;
    public int numRays;  // Ray ����
    float distanceToAgent = 0;

    //������Ʈ
    public Transform Agent;
    public Transform attack_Agent;

    public GameObject explosionPrefab;
    public void Init()
    {
        Agent = null;
        attack_Agent = null;
    }

    public void OnEnable()
    {
        Agent = null;
        attack_Agent = null;
    }

    void FixedUpdate()
    {
        if (transform.localPosition.x <= way1 || transform.localPosition.x >= way2 || transform.localPosition.z <= way1 || transform.localPosition.z >= way2)
        {
            Quaternion currentRotation = transform.localRotation;
            float currentYaw = currentRotation.eulerAngles.y;

            // ���� ȸ�� ������ �ݴ� �������� ȸ��
            Quaternion reverseRotation = Quaternion.Euler(0, currentYaw + 120f, 0);
            transform.rotation = reverseRotation;
        }

        transform.localPosition += transform.forward * Time.deltaTime * speed;


        distanceToAgent = USVObservation();
        Attack();
    }

    float USVObservation()
    {
        float distance;
        float minDistance = obs_rader_size;    

        for(int i = 0; i < numRays; i++)
        {
            float angle = i * 2* Mathf.PI / numRays;
            Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            Ray ray = new Ray(transform.position, direction * obs_rader_size);

            RaycastHit hit;

            //Debug.DrawRay(transform.position, direction * obs_rader_size, Color.red);

            if (Physics.Raycast(ray, out hit, obs_rader_size))
            {
                if(hit.collider.gameObject.CompareTag("Agent"))
                {
                    distance = Vector3.Distance(hit.collider.transform.position, transform.position);

                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        Agent = hit.transform;
                    }

                }
            }
        }

        if (minDistance == obs_rader_size) Agent = null;

        return minDistance;
    }

    void Attack()
    {
        if (attack_Agent || (Agent && distanceToAgent < attack_rader_size))
        {
            if ((bulletCnt > 0) && cool == 0)
            {
                attack_Agent = Agent;
                Vector3 direction = (attack_Agent.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                bullet = Instantiate(bulletPrefab, transform.position, lookRotation);
                TargetBulletManager bulletscript = bullet.GetComponent<TargetBulletManager>();
                bulletscript.Launch(attack_Agent, bulletSpeed);

                bulletCnt--;
            }

            if(++cool == cooltime)
            {
                attack_Agent = null;
                cool = 0;
            }
        }
    }

    /// <summary>
    /// ��ź ���� ��
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Agent_Bullet"))
        {
            HP -= 80;

            if (HP <= 0)
            {
                HP = 200;
                Explode();
                Transform shootAgent;
                if(shootAgent = transform.parent.Find(coll.name.Substring(0,6)))
                    shootAgent.GetComponent<USV>().AddReward(1f);

                gameObject.SetActive(false);
            }
        }
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
