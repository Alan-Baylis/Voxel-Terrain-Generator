using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeItem : MonoBehaviour
{
    Vector3 prevPos;
    public List<OctreeNode> owners = new List<OctreeNode>();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (prevPos != transform.position)
        {
            OctreeNode.getRoot.ProcessItem(this);
            RefreshOwners();
            prevPos = transform.position;
        }
    }

    void RefreshOwners()
    {
        List<OctreeNode> survivors = new List<OctreeNode>();
        List<OctreeNode> obsolete = new List<OctreeNode>();

        foreach (var item in owners)
        {
            if (!item.ContainsItem(transform.position))
            {
                obsolete.Add(item);
            }
            else
            {
                survivors.Add(item);
            }
        }

        owners = survivors;

        foreach (var item in obsolete)
        {
            item.ReduceSubdivisions(this);
        }
    }
}
