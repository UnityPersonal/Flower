using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateGrassBladeMeshTool : EditorWindow
{
    [MenuItem("Tools/Generate Grass Blade Mesh")]
    public static void ShowWindow()
    {
        GetWindow<GenerateGrassBladeMeshTool>("Generate Grass Blade Mesh");
    }

    private void OnGUI()
    {
        GUILayout.Label("잔디 매쉬 생성", EditorStyles.boldLabel);
        if (GUILayout.Button("생성 및 저장"))
        {
            CreateBladeMesh();
        }
    }

    private void CreateBladeMesh()
    {
        int segments = 8; // 세그먼트 수
        float height = 1.0f; // 잔디 블레이드 높이
        float width = 0.5f; // 잔디 블레이드 너비

        Mesh grassBladeMesh = new Mesh();

        // 정점 및 삼각형 리스트
        List<int> triangles = new List<int>();
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        // 정점 생성
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float y = t * height;
            float xOffset = Mathf.Lerp(width, 0, t); // 위로 갈수록 좁아짐

            vertices.Add(new Vector3(-width, y, 0)) ; // 왼쪽 정점
            vertices.Add(new Vector3(width, y, 0)) ; // 오른쪽 정점

            uvs.Add(new Vector2(0, t)); // 왼쪽 UV
            uvs.Add(new Vector2(1, t)); // 오른쪽 UV
        }

        // 삼각형 생성
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 2;

            // 첫 번째 삼각형
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);

            // 두 번째 삼각형
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
        }
        

        // 메시 설정
        grassBladeMesh.vertices = vertices.ToArray();
        grassBladeMesh.triangles = triangles.ToArray();
        grassBladeMesh.uv = uvs.ToArray();

        grassBladeMesh.RecalculateNormals();
        grassBladeMesh.RecalculateBounds();

        // 메시 저장
        SaveMeshAsset(grassBladeMesh, "GrassBladeMesh");
    }
    
    private void SaveMeshAsset(Mesh mesh, string assetName)
    {
        if (mesh == null)
        {
            Debug.LogError("저장할 Mesh가 없습니다.");
            return;
        }

        string path = $"Assets/{assetName}.asset";
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Mesh가 {path} 경로에 저장되었습니다.");
    }

}
