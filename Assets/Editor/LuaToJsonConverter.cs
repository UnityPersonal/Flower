using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using Newtonsoft.Json;

public class LuaToJsonConverter : EditorWindow
{
    private TextAsset luaFile;

    [MenuItem("Tools/Lua to JSON Converter")]
    public static void ShowWindow()
    {
        GetWindow<LuaToJsonConverter>("Lua to JSON Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Lua to JSON Converter", EditorStyles.boldLabel);

        luaFile = (TextAsset)EditorGUILayout.ObjectField("Lua File", luaFile, typeof(TextAsset), false);

        if (GUILayout.Button("Convert to JSON"))
        {
            if (luaFile == null)
            {
                Debug.LogError("Lua 파일을 선택해주세요!");
                return;
            }

            ConvertLuaToJson(luaFile);
        }
    }

    private void ConvertLuaToJson(TextAsset luaFile)
    {
        string luaContent = luaFile.text;

        // Lua 데이터 파싱 (간단한 파싱 로직)
        var flowerMeshes = ParseLuaData(luaContent);

        if (flowerMeshes == null || flowerMeshes.Count == 0)
        {
            Debug.LogError("Lua 데이터를 파싱할 수 없습니다.");
            return;
        }

        // JSON 데이터 저장
        SaveJsonData(flowerMeshes, "MeshNames.json", "BlendData.json", "TriangleIndices.json");

        Debug.Log("JSON 변환이 완료되었습니다.");
    }

    private List<FlowerMeshData> ParseLuaData(string luaContent)
    {
        // Lua 데이터를 파싱하여 FlowerMeshData 리스트로 변환
        var flowerMeshes = new List<FlowerMeshData>();

        // 간단한 파싱 로직 (실제 구현 시 Lua 파싱 라이브러리 사용 권장)
        // 예제에서는 데이터 구조를 기반으로 수동 파싱
        string[] lines = luaContent.Split('\n');
        FlowerMeshData currentMesh = null;

        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("{ \""))
            {
                if (currentMesh != null)
                {
                    flowerMeshes.Add(currentMesh);
                }

                currentMesh = new FlowerMeshData
                {
                    Name = line.Trim().Split('\"')[1],
                    BlendData = new Dictionary<string, List<List<float>>>(),
                    TriangleIndices = new List<List<int>>()
                };
            }
            else if (line.Trim().StartsWith("{ \"blend"))
            {
                string blendName = line.Trim().Split('\"')[1];
                currentMesh.BlendData[blendName] = new List<List<float>>();
            }
            else if (line.Trim().StartsWith("{") && currentMesh != null)
            {
                string[] values = line.Trim().Trim('{', '}').Split(',');
                if (values.Length > 3) // 블렌드 데이터
                {
                    var blendValues = new List<float>();
                    foreach (var value in values)
                    {
                        blendValues.Add(float.Parse(value.Trim()));
                    }

                    string lastBlend = null;
                    foreach (var key in currentMesh.BlendData.Keys)
                    {
                        lastBlend = key;
                    }

                    currentMesh.BlendData[lastBlend].Add(blendValues);
                }
                else // 삼각형 인덱스 데이터
                {
                    var triangleValues = new List<int>();
                    foreach (var value in values)
                    {
                        triangleValues.Add(int.Parse(value.Trim()));
                    }

                    currentMesh.TriangleIndices.Add(triangleValues);
                }
            }
        }

        if (currentMesh != null)
        {
            flowerMeshes.Add(currentMesh);
        }

        return flowerMeshes;
    }

    private void SaveJsonData(List<FlowerMeshData> flowerMeshes, string meshNamesFile, string blendDataFile, string triangleIndicesFile)
    {
        // 메쉬 이름 저장
        var meshNames = new List<string>();
        foreach (var mesh in flowerMeshes)
        {
            meshNames.Add(mesh.Name);
        }

        File.WriteAllText(Path.Combine(Application.dataPath, meshNamesFile), JsonConvert.SerializeObject(meshNames, Formatting.Indented));

        // 블렌드 데이터 저장
        var blendData = new Dictionary<string, Dictionary<string, List<List<float>>>>();
        foreach (var mesh in flowerMeshes)
        {
            blendData[mesh.Name] = mesh.BlendData;
        }

        File.WriteAllText(Path.Combine(Application.dataPath, blendDataFile), JsonConvert.SerializeObject(blendData, Formatting.Indented));

        // 삼각형 인덱스 데이터 저장
        var triangleIndices = new Dictionary<string, List<List<int>>>();
        foreach (var mesh in flowerMeshes)
        {
            triangleIndices[mesh.Name] = mesh.TriangleIndices;
        }

        File.WriteAllText(Path.Combine(Application.dataPath, triangleIndicesFile), JsonConvert.SerializeObject(triangleIndices, Formatting.Indented));
    }

    private class FlowerMeshData
    {
        public string Name { get; set; }
        public Dictionary<string, List<List<float>>> BlendData { get; set; }
        public List<List<int>> TriangleIndices { get; set; }
    }
}