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

    public Vector3 Noise(Vector3 p)
    {
        const float e = 0.0001f;// 0.0009765625;

        Vector3 dx = new Vector3(e, 0.0f, 0.0f);
        Vector3 dy = new Vector3(0.0f, e, 0.0f);
        Vector3 dz = new Vector3(0.0f, 0.0f, e);

        Vector3 p_x0 = sngen.Noise3D(p - dx);
        Vector3 p_x1 = sngen.Noise3D(p + dx);
        Vector3 p_y0 = sngen.Noise3D(p - dy);
        Vector3 p_y1 = sngen.Noise3D(p + dy);
        Vector3 p_z0 = sngen.Noise3D(p - dz);
        Vector3 p_z1 = sngen.Noise3D(p + dz);

        float x = p_y1.z - p_y0.z - p_z1.y + p_z0.y;
        float y = p_z1.x - p_z0.x - p_x1.z + p_x0.z;
        float z = p_x1.y - p_x0.y - p_y1.x + p_y0.x;

        const float divisor = 1.0f/ (2.0f * e);
        return (new Vector3(x, y, z) * divisor).normalized;
    }
}
