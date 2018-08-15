using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Planet
{
    public float minorRadius;

    public Belt(Vector3 pos, float _majorRadius, float _minorRadius) : base(pos, _majorRadius)
    {
        minorRadius = _minorRadius;
    }

    public Belt(Vector3 pos, float _majorRadius, float _minorRadius, TerrainFeature t) : base(pos, _majorRadius)
    {
        minorRadius = _minorRadius;
        terrainFeatures = t;
    }

    public override bool intersects(Vector3 p)
    {
        if (!CheckBounds(p))
            return false;

        OffsetPositions(ref p);
        float plyA = radius - Mathf.Sqrt(p.x * p.x + p.z * p.z);

        return (plyA * plyA + p.y * p.y) - minorRadius * minorRadius < 0 && terrainFeatures(p);
    }

    protected override bool CheckBounds(Vector3 p)
    {
        float radius = this.radius + minorRadius;
        if (p.x > position.x + radius || p.x < position.x - radius)
        {
            return false;
        }
        if (p.y > position.y + radius || p.y < position.y - radius)
        {
            return false;
        }
        if (p.z > position.z + radius || p.z < position.z - radius)
        {
            return false;
        }

        return true;
    }
}
