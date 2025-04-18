using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

namespace Lua
{
    using UnityEngine;
    using System.Collections.Generic;

    [System.Serializable]
    public class LuaMeshVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public LuaMeshVertex(float[] values)
        {
            position = new Vector3(values[0], values[1], values[2]);
            normal = new Vector3(values[3], values[4], values[5]);
            uv = new Vector2(values[6], values[7]);
            
            Debug.Log($"{position}, {normal}, {uv}");
        }
    }

    [System.Serializable]
    public class LuaSubMeshData
    {
        public string name;
        public List<LuaMeshVertex> vertices = new List<LuaMeshVertex>();

        public Vector3[] GetVertices()
        {
            Vector3[] ret = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                ret[i] = vertices[i].position;
            }
            return ret;
        }

        public int[] GetTriangles()
        {
            return triangles.ToArray();
        }
        
        public List<int> triangles = new List<int>();
    }

    [System.Serializable]
    public class LuaMeshGroup
    {
        public string name;
        public List<LuaSubMeshData> subMeshes = new List<LuaSubMeshData>();
    }

    public class LuaMeshParser
{
    public static List<LuaMeshGroup> ParseAll(string luaText)
    {
        List<LuaMeshGroup> meshGroups = new List<LuaMeshGroup>();
        LuaMeshGroup currentGroup = null;
        LuaSubMeshData currentLuaSubMesh = null;

        bool readingVertices = false;
        bool readingTriangles = false;

        using (StringReader reader = new StringReader(luaText))
        {
            string line;
            
            
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                // 새 MeshGroup 시작
                if (line.StartsWith("{ \""))
                {
                    if (currentGroup != null)
                        meshGroups.Add(currentGroup);

                    string meshName = line.Split('\"')[1];
                    currentGroup = new LuaMeshGroup { name = meshName };
                    continue;
                }

                // 새 SubMesh 시작
                if (line.StartsWith(", { \""))
                {
                    if (currentLuaSubMesh != null)
                        currentGroup?.subMeshes.Add(currentLuaSubMesh);

                    string subMeshName = line.Split('\"')[1];
                    currentLuaSubMesh = new LuaSubMeshData { name = subMeshName };
                    readingVertices = true;
                    Debug.Log("Start Reading Vertices");
                    readingTriangles = false;
                    continue;
                }

                // 서브메쉬 끝나고 인덱스 시작
                if (line.StartsWith("},"))
                {
                    Debug.Log("Start Reading Triangels");

                    readingVertices = false;
                    readingTriangles = true;
                    continue;
                }

                // Vertex 파싱
                if (readingVertices && line.StartsWith("{"))
                {
                    
                    Debug.Log(line);

                    string[] parts = line.Replace("{", "").Replace("}", "").Split(',');
                    float[] values = Array.ConvertAll(parts, float.Parse);
                    currentLuaSubMesh?.vertices.Add(new LuaMeshVertex(values));
                    
                }

                // Triangle 인덱스 파싱
                if (readingTriangles && char.IsDigit(line[0]))
                {
                    string[] parts = line.Split(',');
                    foreach (string part in parts)
                    {
                        if (int.TryParse(part.Trim(), out int index))
                            currentLuaSubMesh?.triangles.Add(index);
                    }
                }
            }

            // 마지막 submesh/group 저장
            if (currentLuaSubMesh != null)
                currentGroup?.subMeshes.Add(currentLuaSubMesh);
            if (currentGroup != null)
                meshGroups.Add(currentGroup);
        }

        return meshGroups;
    }
}
    
}

