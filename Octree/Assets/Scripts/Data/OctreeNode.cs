using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OctreeNode
{

    static OctreeNode _root;
    public static OctreeNode getRoot
    {
        get
        {
            if (_root == null)
            {
                _root = new OctreeNode(null, Vector3.zero, 64f, Voxel.FILLED, 0);
            }

            return _root;
        }
    }

    public OctreeNode parent;
    public Voxel data;

    private OctreeNode[] _children = new OctreeNode[8];
    public OctreeNode[] children
    {
        get
        {
            return _children;
        }
    }

    public bool isLeaf()
    {
        return ReferenceEquals(_children[0], null);
    }

    public bool isRoot()
    {
        return this == _root;
    }


    public float halfSize;
    public Vector3 pos { get; private set; }

    public int depth { get; private set; }
    public int childIndex { get; private set; }


    GameObject GO;
    public LineRenderer ren;

    //[RuntimeInitializeOnLoadMethod]
    public static bool Init()
    {

        return getRoot == null;

    }

    public OctreeNode(OctreeNode parent, Vector3 pos, float halfSize, Voxel _data, int childIndex)
    {
        this.parent = parent;
        this.pos = pos;
        this.halfSize = halfSize;
        data = _data;
        this.childIndex = childIndex;

        depth = (parent == null ? 0 : parent.depth) + 1;

        if (VoxelManager.vm.debug)
        {
            GO = new GameObject();
            //GO.hideFlags = HideFlags.HideInHierarchy;
            ren = GO.AddComponent<LineRenderer>();
            GO.transform.parent = (parent == null ? null : parent.GO.transform);
            GO.name = depth.ToString();
            GO.name = depth.ToString() + " " + childIndex.ToString();
            Visualize();
        }
    }



    private void EraseChildren()
    {
        foreach (var item in _children)
        {
            item.KillNode();
        }
        _children = new OctreeNode[8];
    }

    private void KillNode()
    {
        //VoxelManager.vmg.removeMesh.Add(this);
        GameObject.Destroy(GO);
    }

    //Create overload  with data for data deserialaztion and node creation from memory
    public void Subdivide(Voxel[] data)
    {
        Vector3[] positions = childrenPositions(pos);
        Subdivide(data, positions);
    }

    public void Subdivide(Voxel[] data, Vector3[] positions)
    {
        for (int i = 0; i < 4; i++)
        {
            _children[i] = new OctreeNode(this, positions[i], halfSize / 2, data[i], i);
        }
        for (int i = 4; i < 8; i++)
        {
            _children[i] = new OctreeNode(this, positions[i], halfSize / 2, data[i], i);
        }

    }

    public Vector3[] childrenPositions(Vector3 pos)
    {
        Vector3[] v = new Vector3[8];
        Vector3 newPos = new Vector3(halfSize / 2, halfSize / 2, halfSize / 2);
        for (int i = 0; i < 4; i++)
        {
            v[i] = pos + newPos;
            newPos = Quaternion.Euler(0, -90f, 0) * newPos;
        }
        newPos = new Vector3(halfSize / 2, -halfSize / 2, halfSize / 2);
        for (int i = 4; i < 8; i++)
        {
            v[i] = pos + newPos;
            newPos = Quaternion.Euler(0, -90f, 0) * newPos;
        }
        return v;
    }

    public static OctreeNode ChildNodeWithItem(Vector3 pos, OctreeNode start)
    {
        foreach (var item in start.children)
        {
            if (item.ContainsItem(pos))
            {
                return item;
            }
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

    public OctreeNode CreateSubdivisionsWithItem(int maxDepth, Vector3 pos)
    {
        OctreeNode nextChild = this;

        for (int i = 0; i < maxDepth; i++)
        {
            if (nextChild.isLeaf())
            {
                Vector3[] positions = nextChild.childrenPositions(nextChild.pos);
                nextChild.Subdivide(DataGenerator.GenerateChildData(this, positions), positions);
            }
            nextChild = ChildNodeWithItem(pos, nextChild);
            if (nextChild == null) return null;

        }

        return nextChild;
    }

    public bool ReduceSubdivisionsWithoutItem(Vector3 pos, OctreeNode start)
    {
        bool erased = false;
        while (!start.ParentNodeHasItem(pos))
        {
            erased = true;
            start = start.parent;
            start.EraseChildren();
        }

        return erased;
    }

    public bool ParentNodeHasItem(Vector3 pos)
    {
        if (!ReferenceEquals(getRoot, this))
        {
            return parent.ContainsItem(pos);
        }
        else
        {
            return true;
        }
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
        if (VoxelManager.vm.debug)
            ren.material = mat;
    }
}
