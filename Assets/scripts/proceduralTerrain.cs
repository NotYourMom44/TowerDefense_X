using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ProceduralTerrain : MonoBehaviour
{
    [Header("Grid")]
    public int gridSize = 20;
    public float cellSize = 1f;

    [Header("Heights")]
    public float heightScale = 5f;
    public float noiseScale = 0.2f;
    public bool randomizeSeed = true;
    public int seed = 0;

    [Header("Paths")]
    public int numberOfPaths = 3;
    public float pathWidth = 2f;
    [HideInInspector] public Vector3[] spawnPoints;

    [Header("Tower")]
    public GameObject towerPrefab;
    private GameObject towerInstance;

    private Mesh mesh;
    private MeshCollider meshCollider;

    void Start()
    {
        // Cache collider once
        meshCollider = GetComponent<MeshCollider>();
        BuildMesh();
        PlaceTowerAtCenter();
    }

    void BuildMesh()
    {
        if (randomizeSeed) seed = Random.Range(0, 100000);

        mesh = new Mesh();
        mesh.name = "ProceduralTerrainMesh";
        GetComponent<MeshFilter>().mesh = mesh;

        int vertsX = gridSize + 1;
        int vertsZ = gridSize + 1;
        Vector3[] verts = new Vector3[vertsX * vertsZ];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[gridSize * gridSize * 6];

        // Generate vertices & UVs
        int i = 0;
        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                float nx = (x + seed) * noiseScale;
                float nz = (z + seed) * noiseScale;
                float y = Mathf.PerlinNoise(nx, nz) * heightScale;
                verts[i] = new Vector3(x * cellSize, y, z * cellSize);
                uvs[i] = new Vector2((float)x / gridSize, (float)z / gridSize);
                i++;
            }
        }

        // Generate triangles
        int t = 0;
        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                int i0 = z * (gridSize + 1) + x;
                int i1 = i0 + 1;
                int i2 = i0 + (gridSize + 1);
                int i3 = i2 + 1;

                tris[t++] = i0; tris[t++] = i2; tris[t++] = i1;
                tris[t++] = i1; tris[t++] = i2; tris[t++] = i3;
            }
        }

        // Apply to mesh
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        // Update collider
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;

        // Carve paths
        CarvePaths(verts);
    }

    void CarvePaths(Vector3[] verts)
    {
        int vertsPerRow = gridSize + 1;
        spawnPoints = new Vector3[numberOfPaths];

        int cx = gridSize / 2;
        int cz = gridSize / 2;
        Vector3 towerLocal = verts[cz * vertsPerRow + cx];
        Vector3 towerWorld = transform.TransformPoint(towerLocal);

        for (int p = 0; p < numberOfPaths; p++)
        {
            // Pick random edge
            int edge = Random.Range(0, 4);
            int vx = 0, vz = 0;
            if (edge == 0) { vx = 0; vz = Random.Range(0, gridSize); }
            else if (edge == 1) { vx = gridSize; vz = Random.Range(0, gridSize); }
            else if (edge == 2) { vz = 0; vx = Random.Range(0, gridSize); }
            else { vz = gridSize; vx = Random.Range(0, gridSize); }

            // Store initial spawn point
            int spawnIndex = vz * vertsPerRow + vx;
            Vector3 spawnLocal = verts[spawnIndex];
            Vector3 spawnWorld = transform.TransformPoint(spawnLocal);
            spawnPoints[p] = spawnWorld;

            // Carve path toward tower
            int steps = Mathf.Max(30, gridSize);
            for (int s = 0; s <= steps; s++)
            {
                float t = s / (float)steps;
                Vector3 sampleWorld = Vector3.Lerp(spawnWorld, towerWorld, t);

                int ix = Mathf.RoundToInt(sampleWorld.x / cellSize);
                int iz = Mathf.RoundToInt(sampleWorld.z / cellSize);
                if (ix < 0 || iz < 0 || ix >= vertsPerRow || iz >= vertsPerRow) continue;

                int idx = iz * vertsPerRow + ix;
                float targetY = towerLocal.y - 0.1f; // flatten corridor
                verts[idx].y = Mathf.Min(verts[idx].y, targetY);

                // widen path
                int radius = Mathf.CeilToInt(pathWidth);
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        int nx = ix + dx;
                        int nz = iz + dz;
                        if (nx < 0 || nz < 0 || nx >= vertsPerRow || nz >= vertsPerRow) continue;
                        int nIdx = nz * vertsPerRow + nx;
                        verts[nIdx].y = Mathf.Lerp(verts[nIdx].y, targetY, 0.6f);
                    }
                }
            }
        }

        // Snap spawn points to the surface of the carved mesh
        for (int p = 0; p < spawnPoints.Length; p++)
        {
            Vector3 world = spawnPoints[p];
            float localX = world.x / cellSize;
            float localZ = world.z / cellSize;
            int ix = Mathf.RoundToInt(localX);
            int iz = Mathf.RoundToInt(localZ);
            if (ix >= 0 && iz >= 0 && ix < vertsPerRow && iz < vertsPerRow)
            {
                int idx = iz * vertsPerRow + ix;
                Vector3 local = verts[idx];
                spawnPoints[p] = transform.TransformPoint(local);
            }
        }

        // Apply changes to mesh
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    void PlaceTowerAtCenter()
    {
        if (towerPrefab == null || mesh == null) return;

        int vertsPerRow = gridSize + 1;
        int cx = gridSize / 2;
        int cz = gridSize / 2;
        int centerIndex = cz * vertsPerRow + cx;
        Vector3 localCenter = mesh.vertices[centerIndex];
        Vector3 worldCenter = transform.TransformPoint(localCenter);

        if (towerInstance != null) Destroy(towerInstance);
        towerInstance = Instantiate(towerPrefab, worldCenter, Quaternion.identity);
    }

    void OnDrawGizmos()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Vector3 lifted = spawnPoints[i] + Vector3.up * 0.5f; // lift for visibility
                Gizmos.DrawSphere(lifted, 0.25f);
                if (towerInstance) Gizmos.DrawLine(lifted, towerInstance.transform.position + Vector3.up * 0.5f);
            }
        }
    }
}
