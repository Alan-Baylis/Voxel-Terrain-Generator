using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class VoxelManager : MonoBehaviour
{

    static VoxelManager _vm;

    public static VoxelManager vm
    {
        get
        {
            return _vm;
        }
    }


    OctreeNode playerNode;
    OctreeNode prePlayerNode;
    VoxelDataGenerater DataGen;



    public Material debugrenMaterialNorm, debugrenMaterialHigh;
    public int maxGenerations = 3;

    public Transform Player;

    // Use this for initialization

    private void Awake()
    {
        _vm = this;
    }


    void Start()
    {
        DataGen = new VoxelDataGenerater();
        UpdateGenerations();
    }

    void UpdateGenerations()
    {
        playerNode = OctreeNode.getRoot;
        for (int i = 0; i < maxGenerations; i++)
        {
            playerNode.Subdivide(DataGen.GenerateChildData(playerNode));
            playerNode = OctreeNode.NodeWithItem(Player.position, playerNode);
        }
    }

    Vector3 prePos;
    void FixedUpdate()
    {

        if (prePos != Player.position)
        {
            //UpdateGenerations();
            if (playerNode == null || ReferenceEquals(playerNode, prePlayerNode)) return;

            if (prePlayerNode != null)
            {
                prePlayerNode.SetRendererMaterial(debugrenMaterialNorm);
            }

            playerNode.SetRendererMaterial(debugrenMaterialHigh);
            prePlayerNode = playerNode;
            prePos = Player.position;
        }
    }


}
