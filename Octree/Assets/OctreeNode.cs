using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Voxel
{
    FILLED,
    EMPTY
}

public class OctreeNode
{

    static OctreeNode _root;
    public static OctreeNode getRoot
    {
        get
        {
            if (_root == null)
            {
                _root = new OctreeNode(null, Vector3.zero, 1000f, Voxel.FILLED);
            }

            return _root;
        }
    }

    public OctreeNode parent;
    public Voxel data;

    OctreeNode[] _children = new OctreeNode[8];
    public OctreeNode[] children
    {
        get
        {
            return _children;
        }
    }

    public bool isLeaf()
    {
        return ReferenceEquals(children[0], null);
    }

    public bool isRoot()
    {
        return this == _root;
    }

    public float halfSize;
    public Vector3 pos { get; private set; }

    GameObject GO;
    LineRenderer ren;
    public bool debug = true;

    [RuntimeInitializeOnLoadMethod]
    static bool Init()
    {

        return getRoot == null;

    }

    public OctreeNode(OctreeNode parent, Vector3 pos, float halfSize, Voxel _data)
    {
        this.parent = parent;
        this.pos = pos;
        this.halfSize = halfSize;
        data = _data;
        if (debug)
        {
            GO = new GameObject();
            GO.hideFlags = HideFlags.HideInHierarchy;
            ren = GO.AddComponent<LineRenderer>();
            Visualize();
        }
    }

    private void EraseChildren()
    {
        _children = new OctreeNode[8];
    }

    private void KillNode(OctreeNode[] obsSiblings)
    {

        GameObject.Destroy(GO);

    }

    //Create overload  with data for data deserialaztion and node creation from memory
    public void Subdivide(Voxel[] data)
    {
        Vector3 newPos = new Vector3(halfSize / 2, halfSize / 2, halfSize / 2);
        for (int i = 0; i < 4; i++)
        {
            _children[i] = new OctreeNode(this, pos + newPos, halfSize / 2, data[i]);
            newPos = Quaternion.Euler(0, -90f, 0) * newPos;
        }
        newPos = new Vector3(halfSize / 2, -halfSize / 2, halfSize / 2);
        for (int i = 4; i < 8; i++)
        {
            _children[i] = new OctreeNode(this, pos + newPos, halfSize / 2, data[i]);
            newPos = Quaternion.Euler(0, -90f, 0) * newPos;
        }

    }

    public static OctreeNode NodeWithItem(Vector3 pos, OctreeNode start)
    {
        if (start.isLeaf())
        {
            return start;
        }
        foreach (var item in start.children.Where(x => x.ContainsItem(pos)))
        {
            return NodeWithItem(pos, item);
        }
        return null;
    }

    public bool ContainsItem(Vector3 position)
    {
        if (position.x > pos.x + halfSize || position.x < pos.x - halfSize)
            return false;
        if (position.y > pos.y + halfSize || position.y < pos.y - halfSize)
            return false;
        if (position.z > pos.z + halfSize || position.z < pos.z - halfSize)
            return false;
        return true;
    }

    void Visualize()
    {
        Vector3[] coords = new Vector3[8];
        Vector3 corner = new Vector3(halfSize, halfSize, halfSize);

        for (int i = 0; i < 4; i++)
        {
            coords[i] = pos + corner;
            corner = Quaternion.Euler(0, 90f, 0) * corner;
        }

        corner = new Vector3(halfSize, -halfSize, halfSize);

        for (int i = 4; i < 8; i++)
        {
            coords[i] = pos + corner;
            corner = Quaternion.Euler(0, 90f, 0) * corner;
        }

        ren.useWorldSpace = true;
        ren.positionCount = (16);
        ren.startWidth = halfSize / 100;
        ren.endWidth = halfSize / 100;
        ren.material = VoxelManager.vm.debugrenMaterialNorm;

        ren.SetPosition(0, coords[0]);
        ren.SetPosition(1, coords[1]);
        ren.SetPosition(2, coords[2]);
        ren.SetPosition(3, coords[3]);
        ren.SetPosition(4, coords[0]);
        ren.SetPosition(5, coords[4]);
        ren.SetPosition(6, coords[5]);
        ren.SetPosition(7, coords[1]);

        ren.SetPosition(8, coords[5]);
        ren.SetPosition(9, coords[6]);
        ren.SetPosition(10, coords[2]);
        ren.SetPosition(11, coords[6]);
        ren.SetPosition(12, coords[7]);
        ren.SetPosition(13, coords[3]);
        ren.SetPosition(14, coords[7]);
        ren.SetPosition(15, coords[4]);


    }

    public void SetRendererMaterial(Material mat)
    {
        ren.material = mat;
    }
}
