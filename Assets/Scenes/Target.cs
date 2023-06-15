// tartget patrol waypoint 8, 800 * 800, 전체 왕복
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float HP { get; set; }
    public float speed;

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

    //int way1 = -500;
    int way1 = -810;
    //int way2 = 500;
    int way2 = 810;

    public int waypoint { get; set; }

    // 총알
    public int cooltime { get; set; }
    public int cool { get; set; }
    public GameObject bullet { get; set; }  //stagemanager에서 사용하기 위해서 프로퍼티 사용 (에피소드 시작 시 bullet이 존재하면 삭제하는 역할)
    public GameObject bulletPrefab;
    public int bulletCnt;
    public float bulletSpeed;

    // 발사 ray
    public float obs_rader_size;
    public float attack_rader_size;
    public int numRays;  // Ray 개수
    float distanceToAgent = 0;

    //에이전트
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
        /*        // target move1
                if (waypoint == 0)
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination1, Time.deltaTime * speed);
                    if (transform.localPosition.x == destination1.x)
                    {
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                        waypoint = 1;
                    }
                }
                else if (waypoint == 1)
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination9, Time.deltaTime * speed);
                    if (transform.localPosition == destination9)
                    {
                        transform.rotation = Quaternion.Euler(0, 90, 0);
                        waypoint = 9;
                    }
                }
                else if (waypoint == 9)
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination10, Time.deltaTime * speed);
                    if (transform.localPosition == destination10)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        waypoint = 10;
                    }
                }
                else
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, destination8, Time.deltaTime * speed);
                    if (transform.localPosition == destination8)
                    {
                        transform.rotation = Quaternion.Euler(0, 270, 0);
                        waypoint = 0;
                    }
                }*/

        transform.localPosition += transform.forward *Time.deltaTime * speed;

        if (transform.localPosition.x <= way1)
            transform.localRotation = Quaternion.Euler(0, 90, 0);
        if (transform.localPosition.x >= way2)
            transform.localRotation = Quaternion.Euler(0, 270, 0);

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
    /// 포탄 맞을 시
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Agent_Bullet"))
        {
            //Debug.Log($"<color=cyan>***************{name} before attack HP: {HP}***************</color>");
            HP -= 80;
            //Debug.Log($"<color=cyan>***************{name} after attack HP: {HP}***************</color>");
            if (HP <= 0)
            {
                //Debug.Log($"<color=green>***************{name} Destroyed!***************</color>");
                HP = 200;
                Explode();
                Transform shootAgent;
                if(shootAgent = transform.parent.Find(coll.name.Substring(0,6)))
                {
                    shootAgent.GetComponent<USV>().AddReward(1f);
                }

                StartCoroutine(DeactivateAfterDelay(0.02f));
            }
        }
    }

    IEnumerator DeactivateAfterDelay(float delay)
    {
        // delay 시간만큼 대기
        yield return new WaitForSeconds(delay);

        // gameObject를 비활성화
        gameObject.SetActive(false);
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
