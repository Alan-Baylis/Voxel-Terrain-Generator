using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Collections;
using System.IO;


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

    //Dictionary<Vector3Int, int[]> chunks = new Dictionary<Vector3Int, int[]>();
    Queue<GenerationThread> generatinonJobs = new Queue<GenerationThread>();

    public Material debugrenMaterialNorm, debugrenMaterialHigh;
    public int maxGenerations = 3;

    public Transform Player;

    public bool debug;

    public int worldSize, chunkSize;
    public Transform terrain;

    public struct GenerationThreadJob : IJob
    {
        public int chunkSize, x, y, z;

        public NativeArray<Vector3> vertices;
        public NativeArray<int> tris;
        public NativeArray<Color> col;

        public void Execute()
        {
            int[] _tris;
            Color[] _cols;
            Vector3[] _verts;
            vmg.MarchingCubes(vdm.GenerateData(chunkSize, 2f, new Vector3Int(x * (chunkSize - 1), y * (chunkSize - 1), z * (chunkSize - 1))), chunkSize, out _verts, out _tris, out _cols);

            vertices = new NativeArray<Vector3>(_verts, Allocator.TempJob);
            tris = new NativeArray<int>(_tris, Allocator.TempJob);
            col = new NativeArray<Color>(_cols, Allocator.TempJob);

        }
    }

    public struct GenerationThread
    {
        public GenerationThreadJob job;
        public JobHandle jobHandle;
    }

    // Use this for initialization

    private void Awake()
    {
        _vm = this;
        vdm = new VoxelDataGenerater();
        vmg = _vmg;
        OctreeNode.Init();
    }


    void Start()
    {
        Stopwatch s = new Stopwatch();


        //GenerateWorldTest(s, writer);
        s.Start();
        GenerateWorld();
        s.Stop();

        StreamWriter writer = new StreamWriter("Assets/Resources/TimeToGenerate.txt", true);

        UnityEngine.Debug.Log(string.Format("Time to Generate: {0} for {1} vertices; chunkSize: {2}, worldSize: {3}", s.ElapsedMilliseconds.ToString(), Mathf.Pow(chunkSize * worldSize, 3), chunkSize, worldSize));
        writer.WriteLine(string.Format("{0},{1},{2},{3}", s.ElapsedMilliseconds.ToString(), Mathf.Pow(chunkSize * worldSize, 3), chunkSize, worldSize));
        writer.Dispose();

    }


    void Update()
    {
    }

    private void LateUpdate()
    {
        if (generatinonJobs.Count > 0)
        {
            GenerationThread t = generatinonJobs.Dequeue();
            t.jobHandle.Complete();

            GenerationThreadJob j = t.job;
            AddMeshToWorld(MeshFromData(j.vertices.ToArray(), j.tris.ToArray(), j.col.ToArray()), j.x * chunkSize - 1, j.y * chunkSize - 1, j.z * chunkSize - 1);

            j.vertices.Dispose();
            j.tris.Dispose();
            j.col.Dispose();

        }
    }

    private void OnDestroy()
    {
        foreach (var item in generatinonJobs)
        {
            generatinonJobs.Dequeue();
            item.job.vertices.Dispose();
            item.job.tris.Dispose();
            item.job.col.Dispose();

        }
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

    void CreateGenerationThread(int x, int y, int z)
    {
        GenerationThreadJob t = new GenerationThreadJob();
        t.chunkSize = chunkSize + 1;
        t.vertices = new NativeArray<Vector3>(1, Allocator.TempJob);
        t.tris = new NativeArray<int>(1, Allocator.TempJob);
        t.col = new NativeArray<Color>(1, Allocator.TempJob);

        t.x = x;
        t.y = y;
        t.z = z;

        GenerationThread gt = new GenerationThread();
        gt.job = t;
        gt.jobHandle = t.Schedule();
        generatinonJobs.Enqueue(gt);
    }

    void GenerateWorldTest()
    {
        for (int i = 0; i < 5; i++)
        {
            GenerateWorld();

            foreach (var item in terrain.GetComponentsInChildren<Transform>().Where(x => x != terrain))
                Destroy(item.gameObject);

            //chunks = new Dictionary<Vector3Int, int[]>();

            worldSize++;
        }
    }

    void GenerateWorld()
    {

        for (int x = 0; x < worldSize; x++)
            for (int y = 0; y < worldSize; y++)
                for (int z = 0; z < worldSize; z++)
                    //MeshFromData(x, y, z, vdm.GenerateData(chunkSize + 1, 2f, new Vector3Int(x * chunkSize, y * chunkSize, z * chunkSize)));
                    CreateGenerationThread(x, y, z);
    }

    //void MeshFromData(Vector3Int pos, int[] verts, int[] tris, int[] col)
    public Mesh MeshFromData(Vector3[] verts, int[] tris, Color[] col)
    {
        Mesh m = new Mesh();
        m.vertices = verts;
        m.triangles = tris;
        m.colors = col;
        m.RecalculateNormals();
        if (m.vertexCount > 65535)
        {
            UnityEngine.Debug.Log("Hit mesh vertex limit");
        }
        return m;
    }

    void AddMeshToWorld(Mesh m, int x, int y, int z)
    {
        GameObject g = (GameObject)Instantiate(Resources.Load("DefualtChunk"));

        g.GetComponent<MeshFilter>().mesh = m;
        g.GetComponent<MeshCollider>().sharedMesh = m;
        g.transform.parent = terrain;
        g.transform.position = new Vector3(x * chunkSize - 1, y * chunkSize - 1, z * chunkSize - 1);
        g.name = string.Format("X: {0}, Y:{1}, Z{2}", x, y, z);
    }

    public static int getIndex(int x, int y, int z, int chunkSize)
    {
        return x + chunkSize * (y + z * chunkSize);
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






}
