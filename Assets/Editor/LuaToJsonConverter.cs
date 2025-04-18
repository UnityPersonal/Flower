using UnityEngine;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor.Rendering;

public class LuaToJsonConverter
{
    public static void ConvertLuaToJson(TextAsset textAsset)
    {
        
        // Lua 스크립트 실행
        Script lua = new Script();
        DynValue luaTable = lua.DoFile("FlowerMeshes");
        Debug.Log( luaTable.Type);
        if (luaTable.Table is null)
        {
            Debug.LogError($"Failed to convert lua script to json");
        }
        
        // Lua 테이블을 C# Dictionary로 변환
        Dictionary<string, object> tableDict = ConvertLuaTable(luaTable.Table);
        
        // JSON 문자열로 변환
        string json = JsonConvert.SerializeObject(tableDict, Formatting.Indented);

        // 출력 확인
        Debug.Log(json);
    }

    static Dictionary<string, object> ConvertLuaTable(Table table)
    {
        Debug.unityLogger.Log(table.Length);
        Dictionary<string, object> dict = new Dictionary<string, object>();

        
        foreach (var pair in table.Pairs)
        {
            string key = pair.Key.ToString();

            if (pair.Value.Type == DataType.Table)
            {
                dict[key] = ConvertLuaTable(pair.Value.Table);
            }
            else
            {
                dict[key] = pair.Value.ToObject();
            }
        }

        return dict;
    }
}