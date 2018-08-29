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


    public Material debugrenMaterialNorm, debugrenMaterialHigh;
    public int maxOctreeGenerations = 3;

    public Transform Player;

    public bool debug;

    public Vector3Int worldSize;
    public int chunkSize;
    public Transform terrain;
    public int generationJobsQouta = 10;
    public int generationJobsIdleQouta = 20;

    public int maxVerts = 65535;

    public struct GenerationThreadJob : IJob
    {
        public int chunkSize, x, y, z;

        [WriteOnly]
        public NativeArray<int> lengths;
        [WriteOnly]
        public NativeArray<Vector3> vertices;
        [WriteOnly]
        public NativeArray<int> tris;
        [WriteOnly]
        public NativeArray<Color> col;

        public void Execute()
        {
            int[] _tris;
            Color[] _cols;
            Vector3[] _verts;
            vmg.MarchingCubes(vdm.GenerateData(chunkSize, 2f, new Vector3Int(x * (chunkSize - 1), y * (chunkSize - 1), z * (chunkSize - 1))), chunkSize, out _verts, out _tris, out _cols);
            lengths[0] = _verts.Length;
            for (int i = 0; i < _verts.Length; i++)
            {
                vertices[i] = _verts[i];
                col[i] = _cols[i];
            }
            lengths[1] = _tris.Length;
            for (int i = 0; i < _tris.Length; i++)
            {
                tris[i] = _tris[i];
            }
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

        UnityEngine.Debug.Log(string.Format("Time to Generate: {0} for {1} vertices; chunkSize: {2}, worldSize: {3}", s.ElapsedMilliseconds.ToString(), Mathf.Pow(chunkSize * worldSize.x, 3), chunkSize, worldSize.x));
        writer.WriteLine(string.Format("{0},{1},{2},{3}", s.ElapsedMilliseconds.ToString(), Mathf.Pow(chunkSize * worldSize.x, 3), chunkSize, worldSize));
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
            item.job.lengths.Dispose();
            item.job.vertices.Dispose();
            item.job.tris.Dispose();
            item.job.col.Dispose();

        }
        foreach (var item in generationThreadsIdle)
        {
            item.job.lengths.Dispose();
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
        for (int i = 0; i < generationThreadsIdle.Count; i++)
        {
            if (chunksToGenerate.Count > 0)
            {
                Vector3Int pos = chunksToGenerate.Dequeue();
                GenerationThread t = generationThreadsIdle.Dequeue();
                generationThreads.Enqueue(ScheduleGenerationThread(t, pos.x, pos.y, pos.z));
            }
        }

        LateUpdateGenerationOnThreads();


        if (chunksToGenerate.Count > 0 || generationThreads.Count > 0)
            Invoke("UpdateGenerationOnThreads", 0.1f);
    }

    void LateUpdateGenerationOnThreads()
    {
        for (int k = 0; k < generationJobsQouta && generationThreads.Count > 0; k++)
        {
            GenerationThread t = generationThreads.Dequeue();
            GenerationThreadJob j = t.job;
            t.jobHandle.Complete();


            int[] _tris = new int[j.lengths[1]];
            Color[] _cols = new Color[j.lengths[0]];
            Vector3[] _verts = new Vector3[j.lengths[0]];

            for (int i = 0; i < j.lengths[0]; i++)
            {
                _verts[i] = j.vertices[i];
                _cols[i] = j.col[i];
            }
            for (int i = 0; i < j.lengths[1]; i++)
            {
                _tris[i] = j.tris[i];
            }

            AddMeshToWorld(MeshFromData(_verts, _tris, _cols), j.x, j.y, j.z);
            generationThreadsIdle.Enqueue(t);
        }
    }

    GenerationThread ScheduleGenerationThread(GenerationThread g, int x, int y, int z)
    {
        g.job.x = x;
        g.job.y = y;
        g.job.z = z;
        g.jobHandle = g.job.Schedule();

        return g;
    }

    void InitGenerationOnThreads()
    {
        for (int i = 0; i < generationJobsIdleQouta; i++)
        {
            GenerationThreadJob t = new GenerationThreadJob
            {
                chunkSize = chunkSize + 1,
                lengths = new NativeArray<int>(2, Allocator.TempJob),
                vertices = new NativeArray<Vector3>(maxVerts, Allocator.TempJob),
                tris = new NativeArray<int>(maxVerts, Allocator.TempJob),
                col = new NativeArray<Color>(maxVerts, Allocator.TempJob)
            };
            GenerationThread gt = new GenerationThread
            {
                job = t
            };

            if (generationThreads.Count < generationJobsQouta)
            {
                if (chunksToGenerate.Count > 0)
                {
                    Vector3Int p = chunksToGenerate.Dequeue();
                    t.x = p.x;
                    t.y = p.y;
                    t.z = p.z;

                    gt.job = t;
                    gt.jobHandle = gt.job.Schedule();
                    generationThreads.Enqueue(gt);
                }
                else
                {
                    generationThreadsIdle.Enqueue(gt);
                }
            }
            else
            {
                generationThreadsIdle.Enqueue(gt);
            }
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

            worldSize += Vector3Int.one;

        }
    }

    void GenerateWorld()
    {
        for (int x = 0; x < worldSize.x; x++)
            for (int y = 0; y < worldSize.y; y++)
                for (int z = 0; z < worldSize.z; z++)
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
        playerNode = OctreeNode.getRoot.CreateSubdivisionsWithItem(maxOctreeGenerations, Player.position);
        Invoke("UpdateGrpahics", 0.4f);
        //UpdateGrpahics();
    }

    void UpdateGrpahics()
    {
        vmg.GenerateCubes(OctreeNode.getRoot);

    }






}
