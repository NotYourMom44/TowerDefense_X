using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth;
    public float attackRange = 8f;
    public int damage = 10;
    public float attackInterval = 1f;

    private float attackTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
    }

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

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
