using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PetalGenerator : MonoBehaviour
{
    public static PetalGenerator Instance;

    public class Events
    {
        public Action OnGeneratedPetal;
    }
    
    public readonly Events events = new Events();

    [Header("Petal settings")]
    [SerializeField] private float particleCountMax = 10;
    [SerializeField] private float radius = 1f;
    
    public float heightStep = 0.1f; // 높이 증가량
    public float angleStep = 20f; // 각도 증가량 (도 단위)
    public float angleMinStep = 20f;
    public float angleMaxStep = 20f;
    public float petalFollowSpeed;
    [Header("Material Settings")] 
    [SerializeField] private Petal[] petalSamples;

    private readonly Dictionary<Petal.Type, Petal> petalsDict = new Dictionary<Petal.Type, Petal>();
    

    private float angle;
    PlayerBone currentBone;
    int particleCount = 0;
    private void Awake()
    {
        Instance = this;

        foreach (var sample in petalSamples)
        {
            petalsDict.Add(sample.PetalType, sample);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // DOTO:: 테스트용 삭제 필요
            GeneratePetal(Petal.Type.Unknown);
        }
    }

    Petal GetSample(Petal.Type type)
    {
        return petalsDict[type];
    }


    public bool GeneratePetal(Petal.Type type)
    {
        var boneManager = BoneGenerator.Instance;
        
        // get bone to attach petal
        if (currentBone is null || particleCount == particleCountMax + 1)
        {
            if (boneManager.TryGetNextBone(out currentBone) == false)
            {
                return false;
            }

            Debug.Log("Get Next bone");
            particleCount = 0;
        }
        
        // create petal
        Petal petal = Instantiate(petalsDict[type], currentBone.transform);
        
        angle += angleStep * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        petal.normalizePosition = particleCount / (float)particleCountMax;
        petal.transform.localPosition = new Vector3(x, y, 0);
        petal.bone = currentBone;
        petal.petalFollowSpeed = petalFollowSpeed;
        
        particleCount++;
        
        events.OnGeneratedPetal?.Invoke();

        return true;
    }    

    
}
