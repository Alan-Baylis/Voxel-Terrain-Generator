using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshGenerator : MonoBehaviour
{

    OctreeNode root;
    public Transform terrain;

    void Start()
    {
        root = OctreeNode.getRoot;
    }

    void GenerateCubes(OctreeNode node)
    {
        if (ReferenceEquals(node.children[0], null))
        {

            //Debug.Log(" at Mesh " + node.data);
            if (node.data.Equals(Voxel.FILLED))
            {
                GenerateCube(node.pos, node.halfSize * 2f);
            }
        }
        else
        {
            foreach (var item in node.children)
            {
                GenerateCubes(item);
            }
        }

    }

    void GenerateCube(Vector3 pos, float size)
    {
        GameObject GO = (GameObject)Instantiate(Resources.Load("Voxel"));
        GO.transform.localScale = new Vector3(size, size, size);
        GO.transform.position = pos;
        GO.transform.parent = terrain;
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            foreach (var item in terrain.GetComponentsInChildren<Transform>().Where(x => !ReferenceEquals(x, terrain)))
            {
                Destroy(item.gameObject);
            }
            Debug.Log("Generating Cubes");
            GenerateCubes(root);
        }
    }
}
