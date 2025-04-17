using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBoneGenerator : MonoBehaviour
{
    public static PlayerBoneGenerator Instance;
    
    [Header("Bone Settings")]
    [SerializeField] PlayerBone boneSample;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float boneDistance;
    [SerializeField] private float boneRadius;
    
    private readonly List<PlayerBone> boneList = new List<PlayerBone>();

    private int particleMaxCount = 0;
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

    public bool TryGetPlacement(out Vector3 placement)
    {
        if (currentBoneIndex >= boneList.Count)
        {
            placement = Vector3.zero;
            Debug.Log("Fulled Particle");
            return false;
        }
        
        var currentBone =boneList[currentBoneIndex];
        placement = currentBone.placement.GetNextPlacement();
        
        if(currentBone.placement.IsEmpty())
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
            target = PlayerController.localPlayer.transform;
        }
        else
        {
            var parentBone = boneList.Last();
            target = parentBone.transform;

            parentBone.attached = bone;
        }
        
        bone.movementController.Setup(source: target, boneDistance : boneDistance);

        /*if (bone.placement is not null)
        {
            particleMaxCount += bone.placement.particleCount;
            var placement = bone.placement;
            placement.radius = boneRadius;
            placement.heightStep = boneDistance / (float)placement.particleCount;
            placement.BuildPlacement();
        }*/
        
        boneList.Add(bone);
    }
    
}
