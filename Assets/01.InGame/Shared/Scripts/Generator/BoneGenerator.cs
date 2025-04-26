using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoneGenerator : MonoBehaviour
{
    public static BoneGenerator Instance;
    
    [Header("Bone Settings")]
    [SerializeField] PlayerBone boneSample;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float boneDistance;
    [SerializeField] private float boneRadius;
    
    private readonly List<PlayerBone> boneList = new List<PlayerBone>();

    private int currentBoneIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GenerateBone();
        }
    }

    public bool TryGetNextBone(out PlayerBone bone)
    {
        bone = null;
        if (currentBoneIndex >= boneList.Count)
            return false;
        
        bone = boneList[currentBoneIndex];
        currentBoneIndex++;
        
        return true;
    }
    
    
    public void GenerateBone()
    {
        PlayerBone bone = Instantiate(boneSample, transform);
        Transform target;
        float distance = boneDistance;
        
        if (boneList.Count == 0)
        {
            // head bone follow player
            target = PlayerController.localPlayer.transform;
            distance = 0.1f;
        }
        else
        {
            var parentBone = boneList.Last();
            target = parentBone.transform;

            parentBone.attached = bone;
        }
        
        bone.movementController.Setup(source: target, boneDistance : distance);
        
        
        boneList.Add(bone);
    }
    
}
