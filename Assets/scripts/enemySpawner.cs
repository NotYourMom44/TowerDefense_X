using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public ProceduralTerrain terrain;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public int maxEnemies = 20;

    private int enemiesSpawned = 0;

    void Start()
    {
        if (terrain == null)
        {
            terrain = FindObjectOfType<ProceduralTerrain>();
        }

        if (terrain == null || terrain.spawnPoints == null || terrain.spawnPoints.Length == 0 || terrain.towerInstance == null)
        {
            Debug.LogError("EnemySpawner: Terrain or spawn points or tower not found!");
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (enemiesSpawned < maxEnemies)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || terrain.spawnPoints.Length == 0 || terrain.towerInstance == null)
            return;

        // pick a random spawn point
        int spawnIndex = Random.Range(0, terrain.spawnPoints.Length);
        Vector3 spawnPos = terrain.spawnPoints[spawnIndex];

        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Assign path to enemy: start at spawn point, end at tower
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            Vector3[] path = new Vector3[2];
            path[0] = spawnPos;
            path[1] = terrain.towerInstance.transform.position;
            enemy.SetTarget(path);

            enemy.terrain = terrain;
        }

        enemiesSpawned++;
    }
}





