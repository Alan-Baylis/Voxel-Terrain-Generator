using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{

    public static float Perlin(float x, float y, float z, float scale)
    {
        x *= scale;
        y *= scale;
        z *= scale;

        return Mathf.PerlinNoise(x - Mathf.PerlinNoise(z, y), y + Mathf.PerlinNoise(z, x));
        // Phallic terrain return Mathf.PerlinNoise(x, Mathf.PerlinNoise(y, z));
    }
}
