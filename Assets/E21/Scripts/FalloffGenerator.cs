using UnityEngine;
using System.Collections;

public static class FalloffGenerator
{

    // Version cơ bản (giữ nguyên cho backward compatibility)
    public static float[,] GenerateFalloffMap(int size)
    {
        return GenerateFalloffMap(size, 3f, 2.2f);
    }

    // Version mới với parameters tùy chỉnh
    public static float[,] GenerateFalloffMap(int size, float strength, float falloffSize)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value, strength, falloffSize);
            }
        }

        return map;
    }

    // Circular falloff (tạo đảo hình tròn thay vì vuông)
    public static float[,] GenerateCircularFalloffMap(int size, float strength, float falloffSize)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                // Sử dụng distance từ center (tạo hình tròn)
                float value = Mathf.Sqrt(x * x + y * y);
                map[i, j] = Evaluate(value, strength, falloffSize);
            }
        }

        return map;
    }

    static float Evaluate(float value, float a, float b)
    {
        // a = strength (độ dốc)
        // b = size (kích thước đảo)
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}