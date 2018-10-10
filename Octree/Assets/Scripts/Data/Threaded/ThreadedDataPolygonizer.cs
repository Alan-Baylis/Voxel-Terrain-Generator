using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ThreadedDataPolygonizer : MonoBehaviour
{

    List<DataThread> runningThreads = new List<DataThread>();
    Queue<DataThread> idleThreads = new Queue<DataThread>();
    public Queue<JobToDo> toDO = new Queue<JobToDo>();
    public Queue<JobFinished> finishedJobs = new Queue<JobFinished>();

    public delegate void jobCallback(DataJob j);
    public jobCallback callBack;

    public int maxRunningThreads = 2;
    public int globalChunkSize, maxVerts;

    public struct JobToDo
    {
        public int chunkSize, x, y, z;
        public int[] data;
    }

    public struct JobFinished
    {
        public int chunkSize, x, y, z;
        public Vector3[] vertices;
        public int[] tris, lengths;
        public Color[] col;
    }

    public struct DataJob : IJob
    {
        public int chunkSize, x, y, z;

        [ReadOnly]
        public NativeArray<int> data;

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

            DataPolygonizer.MarchingCubes(data.ToArray(), chunkSize, out _verts, out _tris, out _cols);
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

    public struct DataThread
    {
        public DataJob job;
        public JobHandle jobHandle;
    }

    public void UpdateThreads()
    {
        for (int i = 0; i < runningThreads.Count; i++)
        {
            DataThread thread = runningThreads[i];
            if (thread.jobHandle.IsCompleted)
            {
                thread.jobHandle.Complete();
                FinishJob(thread.job);
                runningThreads.RemoveAt(i);
                i--;
                if (toDO.Count > 0)
                {
                    runThread(thread);
                }
                else
                    idleThreads.Enqueue(thread);

            }
        }

        for (int i = 0; i < idleThreads.Count; i++)
        {
            if (toDO.Count > 0)
            {
                DataThread t = idleThreads.Dequeue();
                runThread(t);
            }
        }

        //At the end of voxel managers update call JobHandle.ScheduleBatchedJobs(); to start execution on worker threads
    }

    void FinishJob(DataJob g)
    {
        finishedJobs.Enqueue(new JobFinished
        {
            x = g.x,
            y = g.y,
            z = g.z,
            chunkSize = g.chunkSize,
            vertices = g.vertices.ToArray(),
            tris = g.tris.ToArray(),
            col = g.col.ToArray(),
            lengths = g.lengths.ToArray()
        });
    }

    DataThread ScheduleThread(DataThread g, JobToDo c)
    {
        g.job.x = c.x;
        g.job.y = c.y;
        g.job.z = c.z;
        g.job.data.CopyFrom(c.data);
        g.jobHandle = g.job.Schedule();

        return g;
    }

    DataJob InitDataJob()
    {
        return new DataJob
        {
            chunkSize = globalChunkSize,
            data = new NativeArray<int>(globalChunkSize * globalChunkSize * globalChunkSize, Allocator.TempJob),

            lengths = new NativeArray<int>(2, Allocator.TempJob),
            vertices = new NativeArray<Vector3>(maxVerts, Allocator.TempJob),
            tris = new NativeArray<int>(maxVerts, Allocator.TempJob),
            col = new NativeArray<Color>(maxVerts, Allocator.TempJob)
        };
    }

    public void InitThreads()
    {
        for (int i = 0; i < maxRunningThreads; i++)
        {
            DataThread thread = new DataThread
            {
                job = InitDataJob()
            };

            if (toDO.Count > 0)
            {
                runThread(thread);
            }
            else
            {
                idleThreads.Enqueue(thread);
            }
        }

    }

    ///<summary>
    ///<para>Take thread and assign the next chunkToGenerate data to it</para>
    ///</summary>
    void runThread(DataThread thread)
    {
        runningThreads.Add(ScheduleThread(thread, toDO.Dequeue()));
    }

    protected void OnDestroy()
    {
        foreach (var item in runningThreads)
        {
            item.jobHandle.Complete();
            DisposeAll(item);
        }
        foreach (var item in idleThreads)
        {
            DisposeAll(item);
        }
    }

    void DisposeAll(DataThread thread)
    {
        thread.job.data.Dispose();
        thread.job.lengths.Dispose();
        thread.job.vertices.Dispose();
        thread.job.tris.Dispose();
        thread.job.col.Dispose();
    }

}
