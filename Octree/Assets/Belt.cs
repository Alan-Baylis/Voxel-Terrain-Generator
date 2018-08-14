using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Planet
{
    public float innerRadius;
    public Belt(float _x, float _y, float _z, float _radius, float _perlinScale, float _innerRadius) : base(_x, _y, _z, _radius, _perlinScale)
    {
        innerRadius = _innerRadius;
    }

    public override bool intersects(float px, float py, float pz)
    {
        if (!CheckBounds(px, py, pz))
            return false;

        OffsetPositions(ref px, ref py, ref pz);
        float plyA = radius - Mathf.Sqrt(px * px + pz * pz);

        return (plyA * plyA + py * py) - innerRadius * innerRadius < 0 && Mathf.RoundToInt(PerlinNoise.Perlin(px, py, pz, perlinScale)) > 0;
    }

    protected override bool CheckBounds(float px, float py, float pz)
    {
        float radius = this.radius + innerRadius;
        if (px > x + radius || px < x - radius)
        {
            return false;
        }
        if (py > y + radius || py < y - radius)
        {
            return false;
        }
        if (pz > z + radius || pz < z - radius)
        {
            return false;
        }

        return true;
    }
}
