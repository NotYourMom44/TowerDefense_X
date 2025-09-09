using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public ProceduralTerrain terrain;
    private Vector3[] path;
    private int pathIndex = 0;
    public float moveSpeed = 2f;
    public int health = 10;

    void Update()
    {
        if (path == null || path.Length == 0) return;

        Vector3 target = path[pathIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            pathIndex++;
            if (pathIndex >= path.Length)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetTarget(Vector3[] newPath)
    {
        path = newPath;
        pathIndex = 0;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0) Destroy(gameObject);
    }
}







