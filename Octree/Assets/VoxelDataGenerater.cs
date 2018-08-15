using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDataGenerater
{
    public List<Planet> planets = new List<Planet>();

    public VoxelDataGenerater()
    {
        planets.Add(new Planet(new Vector3(3, 2, 3), 2f, Perlin(1f, 0.2f)));
        planets.Add(new Belt(  new Vector3(3, 2, 3), 4f, 0.1f, Perlin(2f, 0.7f)));
        planets.Add(new Belt  (new Vector3(3, 2, 3), 6f, 0.1f, Perlin(5f, 0.7f)));


        //planets.Add(new Planet(new Vector3(0, 0, 0), 5f, Perlin(0.5f, 0.5f)));

    }

    Planet.TerrainFeature Perlin(float scale, float floor)
    {
        return delegate (Vector3 p) { return (PerlinNoise.thisPerlin.Perlin(p.x, p.y, p.z, scale) > floor); };
    }

    public Voxel[] GenerateChildData(OctreeNode parent, Vector3[] childPositions)
    {
        Voxel[] data = new Voxel[8];
        if (parent == null)
        {
            throw new System.Exception("Node does not have a parent");
        }

        float halfSize = OctreeNode.getRoot.halfSize;
        for (int i = 0; i < 8; i++)
        {
            data[i] = DataAtPoint(childPositions[i].x, childPositions[i].y, childPositions[i].z);

            //data[i] = (Voxel)((childPositions[i].y < Mathf.Sin(childPositions[i].x) * OctreeNode.getRoot.halfSize) ? 0 : 1);

        }

        return data;
    }

    public Voxel[,,] GenerateData(int size, float scale, Vector3Int startPos)
    {

        Voxel[,,] data = new Voxel[size, size, size]; ;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    data[i, j, k] = DataAtPoint((i + startPos.x) / (float)size * scale, (j + startPos.y) / (float)size * scale, (k + startPos.z) / (float)size * scale);
                }
            }
        }

        return data;
    }

    public Voxel DataAtPoint(float x, float y, float z)
    {
        if (planets.TrueForAll(p => !p.intersects(new Vector3(x, y, z))))
        {
            return Voxel.EMPTY;
        }
        else
        {
            return Voxel.FILLED;
        }
        //return (Voxel)Mathf.RoundToInt(PerlinNoise.Perlin(x, y, z)); ;
    }
}
