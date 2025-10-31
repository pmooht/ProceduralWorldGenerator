using UnityEngine;

public class StaticIslandGenerator : MonoBehaviour
{

    [Header("Island Settings")]
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;
    public Material terrainMaterial;

    [Header("Island Generation")]
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int islandLOD = 0;
    public bool generateOnStart = true;

    [Header("Water Settings")]
    public bool generateWaterLakes = true;
    public Material lakeMaterial;
    public float waterLevel = 0f;
    public float minLakeSize = 100f; // m²

    [Header("Ocean")]
    public bool generateOcean = true;
    public Material oceanMaterial;
    public float oceanSize = 2000f;

    [Header("References")]
    public Transform islandParent;

    // Private
    private GameObject islandObject;
    private GameObject oceanObject;
    private LakeSpawner lakeSpawner;
    private HeightMap cachedHeightMap;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateIsland();
        }
    }

    public void GenerateIsland()
    {
        Debug.Log("=== Starting Island Generation ===");

        // 1. Clear existing
        ClearIsland();

        // 2. Validate settings
        if (!ValidateSettings())
        {
            Debug.LogError("Invalid settings! Please assign all required fields.");
            return;
        }

        // 3. Generate terrain
        GenerateTerrain();

        // 4. Generate water lakes
        if (generateWaterLakes)
        {
            GenerateLakes();
        }

        // 5. Generate ocean
        if (generateOcean)
        {
            GenerateOceanPlane();
        }

        Debug.Log("=== Island Generation Complete ===");
    }

    void GenerateTerrain()
    {
        Debug.Log("Generating terrain mesh...");

        // Create island GameObject
        islandObject = new GameObject("Island Terrain");
        islandObject.transform.parent = islandParent != null ? islandParent : transform;
        islandObject.transform.position = Vector3.zero;

        // Add components
        MeshFilter meshFilter = islandObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = islandObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = islandObject.AddComponent<MeshCollider>();

        // Apply material
        meshRenderer.material = terrainMaterial;

        // Generate heightmap
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        cachedHeightMap = HeightMapGenerator.GenerateHeightMap(
            meshSettings.numVertsPerLine,
            meshSettings.numVertsPerLine,
            heightMapSettings,
            Vector2.zero
        );

        // Generate mesh
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(
            cachedHeightMap.values,
            meshSettings,
            islandLOD
        );

        // Apply mesh
        Mesh mesh = meshData.CreateMesh();
        mesh.name = "Island Mesh";
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        Debug.Log($"Terrain generated: {mesh.vertexCount} vertices");
    }

    void GenerateLakes()
    {
        Debug.Log("Generating water lakes...");

        if (cachedHeightMap.values == null)
        {
            Debug.LogWarning("No heightmap available for lake generation!");
            return;
        }

        // Create or get LakeSpawner
        GameObject spawnerObj = GameObject.Find("Lake Spawner");
        if (spawnerObj == null)
        {
            spawnerObj = new GameObject("Lake Spawner");
            spawnerObj.transform.parent = transform;
        }

        lakeSpawner = spawnerObj.GetComponent<LakeSpawner>();
        if (lakeSpawner == null)
        {
            lakeSpawner = spawnerObj.AddComponent<LakeSpawner>();
        }

        // Configure spawner
        lakeSpawner.waterMaterial = lakeMaterial;
        lakeSpawner.waterLevel = waterLevel;
        lakeSpawner.minLakeSize = minLakeSize;
        lakeSpawner.gridResolution = 10;

        // Generate lakes
        lakeSpawner.GenerateLakes(
            cachedHeightMap.values,
            meshSettings.meshWorldSize,
            heightMapSettings.heightMultiplier
        );
    }

    void GenerateOceanPlane()
    {
        Debug.Log("Generating ocean plane...");

        // Clear existing ocean
        if (oceanObject != null)
        {
            DestroyImmediate(oceanObject);
        }

        oceanObject = new GameObject("Ocean");
        oceanObject.transform.parent = transform;
        oceanObject.transform.position = new Vector3(0, waterLevel, 0);

        // Add components
        MeshFilter meshFilter = oceanObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = oceanObject.AddComponent<MeshRenderer>();

        // Create ocean mesh (large quad)
        Mesh mesh = new Mesh();
        mesh.name = "Ocean Mesh";

        float halfSize = oceanSize / 2f;
        Vector3[] vertices = new Vector3[4] {
            new Vector3(-halfSize, 0, -halfSize),
            new Vector3(halfSize, 0, -halfSize),
            new Vector3(-halfSize, 0, halfSize),
            new Vector3(halfSize, 0, halfSize)
        };

        int[] triangles = new int[6] {
            0, 2, 1,
            2, 3, 1
        };

        Vector2[] uvs = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshRenderer.material = oceanMaterial;

        Debug.Log("Ocean plane generated");
    }

    void ClearIsland()
    {
        if (islandObject != null)
        {
            DestroyImmediate(islandObject);
        }
        if (oceanObject != null)
        {
            DestroyImmediate(oceanObject);
        }
        if (lakeSpawner != null)
        {
            lakeSpawner.ClearLakes();
        }
    }

    bool ValidateSettings()
    {
        if (meshSettings == null)
        {
            Debug.LogError("Mesh Settings is null!");
            return false;
        }
        if (heightMapSettings == null)
        {
            Debug.LogError("Height Map Settings is null!");
            return false;
        }
        if (textureData == null)
        {
            Debug.LogError("Texture Data is null!");
            return false;
        }
        if (terrainMaterial == null)
        {
            Debug.LogError("Terrain Material is null!");
            return false;
        }
        return true;
    }

    void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnSettingsUpdated;
            meshSettings.OnValuesUpdated += OnSettingsUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnSettingsUpdated;
            heightMapSettings.OnValuesUpdated += OnSettingsUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureUpdated;
            textureData.OnValuesUpdated += OnTextureUpdated;
        }
    }

    void OnSettingsUpdated()
    {
        // Auto regenerate khi settings thay đổi (chỉ trong Editor)
        if (!Application.isPlaying && islandObject != null)
        {
            GenerateIsland();
        }
    }

    void OnTextureUpdated()
    {
        if (terrainMaterial != null && textureData != null)
        {
            textureData.ApplyToMaterial(terrainMaterial);
        }
    }
}