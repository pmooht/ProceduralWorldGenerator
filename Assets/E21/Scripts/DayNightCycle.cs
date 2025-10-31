using UnityEngine;

public class DayNightCycle : MonoBehaviour
{

    [Header("Time Settings")]
    [Tooltip("Độ dài 1 ngày (giây thực)")]
    [Range(60f, 1200f)]
    public float dayDurationInSeconds = 600f; // 10 phút

    [Tooltip("Giờ bắt đầu (0-24)")]
    [Range(0f, 24f)]
    public float startTime = 8f;

    [Tooltip("Tốc độ thời gian")]
    [Range(0f, 10f)]
    public float timeScale = 1f;

    [Header("Lights")]
    public Light sunLight;
    public Light moonLight;

    [Header("Sun Settings")]
    public Gradient sunColorGradient;
    public AnimationCurve sunIntensityCurve;

    [Header("Moon Settings")]
    public Gradient moonColorGradient;
    public AnimationCurve moonIntensityCurve;

    [Header("Ambient")]
    public Gradient ambientColorGradient;
    public AnimationCurve ambientIntensityCurve;

    [Header("Fog")]
    public bool useFog = true;
    public Gradient fogColorGradient;
    public AnimationCurve fogDensityCurve;

    [Header("Skybox")]
    public Material skyboxMaterial;
    public Gradient skyTintGradient;

    [Header("Debug")]
    public bool showTimeInfo = true;

    private float currentTime;
    private float timeOfDay; // 0-1
    private int currentDay = 1;

    void Start()
    {
        currentTime = startTime;
        InitializeGradients();
        UpdateLighting();
    }

    void InitializeGradients()
    {
        // Default gradients if not set
        if (sunColorGradient.colorKeys.Length == 0)
        {
            sunColorGradient = CreateDefaultSunGradient();
        }
        if (moonColorGradient.colorKeys.Length == 0)
        {
            moonColorGradient = CreateDefaultMoonGradient();
        }
        if (ambientColorGradient.colorKeys.Length == 0)
        {
            ambientColorGradient = CreateDefaultAmbientGradient();
        }
        if (fogColorGradient.colorKeys.Length == 0)
        {
            fogColorGradient = CreateDefaultFogGradient();
        }
    }

    Gradient CreateDefaultSunGradient()
    {
        Gradient g = new Gradient();
        GradientColorKey[] colors = new GradientColorKey[7];
        colors[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.24f), 0f);    // Midnight
        colors[1] = new GradientColorKey(new Color(1f, 0.42f, 0.21f), 0.2f);   // Dawn
        colors[2] = new GradientColorKey(new Color(1f, 0.97f, 0.86f), 0.3f);   // Morning
        colors[3] = new GradientColorKey(new Color(1f, 1f, 1f), 0.5f);         // Noon
        colors[4] = new GradientColorKey(new Color(1f, 0.97f, 0.86f), 0.7f);   // Afternoon
        colors[5] = new GradientColorKey(new Color(1f, 0.42f, 0.21f), 0.8f);   // Dusk
        colors[6] = new GradientColorKey(new Color(0.1f, 0.1f, 0.24f), 1f);    // Night

        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1f, 0f);
        alphas[1] = new GradientAlphaKey(1f, 1f);

        g.SetKeys(colors, alphas);
        return g;
    }

    Gradient CreateDefaultMoonGradient()
    {
        Gradient g = new Gradient();
        GradientColorKey[] colors = new GradientColorKey[3];
        colors[0] = new GradientColorKey(new Color(0.69f, 0.77f, 0.87f), 0f);
        colors[1] = new GradientColorKey(new Color(0.4f, 0.45f, 0.55f), 0.5f);
        colors[2] = new GradientColorKey(new Color(0.69f, 0.77f, 0.87f), 1f);

        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1f, 0f);
        alphas[1] = new GradientAlphaKey(1f, 1f);

        g.SetKeys(colors, alphas);
        return g;
    }

    Gradient CreateDefaultAmbientGradient()
    {
        Gradient g = new Gradient();
        GradientColorKey[] colors = new GradientColorKey[5];
        colors[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.24f), 0f);
        colors[1] = new GradientColorKey(new Color(1f, 0.63f, 0.47f), 0.25f);
        colors[2] = new GradientColorKey(new Color(1f, 1f, 1f), 0.5f);
        colors[3] = new GradientColorKey(new Color(1f, 0.71f, 0.76f), 0.75f);
        colors[4] = new GradientColorKey(new Color(0.1f, 0.1f, 0.24f), 1f);

        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1f, 0f);
        alphas[1] = new GradientAlphaKey(1f, 1f);

        g.SetKeys(colors, alphas);
        return g;
    }

    Gradient CreateDefaultFogGradient()
    {
        Gradient g = new Gradient();
        GradientColorKey[] colors = new GradientColorKey[5];
        colors[0] = new GradientColorKey(new Color(0.04f, 0.04f, 0.12f), 0f);
        colors[1] = new GradientColorKey(new Color(1f, 0.75f, 0.79f), 0.25f);
        colors[2] = new GradientColorKey(new Color(0.88f, 0.96f, 1f), 0.5f);
        colors[3] = new GradientColorKey(new Color(1f, 0.71f, 0.76f), 0.75f);
        colors[4] = new GradientColorKey(new Color(0.04f, 0.04f, 0.12f), 1f);

        GradientAlphaKey[] alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1f, 0f);
        alphas[1] = new GradientAlphaKey(1f, 1f);

        g.SetKeys(colors, alphas);
        return g;
    }

    void Update()
    {
        // Update time
        float timeIncrement = (24f / dayDurationInSeconds) * Time.deltaTime * timeScale;
        currentTime += timeIncrement;

        if (currentTime >= 24f)
        {
            currentTime = 0f;
            currentDay++;
            OnNewDay();
        }

        timeOfDay = currentTime / 24f;
        UpdateLighting();
    }

    void UpdateLighting()
    {
        // Sun rotation and properties
        if (sunLight != null)
        {
            float sunAngle = timeOfDay * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
            sunLight.color = sunColorGradient.Evaluate(timeOfDay);
            sunLight.intensity = sunIntensityCurve.Evaluate(timeOfDay);
        }

        // Moon rotation and properties
        if (moonLight != null)
        {
            float moonAngle = timeOfDay * 360f + 90f;
            moonLight.transform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);
            moonLight.color = moonColorGradient.Evaluate(timeOfDay);
            moonLight.intensity = moonIntensityCurve.Evaluate(timeOfDay);
        }

        // Ambient lighting
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(timeOfDay);
        RenderSettings.ambientIntensity = ambientIntensityCurve.Evaluate(timeOfDay);

        // Fog
        if (useFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColorGradient.Evaluate(timeOfDay);
            RenderSettings.fogDensity = fogDensityCurve.Evaluate(timeOfDay);
        }

        // Skybox
        if (skyboxMaterial != null && skyTintGradient.colorKeys.Length > 0)
        {
            skyboxMaterial.SetColor("_SkyTint", skyTintGradient.Evaluate(timeOfDay));
        }
    }

    void OnGUI()
    {
        if (!showTimeInfo) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.5f));

        string timeString = GetTimeString();
        string periodString = GetPeriodString();

        GUI.BeginGroup(new Rect(10, 10, 250, 100));
        GUI.Box(new Rect(0, 0, 250, 100), "", style);
        GUI.Label(new Rect(10, 10, 230, 30), $"Day {currentDay}", style);
        GUI.Label(new Rect(10, 40, 230, 30), $"Time: {timeString}", style);
        GUI.Label(new Rect(10, 70, 230, 30), $"{periodString}", style);
        GUI.EndGroup();
    }

    Texture2D MakeTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        return string.Format("{0:00}:{1:00}", hours, minutes);
    }

    string GetPeriodString()
    {
        if (currentTime >= 5f && currentTime < 7f) return "🌅 Dawn";
        if (currentTime >= 7f && currentTime < 17f) return "☀️ Day";
        if (currentTime >= 17f && currentTime < 19f) return "🌇 Dusk";
        if (currentTime >= 19f || currentTime < 5f) return "🌙 Night";
        return "Unknown";
    }

    void OnNewDay()
    {
        Debug.Log($"=== Day {currentDay} Started ===");
    }

    // Public API
    public void SetTime(float hours)
    {
        currentTime = Mathf.Clamp(hours, 0f, 24f);
        timeOfDay = currentTime / 24f;
        UpdateLighting();
    }

    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(scale, 0f);
    }

    public float GetCurrentHour()
    {
        return currentTime;
    }

    public bool IsDay()
    {
        return currentTime >= 6f && currentTime < 18f;
    }

    public bool IsNight()
    {
        return !IsDay();
    }
}