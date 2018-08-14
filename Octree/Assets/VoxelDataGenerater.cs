using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDataGenerater
{
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
        if ((new Planet(7, 6, 10, .25f, 1.5f).intersects(x, y, z) || new Belt(7, 4, 7, 4f, 2f, .5f).intersects(x, y, z) || new Planet(7, 4, 7, 3f, 1f).intersects(x, y, z)))
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
