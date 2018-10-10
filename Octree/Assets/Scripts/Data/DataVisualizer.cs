using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataVisualizer : MonoBehaviour
{
    public static DataVisualizer thisDV;

    public int DisplaySize;
    public float scale = 1f;
    public Vector3Int pos;

    int[] data;
    GameObject[,,] visuals;
    DataGenerator vdm = new DataGenerator(6);

    public Transform debug;
    delegate int[] GenerateData();
    GenerateData genData;

    public MeshFilter mf;
    public bool showScalarField = false;
    public bool useTestData = false;
    public int[] testData = new int[8]
                {
                        0,0, // 0-4
                        0,0//3-7
                    ,
                        0,0,//2-6
                        0,0//1-5
                };

    private void Awake()
    {
        thisDV = this;
    }

    // Use this for initialization
    void Start()
    {

        if (useTestData)
        {
            genData = () =>
            {
                return testData;
            };

            DisplaySize = (int)Mathf.Pow(testData.Length, 1/3);
        }
        else
        {
            genData = () => vdm.GenerateData(DisplaySize, scale, pos * DisplaySize);
        }

        data = genData();
        if (showScalarField)
        {
            visuals = new GameObject[DisplaySize, DisplaySize, DisplaySize];
            DisplayData();
        }

        //Vector3[] verts;
        //int[] tris;
        //Color[] cols;

        //DataPolygonizer.MarchingCubes(data, DisplaySize, out verts, out tris, out cols);
        //mf.mesh = DataPolygonizer.MeshFromMeshData(verts, tris, cols);


    }

    public void Init()
    {
        Start();
    }

    public void DisplayData()
    {
        for (int i = 0; i < DisplaySize; i++)
            for (int j = 0; j < DisplaySize; j++)
                for (int k = 0; k < DisplaySize; k++)
                {
                    GameObject g = (GameObject)Instantiate(Resources.Load("DisplayCube"), new Vector3(i, j, k), Quaternion.identity, debug);
                    g.transform.localScale = new Vector3(1 / 10f, 1 / 10f, 1 / 10f);
                    g.GetComponent<Renderer>().material.color = ((Voxel)data[VoxelManager.getIndex(i, j, k, DisplaySize)]).color;
                    if (useTestData)
                    {
                        Vector3Int pos = new Vector3Int(i, j, k);
                        int index = 0;
                        if (pos == new Vector3Int(0, 0, 0))
                            index = 0;
                        else if (pos == new Vector3Int(1, 0, 0))
                            index = 1;
                        else if (pos == new Vector3Int(1, 1, 0))
                            index = 2;
                        else if (pos == new Vector3Int(0, 1, 0))
                            index = 3;
                        else if (pos == new Vector3Int(0, 0, 1))
                            index = 4;
                        else if (pos == new Vector3Int(1, 0, 1))
                            index = 5;
                        else if (pos == new Vector3Int(1, 1, 1))
                            index = 6;
                        else if (pos == new Vector3Int(0, 1, 1))
                            index = 7;
                        g.GetComponent<TextMesh>().text = index.ToString();
                    }
                    else
                    {
                        g.GetComponent<TextMesh>().text = (i + j * DisplaySize + k * DisplaySize * DisplaySize).ToString();
                    }
                }
    }

    public void ShowLine(Vector3 start, Vector3 end)
    {
        GameObject g = (GameObject)Instantiate(Resources.Load("DisplayLine"), Vector3.zero, Quaternion.identity, debug);
        LineRenderer r = g.GetComponent<LineRenderer>();
        r.SetPositions(new Vector3[] { start, end });
        r.startWidth = 0.1f;
        r.endWidth = 0.1f;

        r.material.color = Voxel.FILLED.color;
    }

    public void ResetVisuals()
    {
        foreach (var item in debug.GetComponentsInChildren<Transform>().Where(x => x != debug))
        {
            Destroy(item.gameObject);
        }
    }

}
