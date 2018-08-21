﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataVisualizer : MonoBehaviour
{
    public static DataVisualizer thisDV;

    public int DisplaySize;
    public float scale = 1f;

    int[,,] data;
    GameObject[,,] visuals;
    VoxelDataGenerater vdm = new VoxelDataGenerater();

    public Transform debug;
    public MeshGenerator vmg;
    delegate int[,,] GenerateData();
    GenerateData genData;

    public MeshFilter mf;
    public bool showScalarField = false;
    public bool useTestData = false;
    public int[,,] testData = new int[2, 2, 2]
                {
                    {
                        {0,0}, // 0-4
                        {0,0}//3-7
                    },
                    {
                        {0,0},//2-6
                        {0,0}//1-5
                    }
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
        }
        else
        {
            genData = () => vdm.GenerateData(DisplaySize, scale, Vector3Int.zero);
        }

        data = genData();
        if (showScalarField)
        {
            visuals = new GameObject[DisplaySize, DisplaySize, DisplaySize];
            DisplayData();
        }



        mf.mesh = vmg.MarchingCubes(data);

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
                    g.GetComponent<Renderer>().material.color = ((Voxel)data[i, j, k]).color;
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
