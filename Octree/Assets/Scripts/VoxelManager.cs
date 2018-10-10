using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Unity.Jobs;

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

    public static DataGenerator vdm;

    OctreeNode playerNode;
    OctreeNode prePlayerNode;
    public Transform Player;

    public Material debugrenMaterialNorm, debugrenMaterialHigh;
    public int maxOctreeGenerations = 3;


    public bool debug;

    public Vector3Int worldSize;
    public int chunkSize;
    public float scale = 1f;
    public Transform terrain;
    public int maxVerts = 65535;

    Dictionary<Vector3Int, int[]> dataChunks = new Dictionary<Vector3Int, int[]>();
    public ThreadedDataPolygonizer threadedPolygonizer;
    public ThreadedDataGenerator threadedDataGenerator;

    private void Awake()
    {
        _vm = this;
        vdm = new DataGenerator(worldSize.y);
        OctreeNode.Init();
    }

    Stopwatch s = new Stopwatch();
    void Start()
    {
        InitThreads();

        s.Start();
        GenerateStartingChunks();
        s.Stop();

        StreamWriter writer = new StreamWriter("Assets/Resources/TimeToGenerate.txt", true);

        UnityEngine.Debug.Log(string.Format("Time to Generate: {0} for {1} vertices; chunkSize: {2}, worldSize: {3}", s.ElapsedMilliseconds.ToString(), Mathf.Pow(chunkSize * worldSize.x, 3), chunkSize, worldSize.x));
        writer.WriteLine(string.Format("{0},{1},{2},{3}", s.ElapsedMilliseconds.ToString(), Mathf.Pow(chunkSize * worldSize.x, 3), chunkSize, worldSize));
        writer.Dispose();

        s.Reset();
    }

    void Update()
    {
        s.Start();
        threadedDataGenerator.UpdateThreads();
        threadedPolygonizer.UpdateThreads();
        //TODO: if enough jobs have been batched call  JobHandle.ScheduleBatchedJobs();
        JobHandle.ScheduleBatchedJobs();
    }

    void LateUpdate()
    {
        s.Stop();
        if (s.ElapsedTicks < 1000)
            if (threadedPolygonizer.finishedJobs.Count > 0)
                ThreadedDataPolygonizerDataJobTOMesh(threadedPolygonizer.finishedJobs.Dequeue());

        s.Reset();
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateWorldFromOctree(OctreeNode.getRoot);
        }
        if (Input.GetKeyDown(KeyCode.O))
            if (OctreeNode.getRoot.ContainsItem(Player.position) && (prePlayerNode == null || !prePlayerNode.ContainsItem(Player.position)))
            {
                UpdateOctree();

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

    void InitThreads()
    {
        threadedDataGenerator.callBack += ThreadedDataGenerationReturn;
        threadedDataGenerator.globalChunkSize = chunkSize;
        threadedDataGenerator.InitThreads();

        threadedPolygonizer.globalChunkSize = chunkSize;
        threadedPolygonizer.maxVerts = maxVerts;
        threadedPolygonizer.InitThreads();

        JobHandle.ScheduleBatchedJobs();
    }

    void ThreadedDataGenerationReturn(ThreadedDataGenerator.DataJob j)
    {
        int[] _data = j.data.ToArray();
        dataChunks.Add(new Vector3Int(j.x, j.y, j.z), _data);
        threadedPolygonizer.toDO.Enqueue(new ThreadedDataPolygonizer.JobToDo
        {
            x = j.x,
            y = j.y,
            z = j.z,
            data = _data
        });
    }

    void ThreadedDataPolygonizerDataJobTOMesh(ThreadedDataPolygonizer.JobFinished j)
    {
        Vector3[] vertices = new Vector3[j.lengths[0]];
        int[] tris = new int[j.lengths[1]];
        Color[] col = new Color[j.lengths[0]];

        Vector3[] _verts = j.vertices.ToArray();
        int[] _tris = j.tris.ToArray();
        Color[] _cols = j.col.ToArray();

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = _verts[i];
            col[i] = _cols[i];
        }
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = _tris[i];
        }

        AddMeshToWorld(DataPolygonizer.MeshFromMeshData(vertices, tris, col), j.x, j.y, j.z);
    }

    public struct DataChunk
    {
        public int[] data;
        public int x, y, z;
    }

    void GenerateWorldFromOctree(OctreeNode n)
    {
        foreach (OctreeNode item in n.children)
        {
            //TODO: chunks to enque should pass octree node child (pos, size)
            //chunksToGenerate.Enqueue(,)
        }
    }

    void GenerateChunk(int x, int y, int z, bool threaded)
    {
        if (threaded)
            threadedDataGenerator.toDO.Enqueue(new ThreadedDataGenerator.JobToDo { x = x, y = y, z = z, scale = scale });
        else
            AddMeshToWorld(DataPolygonizer.MeshFromChunkData(vdm.GenerateData(chunkSize, scale, new Vector3Int(x, y, z)), chunkSize), x, y, z);
    }

    void GenerateStartingChunks()
    {
        for (int x = 0; x < worldSize.x; x++)
            for (int y = 0; y < worldSize.y; y++)
                for (int z = 0; z < worldSize.z; z++)
                {
                    GenerateChunk(x, y, z, true);
                }
    }


    public void AddMeshToWorld(Mesh m, int x, int y, int z)
    {
        int _chunkSize = chunkSize - 1;
        GameObject g = (GameObject)Instantiate(Resources.Load("DefualtChunk"));
        g.GetComponent<MeshFilter>().mesh = m;
        g.GetComponent<MeshCollider>().sharedMesh = m;
        g.transform.parent = terrain;
        g.transform.position = new Vector3(x * _chunkSize, y * _chunkSize, z * _chunkSize);
        g.name = string.Format("X: {0}, Y:{1}, Z{2}", x, y, z);
    }

    public static int getIndex(int x, int y, int z, int chunkSize)
    {
        return x + chunkSize * (y + z * chunkSize);
    }

    //-------------------------------------------------------------------------------------------

    void UpdateOctree()
    {
        playerNode = OctreeNode.getRoot.CreateSubdivisionsWithItem(maxOctreeGenerations, Player.position);
        //Invoke("UpdateGrpahics", 0.4f);
        //UpdateGrpahics();
    }

}
