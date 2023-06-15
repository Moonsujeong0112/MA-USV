using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBulletManager : MonoBehaviour
{
    Transform target;
    float speed;
    public float turningForce;
    public GameObject explosionPrefab;

    public void Launch(Transform target, float launchSpeed)
    {
        this.target = target;
        speed = launchSpeed;
    }

    public void Start()
    {
        Destroy(gameObject, 8.0f);
    }

    public void LookAtTarget()
    {
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turningForce * Time.deltaTime);
    }

    // Update is called once per frame
    public void Update()
    {
        transform.Translate(new Vector3(0, 0, speed));
        if (target != null) LookAtTarget();
        if (target.gameObject.activeSelf == false) Destroy(gameObject);
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("DEAD_ZONE") || other.CompareTag("Ground"))
        {
            Explode();
            Destroy(gameObject);
        }
        else if (other.CompareTag("Target"))
        {
            Explode();
            Destroy(gameObject);
        }
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
