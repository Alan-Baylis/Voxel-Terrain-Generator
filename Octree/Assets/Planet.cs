using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet
{
    public Vector3 position;
    public float radius;
    public delegate bool TerrainFeature(Vector3 pos);
    public TerrainFeature terrainFeatures = delegate (Vector3 pos) { return false; };

    public Planet(Vector3 pos, float _radius)
    {
        Setup(pos, _radius);
    }

    public Planet(Vector3 pos, float _radius, TerrainFeature t)
    {
        Setup(pos, _radius);
        terrainFeatures = t;
    }

    void Setup(Vector3 pos, float _radius)
    {
        position = pos;
        radius = _radius;
    }

    //public bool intersects(Vector3Int p)
    //{
    //    return ((p.x - x) * (p.x - x) + (p.y - y) * (p.y - y) + (p.z - z) * (p.z - z)) - radius * radius > 0;
    //}

    public virtual bool intersects(Vector3 p)
    {
        if (!CheckBounds(p))
            return false;
        OffsetPositions(ref p);
        return (p.x * p.x + p.y * p.y + p.z * p.z) - radius * radius < 0 && terrainFeatures(p);
    }

    protected virtual bool CheckBounds(Vector3 p)
    {
        if (p.x >position.x + radius || p.x <position.x - radius)
        {
            return false;
        }
        if (p.y >position.y + radius || p.y <position.y - radius)
        {
            return false;
        }
        if (p.z >position.z + radius || p.z <position.z - radius)
        {
            return false;
        }

        return true;
    }

    protected void OffsetPositions(ref Vector3 p)
    {
        p -= position;
    }

}
