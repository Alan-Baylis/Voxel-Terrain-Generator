using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet
{
    public float x, y, z;
    public float radius;

    public Planet(float _x, float _y, float _z, float _radius)
    {
        x = _x;
        y = _y;
        z = _z;
        radius = _radius;
    }

    //public bool intersects(Vector3Int p)
    //{
    //    return ((p.x - x) * (p.x - x) + (p.y - y) * (p.y - y) + (p.z - z) * (p.z - z)) - radius * radius > 0;
    //}

    public virtual bool intersects(float px, float py, float pz)
    {
        if (!CheckBounds(px, py, pz))
            return false;
        OffsetPositions(ref px, ref py, ref pz);
        return (px * px + py * py + pz * pz) - radius * radius < 0;
    }

    protected bool CheckBounds(float px, float py, float pz)
    {
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

    protected void OffsetPositions(ref float px, ref float py, ref float pz)
    {
        px -= x;
        py -= y;
        pz -= z;
    }

}
