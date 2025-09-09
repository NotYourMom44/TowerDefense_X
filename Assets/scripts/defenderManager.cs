using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenderManager : MonoBehaviour
{
    [Header("References")]
    public GameObject defenderPrefab;
    public ProceduralTerrain terrain;

    [Header("Placement Settings")]
    public float placementHeight = 0.25f;
    public float minDistanceFromPath = 1f;
    public int sampleStep = 4;
    public int maxPositions = 40;

    [Header("Debug")]
    public bool autoPlaceAllDefenders = true;
    public float gizmoSphereSize = 0.25f;

    private List<Vector3> defenderPositions = new List<Vector3>();
    private List<bool> occupied = new List<bool>();

    void Start()
    {
        if (terrain == null)
            terrain = FindObjectOfType<ProceduralTerrain>();

        if (terrain == null || terrain.Mesh == null)
        {
            Debug.LogError("DefenderManager: ProceduralTerrain or its Mesh is missing.");
            return;
        }

        GenerateDefenderPositions();

        if (autoPlaceAllDefenders)
        {
            for (int i = 0; i < defenderPositions.Count; i++)
            {
                PlaceDefender(i);
            }
        }
    }

    void GenerateDefenderPositions()
    {
        defenderPositions.Clear();
        occupied.Clear();

        int grid = (int)terrain.width; // cast float  int
        float cell = terrain.cellSize;
        Transform terrainT = terrain.transform;

        Vector3 centerWorld = terrainT.TransformPoint(new Vector3((grid / 2f) * cell, 0f, (grid / 2f) * cell));

        int tested = 0, rejected = 0;

        for (int x = 0; x <= grid && defenderPositions.Count < maxPositions; x += sampleStep)
        {
            for (int z = 0; z <= grid && defenderPositions.Count < maxPositions; z += sampleStep)
            {
                tested++;
                Vector3 localPos = new Vector3(x * cell, 0f, z * cell);
                Vector3 sampleWorld = terrainT.TransformPoint(localPos);

                int vertsPerRow = (int)terrain.width + 1; // cast float  int
                int vertIndex = Mathf.Clamp(z * vertsPerRow + x, 0, terrain.Mesh.vertices.Length - 1);
                Vector3 vertex = terrain.Mesh.vertices[vertIndex];
                sampleWorld.y = terrainT.TransformPoint(vertex).y + placementHeight;

                bool tooClose = false;

                if (terrain.spawnPoints != null)
                {
                    foreach (var spawn in terrain.spawnPoints)
                    {
                        float d = Vector2.Distance(new Vector2(sampleWorld.x, sampleWorld.z),
                                                   new Vector2(spawn.x, spawn.z));
                        if (d < minDistanceFromPath)
                        {
                            tooClose = true;
                            rejected++;
                            break;
                        }
                    }
                }

                float distToCenter = Vector2.Distance(new Vector2(sampleWorld.x, sampleWorld.z),
                                                     new Vector2(centerWorld.x, centerWorld.z));
                if (distToCenter < minDistanceFromPath * 0.5f)
                {
                    tooClose = true;
                    rejected++;
                }

                if (!tooClose)
                {
                    defenderPositions.Add(sampleWorld);
                    occupied.Add(false);
                }
            }
        }

        Debug.Log($"DefenderManager: tested {tested} samples, rejected {rejected}, kept {defenderPositions.Count}.");
    }

    public void PlaceDefender(int index)
    {
        if (defenderPrefab == null) return;
        if (index < 0 || index >= defenderPositions.Count) return;
        if (occupied[index]) return;

        Instantiate(defenderPrefab, defenderPositions[index], Quaternion.identity);
        occupied[index] = true;
    }

    void OnDrawGizmos()
    {
        if (defenderPositions == null) return;
        for (int i = 0; i < defenderPositions.Count; i++)
        {
            Vector3 lifted = defenderPositions[i] + Vector3.up * 0.15f;
            Gizmos.color = occupied[i] ? Color.red : Color.green;
            Gizmos.DrawSphere(lifted, gizmoSphereSize);
        }
    }
}
