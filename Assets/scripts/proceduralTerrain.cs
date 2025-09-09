using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTerrain : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int gridSize = 20;          // number of cells per side
    public float cellSize = 1f;        // size of each cell
    public float heightScale = 2f;     // terrain height multiplier

    [Header("Tower")]
    public GameObject towerPrefab;
    [HideInInspector] public GameObject towerInstance;

    [Header("Spawn Points")]
    public Vector3[] spawnPoints;

    [Header("Path Points")]
    public Transform[] pathPoints;

    private Mesh terrainMesh;
    private Vector3[] vertices;

    // Expose these for DefenderManager
    public Vector3[] Vertices => vertices;
    public float width => gridSize * cellSize;
    public float depth => gridSize * cellSize;
    public Mesh Mesh => terrainMesh;

    void Awake()
    {
        GenerateTerrain();
        PlaceTower();
        SetupSpawnPoints();
    }

    void GenerateTerrain()
    {
        terrainMesh = new Mesh();
        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        int[] triangles = new int[gridSize * gridSize * 6];

        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * heightScale;
                vertices[z * (gridSize + 1) + x] = new Vector3(x * cellSize, y, z * cellSize);
            }
        }

        int triIndex = 0;
        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int i = z * (gridSize + 1) + x;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + gridSize + 1;
                triangles[triIndex++] = i + 1;

                triangles[triIndex++] = i + 1;
                triangles[triIndex++] = i + gridSize + 1;
                triangles[triIndex++] = i + gridSize + 2;
            }
        }

        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.RecalculateNormals();

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = terrainMesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard"));
    }

    void PlaceTower()
    {
        if (towerPrefab != null)
        {
            Vector3 center = new Vector3(width * 0.5f, 0f, depth * 0.5f);
            towerInstance = Instantiate(towerPrefab, center, Quaternion.identity);
        }
    }

    void SetupSpawnPoints()
    {
        spawnPoints = new Vector3[3];
        spawnPoints[0] = new Vector3(0, 0, 0);
        spawnPoints[1] = new Vector3(width, 0, 0);
        spawnPoints[2] = new Vector3(0, 0, depth);
    }
}
