using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Voxel : Enumeration
{
    public static Voxel EMPTY = new EmptyVoxel();
    public static Voxel FILLED = new FilledVoxel();
    public static Voxel SNOW = new SnowVoxel();
    public static Voxel REDSAND = new RedSandVoxel();
    public static Voxel DARKGRASS = new DatkGrassVoxel();


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
        public EmptyVoxel() : base(0, "Empty", false, Color.grey) { }
    }

    private class FilledVoxel : Voxel
    {
        public FilledVoxel() : base(1, "Filled", true, Color.green) { }
    }

    private class SnowVoxel : Voxel
    {
        public SnowVoxel() : base(2, "Snow", true, Color.white) { }
    }

    private class RedSandVoxel : Voxel
    {
        public RedSandVoxel() : base(3, "Red Sand", true, ColorFromHTML("#9b2900")) { }
    }

    private class DatkGrassVoxel : Voxel
    {
        public DatkGrassVoxel() : base(4, "Dark Grass", true, ColorFromHTML("#415b2d")) { }
    }

    public static Color ColorFromHTML(string html)
    {
        Color c;
        if(ColorUtility.TryParseHtmlString(html, out c))
        {
            return c;
        }
        else
        {
            return Color.black;
        }
        
    }

    public static explicit operator Voxel(int v)
    {
        //return Voxels[v];

        return Voxels[v];
    }
}

