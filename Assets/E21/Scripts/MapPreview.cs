using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public enum DrawMode { NoiseMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;
    public Material terrainMaterial;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;
    public bool autoUpdate;

    public void DrawMapInEditor()
    {
        if (textureData == null || terrainMaterial == null || heightMapSettings == null || meshSettings == null)
        {
            Debug.LogWarning("MapPreview: Missing references. Please assign all required fields.");
            return;
        }

        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

        if (drawMode == DrawMode.NoiseMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine), 0, 1)));
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        // NULL CHECK
        if (textureRender == null || meshFilter == null)
        {
            Debug.LogWarning("MapPreview: textureRender or meshFilter is null!");
            return;
        }

        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

        textureRender.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        // NULL CHECK
        if (meshFilter == null || textureRender == null)
        {
            Debug.LogWarning("MapPreview: meshFilter or textureRender is null!");
            return;
        }

        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRender.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    void OnValuesUpdated()
    {
        // Chỉ update trong Editor, không phải Play Mode
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        if (textureData != null && terrainMaterial != null)
        {
            textureData.ApplyToMaterial(terrainMaterial);
        }
    }

    void OnValidate()
    {
        // UNSUBSCRIBE trước khi SUBSCRIBE để tránh duplicate
        UnsubscribeFromEvents();
        SubscribeToEvents();
    }

    void OnEnable()
    {
        SubscribeToEvents();
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    void SubscribeToEvents()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    void UnsubscribeFromEvents()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
        }
    }
}