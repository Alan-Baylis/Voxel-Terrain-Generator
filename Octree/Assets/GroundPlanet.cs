using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPlanet : Planet
{

    public GroundPlanet(Vector3 pos, float _radius, TerrainFeature t) : base(pos, _radius, t)
    {


    }

    int depth = 4;
    public override Voxel VoxelAt(Vector3 p)
    {
        float perlinHeight = VoxelManager.vm.worldSize.y / 1.5f + PerlinNoise.thisPerlin.Perlin(p.x, 0, p.z, 0.1f) * 10;
        for (int i = 1; i < depth; i++)
        {
            perlinHeight -= PerlinNoise.thisPerlin.Perlin(p.x, p.y, p.z, 1f / i) * i;
        }

        if (p.y < perlinHeight)
        {
            if (perlinHeight > 10)
                return Voxel.SNOW;
            else if (perlinHeight > 5)
                return Voxel.DARKGRASS;
            else return Voxel.FILLED;

        }
        else
        {
            return Voxel.EMPTY;

        }


    }

}
