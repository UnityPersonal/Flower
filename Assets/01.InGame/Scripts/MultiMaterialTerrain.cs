using UnityEngine;

public class MultiMaterialTerrain : MonoBehaviour
{
    public Terrain terrain; // 대상 Terrain
    public TerrainLayer[] terrainLayers; // 사용할 TerrainLayer 배열

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain이 설정되지 않았습니다.");
            return;
        }

        if (terrainLayers == null || terrainLayers.Length == 0)
        {
            Debug.LogError("TerrainLayer가 설정되지 않았습니다.");
            return;
        }

        // Terrain에 TerrainLayer 설정
        terrain.terrainData.terrainLayers = terrainLayers;

        Debug.Log("Terrain에 여러 TerrainLayer가 적용되었습니다.");
    }
}