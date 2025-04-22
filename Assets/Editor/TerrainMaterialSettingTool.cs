using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainMaterialSettingTool : EditorWindow
{
    public Terrain[] terrains;
    Terrain terrain;
    Material material;
    
    [MenuItem("Tools/Terrain Material Setting Tool")]
    public static void ShowWindow()
    {
        GetWindow<TerrainMaterialSettingTool>("Auto Terrain Generate Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("지형 메터리얼 일괄 수정", EditorStyles.boldLabel);
        
        if (GUILayout.Button("씬에서 Terrain 읽어오기"))
        {
            LoadTerrainsFromScene();
        }
        
        material = (Material)EditorGUILayout.ObjectField("적용할 메터리얼", material, typeof(Material), false);

        if (GUILayout.Button("메터리얼 적용"))
        {
            ApplyMaterialToTerrains();
        }
    }
    
    private void LoadTerrainsFromScene()
    {
        // 씬에 배치된 모든 Terrain 컴포넌트를 찾아 배열에 저장
        terrains = Object.FindObjectsOfType<Terrain>();
        Debug.Log($"씬에서 {terrains.Length}개의 Terrain을 발견했습니다.");
    }
    
    private void ApplyMaterialToTerrains()
    {
        if (terrains == null || terrains.Length == 0)
        {
            Debug.LogError("적용할 Terrain이 없습니다. 먼저 Terrain을 읽어오세요.");
            return;
        }

        if (material == null)
        {
            Debug.LogError("적용할 메터리얼을 설정해주세요.");
            return;
        }

        foreach (var terrain in terrains)
        {
            terrain.materialTemplate = material;
        }

        Debug.Log("모든 Terrain에 메터리얼이 적용되었습니다.");
    }
    
}
