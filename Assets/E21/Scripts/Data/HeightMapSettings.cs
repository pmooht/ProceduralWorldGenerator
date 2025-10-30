using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{

    public NoiseSettings noiseSettings;

    [Header("Falloff Settings")]
    public bool useFalloff;

    [Range(1f, 10f)]
    [Tooltip("Độ dốc của rìa đảo. Cao hơn = rìa dốc hơn")]
    public float falloffStrength = 3f;

    [Range(0.5f, 5f)]
    [Tooltip("Kích thước đảo. Cao hơn = đảo nhỏ hơn")]
    public float falloffSize = 2.2f;

    [Range(0f, 1f)]
    [Tooltip("Độ mạnh của falloff effect (0 = không có effect, 1 = max effect)")]
    public float falloffIntensity = 1f;

    [Header("Height Settings")]
    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();

        // Validate falloff values
        falloffStrength = Mathf.Max(falloffStrength, 1f);
        falloffSize = Mathf.Max(falloffSize, 0.5f);
        falloffIntensity = Mathf.Clamp01(falloffIntensity);

        base.OnValidate();
    }
#endif
}