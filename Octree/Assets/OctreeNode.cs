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
                _root = new OctreeNode(null, Vector3.zero, 50f, new List<OctreeItem>());
            }

            return _root;
        }
    }

    public OctreeNode parent;
    public List<OctreeItem> containedItems = new List<OctreeItem>();

    OctreeNode[] _children = new OctreeNode[8];
    public OctreeNode[] children
    {
        get
        {
            return _children;
        }
    }

    public float halfSize;
    public Vector3 pos { get; private set; }


    static int itemLimit = 1;
    static float sizeLimit = 0.25f;

    GameObject GO;
    LineRenderer ren;
    public bool debug = true;

    public Voxel data;

    [RuntimeInitializeOnLoadMethod]
    static bool Init()
    {

        return getRoot == null;

    }

    public OctreeNode(OctreeNode parent, Vector3 pos, float halfSize, List<OctreeItem> items)
    {
        //data = Voxel.EMPTY;
        data = (Voxel)UnityEngine.Random.Range(0, 2);

        //Debug.Log(data);
        CreateNode(parent, pos, halfSize, items);
    }

    public OctreeNode(OctreeNode parent, Vector3 pos, float halfSize, List<OctreeItem> items, Voxel data)
    {
        this.data = data;
        CreateNode(parent, pos, halfSize, items);
    }

    void CreateNode(OctreeNode parent, Vector3 pos, float halfSize, List<OctreeItem> items)
    {
        this.parent = parent;
        this.pos = pos;
        this.halfSize = halfSize;

        if (debug)
        {
            GO = new GameObject();
            GO.hideFlags = HideFlags.HideInHierarchy;
            ren = GO.AddComponent<LineRenderer>();
            Visualize();
        }

        foreach (var item in items)
        {
            ProcessItem(item);
        }
    }

    public void ReduceSubdivisions(OctreeItem item)
    {
        if (!ReferenceEquals(getRoot, this) && !SiblingsHaveTooManyChildren())
        {
            foreach (var child in parent.children)
            {
                if (!ReferenceEquals(child, null))
                {
                    child.KillNode(parent.children.Where(i => !ReferenceEquals(i, this)).ToArray());
                }
            }
            parent.EraseChildren();
        }
        else
        {
            containedItems.Remove(item);
            item.owners.Remove(this);
        }
    }

    private void EraseChildren()
    {
        _children = new OctreeNode[8];
    }

    private void KillNode(OctreeNode[] obsSiblings)
    {
        foreach (var item in containedItems)
        {
            item.owners = item.owners.Except(obsSiblings).ToList();
            item.owners.Remove(this);
            item.owners.Add(parent);
            parent.containedItems.Add(item);
        }
        GameObject.Destroy(GO);

    }

    bool SiblingsHaveTooManyChildren()
    {
        List<OctreeItem> childItems = new List<OctreeItem>();
        foreach (var item in parent.children)
        {
            if (!ReferenceEquals(item, null))
            {
                if (!ReferenceEquals(item._children[0], null))
                {
                    return true;
                }
                childItems.AddRange(item.containedItems.Where(i => !childItems.Contains(i)));
            }
        }

        if (childItems.Count > itemLimit + 1)
        {
            return true;
        }

        return false;
    }

    public bool ProcessItem(OctreeItem item)
    {
        if (ContainsItem(item.transform.position))
        {
            if (ReferenceEquals(children[0], null))
            {
                PushItem(item);
                return true;
            }
            else
            {
                foreach (var child in children)
                {
                    if (child.ProcessItem(item))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void PushItem(OctreeItem item)
    {
        if (!containedItems.Contains(item))
        {
            containedItems.Add(item);
            item.owners.Add(this);
        }

        if (containedItems.Count > itemLimit)
        {
            Split();
        }
    }

    private void Split()
    {
        if (halfSize / 2 < sizeLimit)
        {
            return;
        }

        foreach (var item in containedItems)
        {
            item.owners.Remove(this);
        }

        Vector3 newPos = new Vector3(halfSize / 2, halfSize / 2, halfSize / 2);
        for (int i = 0; i < 4; i++)
        {
            _children[i] = new OctreeNode(this, pos + newPos, halfSize / 2, containedItems);
            newPos = Quaternion.Euler(0, -90f, 0) * newPos;
        }
        newPos = new Vector3(halfSize / 2, -halfSize / 2, halfSize / 2);
        for (int i = 4; i < 8; i++)
        {
            _children[i] = new OctreeNode(this, pos + newPos, halfSize / 2, containedItems);
            newPos = Quaternion.Euler(0, -90f, 0) * newPos;
        }

        containedItems.Clear();
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
        ren.startWidth = 0.03f;
        ren.endWidth = 0.03f;

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
}
