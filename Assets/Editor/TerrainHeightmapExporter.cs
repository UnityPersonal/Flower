using UnityEngine;
using UnityEditor;
using System.IO;

public class TerrainHeightmapExporter : EditorWindow
{
    [MenuItem("Tools/Export Terrain Heightmap")]
    public static void ShowWindow()
    {
        GetWindow<TerrainHeightmapExporter>("Terrain Heightmap Exporter");
    }

    private Terrain terrain;

    private void OnGUI()
    {
        GUILayout.Label("Terrain Heightmap Exporter", EditorStyles.boldLabel);

        terrain = EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true) as Terrain;

        if (terrain == null)
        {
            EditorGUILayout.HelpBox("Please assign a Terrain object.", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("Export Heightmap"))
        {
            ExportHeightmap();
        }
    }

    private void ExportHeightmap()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        RenderTexture heightmapRT = terrainData.heightmapTexture;

        if (heightmapRT == null)
        {
            Debug.LogError("Heightmap RenderTexture is not available.");
            return;
        }

        // RenderTexture를 Texture2D로 복사
        RenderTexture.active = heightmapRT;
        Texture2D heightmapTexture = new Texture2D(heightmapRT.width, heightmapRT.height, TextureFormat.R16, false);
        heightmapTexture.ReadPixels(new Rect(0, 0, heightmapRT.width, heightmapRT.height), 0, 0);
        heightmapTexture.Apply();
        RenderTexture.active = null;

        // 에셋으로 저장
        string path = EditorUtility.SaveFilePanelInProject("Save Heightmap as Asset", "Heightmap", "asset", "Select a location to save the heightmap.");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(heightmapTexture, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Heightmap saved as asset at {path}");
        }
        else
        {
            Debug.LogWarning("Save operation canceled.");
        }
    }
}