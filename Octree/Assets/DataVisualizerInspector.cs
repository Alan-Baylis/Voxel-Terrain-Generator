﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataVisualizer))]

public class DataVisualizerInspector : Editor
{
    DataVisualizer dv;

    void ApplyVertexToggle()
    {
        GUILayout.Label("Test Data Vertices");
        for (int i = 0; i < dv.testData.GetLength(0); i++)
        {
            GUILayout.BeginHorizontal(i.ToString(), GUILayout.Height(10));

            for (int j = 0; j < dv.testData.GetLength(1); j++)
            {
                for (int k = 0; k < dv.testData.GetLength(2); k++)
                {

                    //Debug.Log(string.Format("x{0},y{1},z{2}", i, j, k));
                    dv.testData[k, j, i] = (Voxel)((GUILayout.Toggle(dv.testData[k, j, i] == Voxel.FILLED, "")) ? 1 : 0);
                }
            }
            GUILayout.EndHorizontal();
        }
    }

  

    void OnEnable()
    {
        dv = (DataVisualizer)target;
    }

    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        if (dv.useTestData)
        {
            ApplyVertexToggle();
        }



        if (!Application.isPlaying) return;

        if (GUILayout.Button("Visualize Data"))
        {
            dv.ResetVisuals();
            dv.DisplayData();
        }

        if (GUILayout.Button("Delete Visuals"))
        {
            dv.ResetVisuals();
        }

        if (GUILayout.Button("Reset"))
        {
            dv.ResetVisuals();
            dv.Init();
        }
    }

}