using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Lua;
public class LuaMeshConvertTool : EditorWindow
{
    private TextAsset luaScript; // Lua 스크립트 참조
    
    [MenuItem("Tools/Lua mesh convert tool")]
    public static void ShowWindow()
    {
        GetWindow<LuaMeshConvertTool>("Lua mesh convert tool");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Lua Mesh Convert Tool", EditorStyles.boldLabel);

        // Lua 스크립트 선택
        luaScript = (TextAsset)EditorGUILayout.ObjectField("Lua Script", luaScript, typeof(TextAsset), false);

        if (GUILayout.Button("Mesh 생성"))
        {
            if (luaScript == null)
            {
                Debug.LogError("Lua 스크립트를 선택해주세요!");
                return;
            }

            CreateMeshFromLuaScript(luaScript);
        }
            
    }
    
    private void CreateMeshFromLuaScript(TextAsset luaScript)
    {
        // LuaMeshParser의 ParseAll 호출
        List<LuaMeshGroup> meshGroups = LuaMeshParser.ParseAll(luaScript.text);

        if (meshGroups == null || meshGroups.Count == 0)
        {
            Debug.LogError("Mesh 데이터를 생성할 수 없습니다.");
            return;
        }

        // 생성된 Mesh 데이터를 처리
        foreach (LuaMeshGroup group in meshGroups)
        {
            Debug.Log($"{group.name}");
            foreach (LuaSubMeshData subMesh in group.subMeshes)
            {
                Debug.Log($"{subMesh.name}, {subMesh.vertices.Count}");
                
                /*Mesh newMesh = new Mesh
                {
                    vertices = subMesh.GetVertices(),
                    triangles = subMesh.GetTriangles()
                };

                newMesh.RecalculateBounds();
                newMesh.RecalculateNormals();
                newMesh.RecalculateTangents();

                // Mesh를 에셋으로 저장
                SaveMeshAsset(newMesh, $"{group.name}_{subMesh.name}");*/
            }
        }

        Debug.Log("Mesh 생성이 완료되었습니다.");
    }
    
    private void SaveMeshAsset(Mesh mesh, string assetName)
    {
        if (mesh == null)
        {
            Debug.LogError("저장할 Mesh가 없습니다.");
            return;
        }

        string path = $"Assets/GeneratedMeshes/{assetName}.asset";

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Mesh가 {path} 경로에 저장되었습니다.");
    }

}
