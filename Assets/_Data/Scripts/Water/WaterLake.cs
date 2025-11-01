using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterLake : MonoBehaviour
{

    [Header("Lake Properties")]
    public float waterLevel = 0f;
    public Vector2 lakeSize = new Vector2(50, 50);
    public float depth = 5f;

    [Header("Visual")]
    public Material waterMaterial;

    [Header("Wave Animation")]
    public bool animateWaves = true;
    public float waveSpeed = 0.5f;
    public float waveHeight = 0.1f;
    public float waveFrequency = 5f;

    [Header("Physics")]
    public float waterDrag = 5f;
    public float buoyancyForce = 10f;

    [Header("Effects")]
    public bool playSplashSound = true;
    public AudioClip splashSound;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private BoxCollider waterCollider;
    private AudioSource audioSource;

    void Start()
    {
        SetupComponents();
        SetupWaterMesh();
        SetupCollider();

        if (playSplashSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = splashSound;
            audioSource.playOnAwake = false;
        }
    }

    void SetupComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }

    void SetupWaterMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Water Lake Mesh";

        // Simple quad
        float halfX = lakeSize.x / 2f;
        float halfZ = lakeSize.y / 2f;

        Vector3[] vertices = new Vector3[4] {
            new Vector3(-halfX, 0, -halfZ),
            new Vector3(halfX, 0, -halfZ),
            new Vector3(-halfX, 0, halfZ),
            new Vector3(halfX, 0, halfZ)
        };

        int[] triangles = new int[6] {
            0, 2, 1,
            2, 3, 1
        };

        Vector2[] uvs = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(lakeSize.x / 10f, 0),
            new Vector2(0, lakeSize.y / 10f),
            new Vector2(lakeSize.x / 10f, lakeSize.y / 10f)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        if (waterMaterial != null)
        {
            meshRenderer.material = waterMaterial;
        }
    }

    void SetupCollider()
    {
        waterCollider = GetComponent<BoxCollider>();
        if (waterCollider == null)
        {
            waterCollider = gameObject.AddComponent<BoxCollider>();
        }

        waterCollider.isTrigger = true;
        waterCollider.size = new Vector3(lakeSize.x, depth, lakeSize.y);
        waterCollider.center = new Vector3(0, -depth / 2f, 0);
    }

    void Update()
    {
        if (animateWaves && waterMaterial != null)
        {
            // Pass time to shader for wave animation
            waterMaterial.SetFloat("_Time", Time.time * waveSpeed);
            waterMaterial.SetFloat("_WaveHeight", waveHeight);
            waterMaterial.SetFloat("_WaveFrequency", waveFrequency);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Player enters water
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered lake at {transform.position}");

            // Play splash sound
            if (playSplashSound && audioSource != null && splashSound != null)
            {
                audioSource.Play();
            }

            // TODO: Enable swim mode
            // PlayerController player = other.GetComponent<PlayerController>();
            // if (player != null) player.EnterWater(waterDrag);
        }

        // Zombie enters water
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy entered water - applying slow");
            // TODO: Slow down zombie
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited water");
            // TODO: Disable swim mode
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Apply buoyancy
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float waterSurface = transform.position.y + waterLevel;

            if (other.transform.position.y < waterSurface)
            {
                // Apply upward force
                rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Acceleration);

                // Apply drag
                rb.linearDamping = waterDrag;
            }
        }
    }

    void OnDrawGizmos()
    {
        // Visualize lake in editor
        Gizmos.color = new Color(0.2f, 0.5f, 0.8f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(lakeSize.x, 0.1f, lakeSize.y));

        // Draw water level
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * waterLevel,
            new Vector3(lakeSize.x, 0.1f, lakeSize.y)
        );
    }
}