using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRigBuidTool : EditorWindow
{
    private Transform root;
    private GameObject playerController;
    private GameObject bonePrefab;
    private GameObject damperPrefab;
    
    private float branchLengthMin = 2.0f; // 가지 길이
    private float branchLengthMax = 2.0f; // 가지 길이
    private float branchAngle = 30.0f; // 가지 각도
    
    [MenuItem("Tools/Auto Rig Build Tool")]
    public static void ShowWindow()
    {
        GetWindow<PlayerRigBuidTool>("Auto Player Rig Build Tool");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("플레이어 본 구조 자동 생성", EditorStyles.boldLabel);
        
        playerController = (GameObject)EditorGUILayout.ObjectField("플레이어 컨트롤러", playerController, typeof(GameObject), true);
        bonePrefab = (GameObject)EditorGUILayout.ObjectField("본 프리팹:", bonePrefab, typeof(GameObject), false);
        damperPrefab = (GameObject)EditorGUILayout.ObjectField("댐프 프리팹:", damperPrefab, typeof(GameObject), false);

        branchLengthMin = EditorGUILayout.FloatField("가지 길이", branchLengthMin);
        branchLengthMax = EditorGUILayout.FloatField("가지 길이", branchLengthMax);
        branchAngle = EditorGUILayout.FloatField("가지 각도", branchAngle);
        
        if (GUILayout.Button("본 생성"))
        {
            CreateAndConnectObjects();
        }
    }
    
    private void CreateAndConnectObjects()
    {
        
        if (playerController == null)
        {
            Debug.LogError("플레이어를 설정해주세요!");
            return;
        }
        
        if (bonePrefab == null)
        {
            Debug.LogError("본 프리팹을 설정해주세요!");
            return;
        }
        
        if (damperPrefab == null)
        {
            Debug.LogError("댐프 프리팹을 설정해주세요!");
            return;
        }
        
        PlayerController player= playerController.GetComponent<PlayerController>();
        List<Transform> boneList = new List<Transform>();
        boneList.Add(player.headTransform);
        
        var rig =  player.rig;

        for (int i = 0; i < 100; i++)
        {
            int boneCount = boneList.Count;
            int parentIndex = Random.Range(0, boneCount);
            Transform parent = boneList[parentIndex];
            
            Vector2 randCircle = Random.insideUnitCircle;
            // 본 생성
            GameObject bone = Instantiate(bonePrefab,parent);
            // 댐프 생성
            DampedTransform damper = Instantiate(damperPrefab, rig.transform).GetComponent<DampedTransform>();
            damper.data.sourceObject = parent;
            damper.data.constrainedObject = bone.transform;
            float randAngle = Random.Range(0,branchAngle);
            
            // 위치 및 회전 설정
            Vector3 branchDirection = Quaternion.Euler(0, randAngle, 0) * Vector3.back;
            
            float branchLength = Random.Range(branchLengthMin, branchLengthMax);
            bone.transform.localPosition = branchDirection * branchLength;
            
            boneList.Add(bone.transform);
        }
        
        List<Transform> renderBones = new List<Transform>();
        foreach (PlayerBone playerBone in player.GetComponentsInChildren<PlayerBone>())
        {
            // rotation angle 누적하기
            renderBones.Add(playerBone.transform);
        }
        player.boneRenderer.transforms = renderBones.ToArray();
        player.rigBuilder.Build();
    }

    
}
