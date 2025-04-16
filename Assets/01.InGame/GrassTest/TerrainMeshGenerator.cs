using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainMeshGenerator : MonoBehaviour
{
    private void Start()
    {
        CreateTerrainMesh();
    }
    void CreateTerrainMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;

        // get vertices
        // 1. Get the vertices of the mesh
        List<Vector3> vertices = new List<Vector3>();
        mesh.GetVertices(vertices);
        
        // 2. Get the UVs of the mesh
        List<Vector2> uvs = new List<Vector2>();
        mesh.GetUVs(0, uvs);
        
        for (int i = 0 ; i < vertices.Count ; i++)
        {
            Vector3 v = vertices[i];
            var worldPos =  transform.TransformPoint(v);
            v.y += worldPos.magnitude / 100f;
            vertices[i] = v;
        }
        
        mesh.SetVertices(vertices);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}
