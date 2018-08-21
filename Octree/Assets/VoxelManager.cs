using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Collections;

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

    public static VoxelDataGenerater vdm;


    OctreeNode playerNode;
    OctreeNode prePlayerNode;

    public static MeshGenerator vmg;
    public MeshGenerator _vmg;

    Dictionary<Vector3Int, int[,,]> chunks = new Dictionary<Vector3Int, int[,,]>();


    public Material debugrenMaterialNorm, debugrenMaterialHigh;
    public int maxGenerations = 3;

    public Transform Player;

    public bool debug;

    public int worldSize, chunkSize;
    public Transform terrain;


    // Use this for initialization

    private void Awake()
    {
        _vm = this;
        vdm = new VoxelDataGenerater();
        vmg = _vmg;
        OctreeNode.Init();
    }

    public struct MyJob : IJob
    {
        public float a;
        public float b;
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = a + b;
        }
    }

    void Start()
    {
        NativeArray<int> d = new NativeArray<int>(1,Allocator.TempJob);
        MyJob mj = new MyJob();
        mj.a = 1;
        mj.b = 2;
        JobHandle j = mj.Schedule();
        j.Complete();

        Stopwatch s = new Stopwatch();


        s.Start();
        for (int x = 0; x < worldSize; x++)
            for (int y = 0; y < worldSize; y++)
                for (int z = 0; z < worldSize; z++)
                {
                    GameObject g = (GameObject)GameObject.Instantiate(Resources.Load("DefualtChunk"));
                    int[,,] data = vdm.GenerateData(chunkSize + 1, 2f, new Vector3Int(x * chunkSize, y * chunkSize, z * chunkSize));
                    Mesh m = vmg.MarchingCubes(data);
                    g.GetComponent<MeshFilter>().mesh = m;
                    g.GetComponent<MeshCollider>().sharedMesh = m;
                    g.transform.parent = terrain;
                    g.transform.position = new Vector3(x * chunkSize - 1, y * chunkSize - 1, z * chunkSize - 1);
                    g.name = string.Format("X: {0}, Y:{1}, Z{2}", x, y, z);

                }


        s.Stop();

        UnityEngine.Debug.Log(string.Format("Time to Generate: {0} for {1}", s.ElapsedMilliseconds.ToString(), worldSize * worldSize * worldSize * chunkSize));
    }



    void UpdateGenerations()
    {
        playerNode = OctreeNode.getRoot.CreateSubdivisionsWithItem(maxGenerations, Player.position);
        Invoke("UpdateGrpahics", 0.4f);
        //UpdateGrpahics();
    }

    void UpdateGrpahics()
    {
        vmg.GenerateCubes(OctreeNode.getRoot);

    }

    void FixedUpdate()
    {


        if (OctreeNode.getRoot.ContainsItem(Player.position) && (prePlayerNode == null || !prePlayerNode.ContainsItem(Player.position)))
        {
            //UpdateGenerations();

            if (prePlayerNode != null)
            {
                if (!prePlayerNode.ReduceSubdivisionsWithoutItem(Player.position, prePlayerNode) && debug)
                {
                    prePlayerNode.SetRendererMaterial(debugrenMaterialNorm);
                    prePlayerNode.ren.startWidth = prePlayerNode.halfSize / 100;
                    prePlayerNode.ren.endWidth = prePlayerNode.halfSize / 100;
                }
            }
            if (playerNode != null && debug)
            {
                playerNode.SetRendererMaterial(debugrenMaterialHigh);
                playerNode.ren.startWidth = playerNode.halfSize / 100 + Vector3.Distance(playerNode.pos, Player.position) / 100;
                playerNode.ren.endWidth = playerNode.halfSize / 100 + Vector3.Distance(playerNode.pos, Player.position) / 100;
            }
            prePlayerNode = playerNode;
        }
    }




}
