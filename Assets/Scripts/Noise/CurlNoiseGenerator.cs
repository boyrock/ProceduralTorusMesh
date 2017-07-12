using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CurlNoiseGenerator
{
    SimplexNoiseGenerator sngen;
    public CurlNoiseGenerator()
    {
        sngen = new SimplexNoiseGenerator();
    }

    public Vector2 Noise2D(Vector2 p)
    {
        return Noise2D(p.x, p.y);
    }
    public Vector2 Noise2D(float x, float y)
    {
        float eps = 0.1f;
        float n1, n2, a, b;

        n1 = Mathf.PerlinNoise(x, y + eps);
        n2 = Mathf.PerlinNoise(x, y - eps);
        a = (n1 - n2) / (2 * eps);
        n1 = Mathf.PerlinNoise(x + eps, y);
        n2 = Mathf.PerlinNoise(x - eps, y);
        b = (n1 - n2) / (2 * eps);

        Vector2 curl = new Vector2(a, -b);
        return curl;

    }
    //public Vector3 Noise3D(Vector3 p)
    //{
    //    return Noise3D(p.x, p.y, p.z);
    //}
    Vector3 Noise3D(float x, float y, float z)
    {
        float eps = 0.001f;
        float n1, n2, a, b;
        Vector3 curl;
        n1 = Perlin.Noise(x, y + eps, z);
        n2 = Perlin.Noise(x, y - eps, z);
        a = (n1 - n2) / (2 * eps);

        n1 = Perlin.Noise(x, y, z + eps);
        n2 = Perlin.Noise(x, y, z - eps);
        b = (n1 - n2) / (2 * eps);

        curl.x = a - b;

        n1 = Perlin.Noise(x, y, z + eps);
        n2 = Perlin.Noise(x, y, z - eps);
        a = (n1 - n2) / (2 * eps);

        n1 = Perlin.Noise(x + eps, y, z);
        n2 = Perlin.Noise(x + eps, y, z);
        b = (n1 - n2) / (2 * eps);

        curl.y = a - b;
        n1 = Perlin.Noise(x + eps, y, z);
        n2 = Perlin.Noise(x - eps, y, z);
        a = (n1 - n2) / (2 * eps);

        n1 = Perlin.Noise(x, y + eps, z);
        n2 = Perlin.Noise(x, y - eps, z);
        b = (n1 - n2) / (2 * eps);

        curl.z = a - b;

        return curl;
    }

    public Vector3 Noise3D(Vector3 p)
    {
        const float e = 0.01f;// 0.0009765625;

        Vector3 dx = new Vector3(e, 0.0f, 0.0f);
        Vector3 dy = new Vector3(0.0f, e, 0.0f);
        Vector3 dz = new Vector3(0.0f, 0.0f, e);

        Vector3 p_x0 = Perlin.Noise3D(p - dx);
        Vector3 p_x1 = Perlin.Noise3D(p + dx);
        Vector3 p_y0 = Perlin.Noise3D(p - dy);
        Vector3 p_y1 = Perlin.Noise3D(p + dy);
        Vector3 p_z0 = Perlin.Noise3D(p - dz);
        Vector3 p_z1 = Perlin.Noise3D(p + dz);

        //Vector3 p_x0 = sngen.Noise3D(p - dx);
        //Vector3 p_x1 = sngen.Noise3D(p + dx);
        //Vector3 p_y0 = sngen.Noise3D(p - dy);
        //Vector3 p_y1 = sngen.Noise3D(p + dy);
        //Vector3 p_z0 = sngen.Noise3D(p - dz);
        //Vector3 p_z1 = sngen.Noise3D(p + dz);

        float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
        float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
        float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

        //const float divisor = 1.0f/ (2.0f * e);
        return (new Vector3(x, y, z) / (2.0f * e)).normalized;
    }
}
