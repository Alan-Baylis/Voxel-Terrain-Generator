using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using Unity.Jobs;
using Unity.Collections;
using System.IO;
using UnityEditor;

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
    Queue<GenerationThread> generationThreads = new Queue<GenerationThread>();
    Queue<GenerationThread> generationThreadsIdle = new Queue<GenerationThread>();
    Queue<Vector3Int> chunksToGenerate = new Queue<Vector3Int>();

    public int generationJobQouta = 10;

    public Material debugrenMaterialNorm, debugrenMaterialHigh;
    public int maxGenerations = 3;

    public Transform Player;

    public bool debug;

    public int worldSize, chunkSize;
    public Transform terrain;

    public int maxVerts = 65535;

    public struct GenerationThreadJob : IJob
    {
        public int chunkSize, x, y, z;
        public int verticesLength, trisLength;

        public NativeArray<Vector3> vertices;
        public NativeArray<int> tris;
        public NativeArray<Color> col;

        public void Execute()
        {
            int[] _tris;
            Color[] _cols;
            Vector3[] _verts;
            vmg.MarchingCubes(vdm.GenerateData(chunkSize, 2f, new Vector3Int(x * (chunkSize - 1), y * (chunkSize - 1), z * (chunkSize - 1))), chunkSize, out _verts, out _tris, out _cols);
            verticesLength = _verts.Length;
            for (int i = 0; i < verticesLength; i++)
            {
                vertices[i] = _verts[i];
                col[i] = _cols[i];
            }
            trisLength = _tris.Length;
            for (int i = 0; i < trisLength; i++)
            {
                tris[i] = _tris[i];
            }

            UnityEngine.Debug.Log("In thread" + verticesLength);
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

        GenerationThread t = new GenerationThread();
        t.job.chunkSize = chunkSize + 1;
        t.job.vertices = new NativeArray<Vector3>(maxVerts, Allocator.TempJob);
        t.job.tris = new NativeArray<int>(maxVerts, Allocator.TempJob);
        t.job.col = new NativeArray<Color>(maxVerts, Allocator.TempJob);
        t.job.x = 0;
        t.job.y = 0;
        t.job.z = 0;
        t.jobHandle = t.job.Schedule();
        t.jobHandle.Complete();
        UnityEngine.Debug.Log(t.jobHandle.IsCompleted);
        UnityEngine.Debug.Log(t.job.verticesLength);
        UnityEngine.Debug.Log(t.job.vertices.Count());


        //for (int x = 0; x < worldSize; x++)
        //    for (int y = 0; y < worldSize; y++)
        //        for (int z = 0; z < worldSize; z++)
        //        {

        //            t.job.x = x;
        //            t.job.y = y;
        //            t.job.z = z;
        //            t.jobHandle = t.job.Schedule();
        //            t.jobHandle.Complete();
        //            GenerationThreadJob j = t.job;

        //            int[] _tris = new int[j.trisLength];
        //            Color[] _cols = new Color[j.verticesLength];
        //            Vector3[] _verts = new Vector3[j.verticesLength];

        //            for (int i = 0; i < j.verticesLength; i++)
        //            {
        //                _verts[i] = j.vertices[i];
        //                _cols[i] = j.col[i];
        //            }
        //            for (int i = 0; i < j.trisLength; i++)
        //            {
        //                _tris[i] = j.tris[i];
        //            }

        //            UnityEngine.Debug.Log(j.verticesLength);
        //            AddMeshToWorld(MeshFromData(_verts, _tris, _cols), j.x, j.y, j.z);

        //        }

        t.job.vertices.Dispose();
        t.job.tris.Dispose();
        t.job.col.Dispose();


        //GenerateWorld();
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

    }

    private void OnDestroy()
    {
        foreach (var item in generationThreads)
        {
            item.jobHandle.Complete();
            item.job.vertices.Dispose();
            item.job.tris.Dispose();
            item.job.col.Dispose();

        }
        foreach (var item in generationThreadsIdle)
        {
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

    //---------------------------------------------------------------------------------------------

    void UpdateGenerationOnThreads()
    {
        if (generationThreads.Count > 0)
        {
            GenerationThread t = generationThreads.Dequeue();
            t.jobHandle.Complete();

            GenerationThreadJob j = t.job;

            int[] _tris = new int[j.trisLength];
            Color[] _cols = new Color[j.verticesLength];
            Vector3[] _verts = new Vector3[j.verticesLength];

            for (int i = 0; i < j.verticesLength; i++)
            {
                _verts[i] = j.vertices[i];
                _cols[i] = j.col[i];
            }
            for (int i = 0; i < j.trisLength; i++)
            {
                _tris[i] = j.tris[i];
            }

            AddMeshToWorld(MeshFromData(_verts, _tris, _cols), j.x, j.y, j.z);
            generationThreadsIdle.Enqueue(t);
        }

        if (chunksToGenerate.Count > 0)
        {
            if (generationThreadsIdle.Count > 0)
            {
                Vector3Int pos = chunksToGenerate.Dequeue();
                GenerationThread t = generationThreadsIdle.Dequeue();
                ScheduleGenerationThread(t, pos.x, pos.y, pos.z);
                generationThreads.Enqueue(t);
            }
            Invoke("UpdateGenerationOnThreads", 0.1f);
        }
    }

    void ScheduleGenerationThread(GenerationThread g, int x, int y, int z)
    {
        g.job.x = x;
        g.job.y = y;
        g.job.z = z;
        g.jobHandle = g.job.Schedule();

    }

    void InitGenerationOnThreads()
    {
        for (int i = 0; i < generationJobQouta; i++)
        {
            GenerationThreadJob t = new GenerationThreadJob();
            t.chunkSize = chunkSize + 1;
            t.vertices = new NativeArray<Vector3>(maxVerts, Allocator.TempJob);
            t.tris = new NativeArray<int>(maxVerts, Allocator.TempJob);
            t.col = new NativeArray<Color>(maxVerts, Allocator.TempJob);

            GenerationThread gt = new GenerationThread();
            gt.job = t;
            gt.job.Schedule();
            generationThreads.Enqueue(gt);
        }

    }

    void CreateGeneration()
    {
        int[] _tris;
        Color[] _cols;
        Vector3[] _verts;

        for (int i = 0; i < chunksToGenerate.Count; i++)
        {
            Vector3Int pos = chunksToGenerate.Dequeue();
            int x = pos.x;
            int y = pos.y;
            int z = pos.z;

            vmg.MarchingCubes(vdm.GenerateData(chunkSize + 1, 2f, new Vector3Int(x * (chunkSize), y * (chunkSize), z * (chunkSize))), chunkSize + 1, out _verts, out _tris, out _cols);

            AddMeshToWorld(MeshFromData(_verts, _tris, _cols), x, y, z);
        }
    }

    void CreateGenerationOnThreads()
    {
        InitGenerationOnThreads();
        UpdateGenerationOnThreads();
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
                {
                    chunksToGenerate.Enqueue(new Vector3Int(x, y, z));
                }

        //CreateGeneration();
        CreateGenerationOnThreads();
    }

    public Mesh MeshFromData(Vector3[] verts, int[] tris, Color[] col)
    {
        Mesh m = new Mesh
        {
            vertices = verts,
            triangles = tris,
            colors = col
        };
        m.RecalculateNormals();
        if (m.vertexCount > maxVerts)
        {
            UnityEngine.Debug.Log("Hit mesh vertex limit");
        }
        return m;
    }

    void AddMeshToWorld(Mesh m, int x, int y, int z)
    {
        GameObject g = (GameObject)Instantiate(Resources.Load("DefualtChunk"));
        g.GetComponent<MeshFilter>().mesh = m;
        //g.GetComponent<MeshCollider>().sharedMesh = m;
        g.transform.parent = terrain;
        g.transform.position = new Vector3(x * chunkSize - 1, y * chunkSize - 1, z * chunkSize - 1);
        g.name = string.Format("X: {0}, Y:{1}, Z{2}", x, y, z);
    }

    public static int getIndex(int x, int y, int z, int chunkSize)
    {
        return x + chunkSize * (y + z * chunkSize);
    }

    //-------------------------------------------------------------------------------------------

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
