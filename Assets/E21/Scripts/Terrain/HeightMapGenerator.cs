using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{

    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        // Generate noise map
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

        // Generate falloff map nếu enabled
        float[,] falloffMap = null;
        if (settings.useFalloff)
        {
            falloffMap = FalloffGenerator.GenerateCircularFalloffMap(
                width,
                settings.falloffStrength,
                settings.falloffSize
            );
        }

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Apply falloff trước khi apply height curve
                if (settings.useFalloff)
                {
                    // Blend giữa noise value và falloff
                    float falloffValue = falloffMap[i, j];
                    values[i, j] = Mathf.Clamp01(values[i, j] - falloffValue * settings.falloffIntensity);
                }

                // Apply height curve và multiplier
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }
}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}