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

    public MeshGenerator vmg;

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

    }

    void UpdateGenerations()
    {

        playerNode = OctreeNode.getRoot.CreateSubdivisionsWithItem(maxGenerations, Player.position);
        vmg.GenerateCubes(OctreeNode.getRoot);
    }

    void FixedUpdate()
    {


        if (OctreeNode.getRoot.ContainsItem(Player.position) && (playerNode == null || !playerNode.ContainsItem(Player.position)))
        {
            UpdateGenerations();

            if (prePlayerNode != null)
            {
                if (!prePlayerNode.ReduceSubdivisionsWithoutItem(Player.position, prePlayerNode))
                {
                    prePlayerNode.SetRendererMaterial(debugrenMaterialNorm);
                    prePlayerNode.ren.startWidth = prePlayerNode.halfSize / 100;
                    prePlayerNode.ren.endWidth = prePlayerNode.halfSize / 100;
                }
            }
            if (playerNode != null)
            {
                playerNode.SetRendererMaterial(debugrenMaterialHigh);
                playerNode.ren.startWidth = playerNode.halfSize / 100 + Vector3.Distance(playerNode.pos, Player.position) / 100;
                playerNode.ren.endWidth = playerNode.halfSize / 100 + Vector3.Distance(playerNode.pos, Player.position) / 100;
            }
            prePlayerNode = playerNode;
        }
    }


}
