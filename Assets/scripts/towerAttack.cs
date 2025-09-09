using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    public float attackRange = 10f;
    public float attackInterval = 1f;
    public int damage = 10;

    private float attackTimer = 0f;

    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Enemy target = FindClosestEnemy();
            if (target != null)
            {
                target.TakeDamage(damage);
                attackTimer = attackInterval;
            }
        }
    }

    Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float minDist = Mathf.Infinity;

        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist <= attackRange && dist < minDist)
            {
                minDist = dist;
                closest = e;
            }
        }

        return closest;
    }
}
