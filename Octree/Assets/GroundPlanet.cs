using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPlanet : Planet
{

    public GroundPlanet(Vector3 pos, float _radius, TerrainFeature t) : base(pos, _radius, t)
    {


    }

    int depth = 5;
    public override bool intersects(Vector3 p)
    {

        float perlinHeight = VoxelManager.vm.worldSize / 2f;
        for (int i = 1; i < depth; i++)
        {
            perlinHeight += PerlinNoise.thisPerlin.Perlin(p.x, p.y, p.z, 1f / i) * i;
        }
        return p.y > perlinHeight;
    }

}
