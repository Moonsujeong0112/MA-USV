// tartget patrol waypoint 8, 800 * 800, 전체 왕복
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float HP { get; set; }
    public float moveSpeed;

    int way1 = -810;
    int way2 = 810;

    // 포탄
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

    private Vector3 currentPosition;
    private Quaternion currentRotation;
    private bool xTouched;
    private bool zTouched;

    public void OnEnable()
    {
        Agent = null;
        attack_Agent = null;
        xTouched = false;
        zTouched = false;
    }

    void FixedUpdate()
    {
        /*        if (transform.localPosition.x <= way1 || transform.localPosition.x >= way2 || transform.localPosition.z <= way1 || transform.localPosition.z >= way2)
                {
                    Quaternion currentRotation = transform.localRotation;
                    float currentYaw = currentRotation.eulerAngles.y;

                    // 기존 회전 방향의 반대 방향으로 회전
                    Quaternion reverseRotation = Quaternion.Euler(0, currentYaw + 170f, 0);
                    transform.rotation = reverseRotation;
                }*/
        currentPosition = transform.localPosition;
        currentRotation = transform.localRotation;

        if ((xTouched || zTouched) && (currentPosition.x > 850 || currentPosition.x < -850 || currentPosition.z > 850 || currentPosition.z < -850))
            transform.localRotation = Quaternion.Euler(0, currentRotation.eulerAngles.y + 180f, 0);

        if ((currentPosition.z > 810 || currentPosition.z < -810) && !xTouched && !zTouched)
        {
            zTouched = true;
            if (currentPosition.x > 0)
                transform.localRotation = Quaternion.Euler(0, 90, 0);
            else
                transform.localRotation = Quaternion.Euler(0, 270, 0);
        }

        if ((currentPosition.x > 810 || currentPosition.x < -810) && !xTouched && !zTouched)
        {
            xTouched = true;
            if (currentPosition.z > 0)
                transform.localRotation = Quaternion.Euler(0, 0, 0);
            else
                transform.localRotation = Quaternion.Euler(0, 180, 0);
        }




        transform.localPosition += transform.forward * Time.deltaTime * moveSpeed;

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
            Vector3 direction = new (Mathf.Sin(angle), 0, Mathf.Cos(angle));
            Ray ray = new (transform.position, direction * obs_rader_size);

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
            HP -= 80;

            if (HP <= 0)
            {
                HP = 200;
                Explode();
                Transform shootAgent;
                if (shootAgent = transform.parent.Find(coll.name.Substring(0, 6)))
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
