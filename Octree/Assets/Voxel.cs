using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Voxel : Enumeration
{
    public static Voxel EMPTY = new EmptyVoxel();
    public static Voxel FILLED = new FilledVoxel();

    static Voxel[] Voxels;

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        Voxels = GetAll<Voxel>().OrderBy(x => x.Id).ToArray();
    }


    public Color color;
    public bool opaque;

    protected Voxel(int id, string name, bool _opaque, Color _color) : base(id, name)
    {
        color = _color;
        opaque = _opaque;
    }

    private class EmptyVoxel : Voxel
    {
        public EmptyVoxel() : base(0, "Empty", false, Color.green) { }
    }

    private class FilledVoxel : Voxel
    {
        public FilledVoxel() : base(1, "Filled", true, Color.grey) { }
    }

    public static explicit operator Voxel(int v)
    {
        //return Voxels[v];

        return Voxels[v];
    }
}

