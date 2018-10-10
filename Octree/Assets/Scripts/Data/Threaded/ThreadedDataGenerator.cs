using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ThreadedDataGenerator : MonoBehaviour
{
    List<DataThread> runningThreads = new List<DataThread>();
    Queue<DataThread> idleThreads = new Queue<DataThread>();
    public Queue<JobToDo> toDO = new Queue<JobToDo>();
    public delegate void jobCallback(DataJob j);
    public jobCallback callBack;

    public int maxRunningThreads = 2;
    public int globalChunkSize;

    public struct JobToDo
    {
        public int chunkSize, x, y, z;
        public float scale;
    }

    public struct DataJob : IJob
    {
        public int chunkSize, x, y, z;
        public float scale;
        [WriteOnly]
        public NativeArray<int> data;

        public void Execute()
        {

            data.CopyFrom(VoxelManager.vdm.GenerateData(chunkSize, scale, new Vector3Int(x, y, z)));
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
                callBack(thread.job);
                runningThreads.RemoveAt(i);
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



    DataThread ScheduleThread(DataThread g, JobToDo p)
    {
        g.job.x = p.x;
        g.job.y = p.y;
        g.job.z = p.z;
        g.job.scale = p.scale;
        g.jobHandle = g.job.Schedule();

        return g;
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

    DataJob InitDataJob()
    {

        return new DataJob
        {
            chunkSize = globalChunkSize,
            data = new NativeArray<int>(globalChunkSize * globalChunkSize * globalChunkSize, Allocator.Persistent)
        };
    }

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

    void DisposeAll(DataThread item)
    {
        item.job.data.Dispose();
    }

}
