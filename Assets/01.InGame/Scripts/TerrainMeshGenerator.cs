using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainMeshGenerator : MonoBehaviour
{
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private Material grassMaterial;
    [SerializeField] private Material groundMaterial;
    private void Start()
    {
        CreateTerrainMesh();
    }
    void CreateTerrainMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var originalMesh = meshFilter.sharedMesh;

        // 새로운 Mesh 생성
        Mesh newMesh = new Mesh();

        // 기존 Mesh의 데이터 복사
        newMesh.vertices = originalMesh.vertices;
        newMesh.triangles = originalMesh.triangles;
        newMesh.uv = originalMesh.uv;
        newMesh.normals = originalMesh.normals;
        newMesh.tangents = originalMesh.tangents;
        newMesh.colors = originalMesh.colors;

        // SubMesh 데이터 복사
        newMesh.subMeshCount = originalMesh.subMeshCount;
        for (int i = 0; i < originalMesh.subMeshCount; i++)
        {
            newMesh.SetIndices(originalMesh.GetIndices(i), originalMesh.GetTopology(i), i);
        }
        
        List<Vector3> vertices = new List<Vector3>();
        originalMesh.GetVertices(vertices);

        var pixels = heightMap.GetPixels();

        for (int i = 0; i < vertices.Count; i++)
        {
            var wp =  transform.TransformPoint(vertices[i]);
            Vector2 uv = new Vector2(wp.x / heightMap.width, wp.z / heightMap.height);
            uv *= 2f;
            
            Color pixel = heightMap.GetPixelBilinear(uv.x , uv.y);

            var vector3 = vertices[i];
            vector3.y += pixel.a * 60f; // 높이 조정
            vertices[i] = vector3;
        }
        
        newMesh.SetVertices(vertices);
        
        // Bounds 및 기타 데이터 갱신
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();

        // 새로운 Mesh를 MeshFilter에 설정
        meshFilter.mesh = newMesh;
        
    }
}
