using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDataGenerater
{

    static VoxelDataGenerater _vdm;

    public static VoxelDataGenerater vdm
    {
        get
        {
            if (_vdm == null)
            {
                _vdm = new VoxelDataGenerater();
            }
            return _vdm;
        }
    }

    public Voxel[] GenerateChildData(OctreeNode parent)
    {
        Voxel[] data = new Voxel[8];
        int passParentData = 4;
        if (parent == null)
        {
            throw new System.Exception("Node does not have a parent");
        }
        for (int i = 0; i < passParentData; i++)
        {
            data[Random.Range(0, passParentData)] = parent.data;
        }

        for (int i = 0; i < 8; i++)
        {

            data[i] = (Voxel)Random.Range(0, System.Enum.GetNames(typeof(Voxel)).Length);
            //Debug.Log(data[i]);
        }

        return data;
    }
}
