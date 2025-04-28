using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoneGenerator : MonoBehaviour , ILoadable
{
    public static BoneGenerator Instance;
    
    [Header("Bone Settings")]
    [SerializeField] PlayerBone boneSample;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float boneDistance;
    [SerializeField] private float boneRadius;
    
    [SerializeField] private int initPoolSize = 50;
    
    private readonly List<PlayerBone> boneList = new List<PlayerBone>();

    private int currentBoneIndex = 0;

    private void Awake()
    {
        Instance = this;

        for(int i = 0; i < initPoolSize; i++)
        {
            GenerateBone();
        }

        OnLoadComplete();
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
        {
            GenerateBone();
        }
        
        bone = boneList[currentBoneIndex];
        currentBoneIndex++;
        
        return true;
    }
    
    
    public void GenerateBone()
    {
        PlayerBone bone = Instantiate(boneSample, transform);
        Transform target;
        
        if (boneList.Count == 0)
        {
            // head bone follow player
            target = PlayerController.LocalPlayer.headTransform;
            bone.isRoot = true;
        }
        else
        {
            var parentBone = boneList.Last();
            target = parentBone.transform;

            parentBone.attached = bone;
        }
        
        bone.movementController.Setup(source: target);
        boneList.Add(bone);
    }

    public void OnLoadComplete()
    {
        GameManager.Instance.OnLoadComplete(this); 
    }
}
