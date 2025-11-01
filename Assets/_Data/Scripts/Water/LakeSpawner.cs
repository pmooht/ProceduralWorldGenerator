using UnityEngine;
using System.Collections.Generic;

public class LakeSpawner : MonoBehaviour
{

    [Header("Detection Settings")]
    public float waterLevel = 0f;
    public float minLakeSize = 100f; // m²
    public int gridResolution = 10; // Check every N units

    [Header("Lake Prefab")]
    public Material waterMaterial;
    public float lakeDepth = 5f;

    private List<GameObject> spawnedLakes = new List<GameObject>();

    public void GenerateLakes(float[,] heightMapValues, float worldSize, float heightMultiplier)
    {
        Debug.Log("Starting lake generation...");

        // Clear existing
        ClearLakes();

        int width = heightMapValues.GetLength(0);
        int height = heightMapValues.GetLength(1);

        bool[,] visited = new bool[width, height];
        int lakesFound = 0;

        // Scan for water areas
        for (int y = 0; y < height; y += gridResolution)
        {
            for (int x = 0; x < width; x += gridResolution)
            {

                if (visited[x, y]) continue;

                float terrainHeight = heightMapValues[x, y];

                // Check if below water level
                if (terrainHeight < waterLevel)
                {

                    // Flood fill to find lake extent
                    List<Vector2Int> lakePixels = FloodFill(
                        heightMapValues,
                        visited,
                        x, y,
                        waterLevel,
                        width, height
                    );

                    // Calculate area
                    float pixelSize = worldSize / width;
                    float lakeArea = lakePixels.Count * pixelSize * pixelSize;

                    if (lakeArea >= minLakeSize)
                    {
                        SpawnLake(lakePixels, heightMapValues, worldSize, width, height);
                        lakesFound++;
                    }
                }
            }
        }

        Debug.Log($"Lake generation complete: {lakesFound} lakes spawned");
    }

    List<Vector2Int> FloodFill(float[,] heightMap, bool[,] visited, int startX, int startY,
                                float threshold, int width, int height)
    {

        List<Vector2Int> result = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0 && result.Count < 10000)
        { // Safety limit
            Vector2Int pos = queue.Dequeue();
            result.Add(pos);

            // Check 4 neighbors
            int[] dx = { -gridResolution, gridResolution, 0, 0 };
            int[] dy = { 0, 0, -gridResolution, gridResolution };

            for (int i = 0; i < 4; i++)
            {
                int nx = pos.x + dx[i];
                int ny = pos.y + dy[i];

                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (visited[nx, ny]) continue;

                if (heightMap[nx, ny] < threshold)
                {
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return result;
    }

    void SpawnLake(List<Vector2Int> lakePixels, float[,] heightMap, float worldSize,
                   int mapWidth, int mapHeight)
    {

        // Calculate bounding box
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (Vector2Int pixel in lakePixels)
        {
            float worldX = (pixel.x / (float)mapWidth - 0.5f) * worldSize;
            float worldZ = (pixel.y / (float)mapHeight - 0.5f) * worldSize;

            if (worldX < minX) minX = worldX;
            if (worldX > maxX) maxX = worldX;
            if (worldZ < minZ) minZ = worldZ;
            if (worldZ > maxZ) maxZ = worldZ;
        }

        Vector3 center = new Vector3(
            (minX + maxX) / 2f,
            waterLevel,
            (minZ + maxZ) / 2f
        );

        Vector2 size = new Vector2(
            maxX - minX,
            maxZ - minZ
        );

        // Create lake GameObject
        GameObject lake = new GameObject($"Lake_{spawnedLakes.Count + 1}");
        lake.transform.position = center;
        lake.transform.parent = transform;
        lake.layer = LayerMask.NameToLayer("Water"); // Optional: set layer

        // Add WaterLake component
        WaterLake waterLake = lake.AddComponent<WaterLake>();
        waterLake.waterLevel = waterLevel;
        waterLake.lakeSize = size;
        waterLake.depth = lakeDepth;
        waterLake.waterMaterial = waterMaterial;
        waterLake.animateWaves = true;
        waterLake.waveSpeed = 0.3f;
        waterLake.waveHeight = 0.05f;
        waterLake.buoyancyForce = 10f;
        waterLake.waterDrag = 5f;

        spawnedLakes.Add(lake);

        Debug.Log($"Spawned lake at {center} with size {size}");
    }

    public void ClearLakes()
    {
        foreach (GameObject lake in spawnedLakes)
        {
            if (lake != null)
            {
                DestroyImmediate(lake);
            }
        }
        spawnedLakes.Clear();
    }
}