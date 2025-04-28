using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class PetalGenerator : MonoBehaviour , ILoadable
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
    [SerializeField] private PetalRope petalRopeSample;

    private readonly Dictionary<Petal.Type, Petal> petalsDict = new Dictionary<Petal.Type, Petal>();


    [Header("Pool Settings")]
    private int initPoolSize = 100;
    Dictionary<Petal, Queue<Petal>> petalsPool = new Dictionary<Petal, Queue<Petal>>();
    Queue<PetalRope> ropePool = new Queue<PetalRope>();
        
    private float angle;
    PlayerBone currentBone;
    int particleCount = 0;
    private void Awake()
    {
        Instance = this;

        foreach (var sample in petalSamples)
        {
            petalsDict.Add(sample.PetalType, sample);
            petalsPool.Add(sample, new Queue<Petal>());

            for (int i = 0; i < initPoolSize; i++)
            {
                Create(sample);
            }
        }
        for (int i = 0; i < initPoolSize; i++)
        {
            CreateRope();
        }

        OnLoadComplete();
    }

    private Petal Get(Petal.Type type)
    {
        Petal sample = GetSample(type);
        var pool = petalsPool[sample];
        if (pool.Count == 0)
        {
            Create(sample);
        }
        Petal petal = pool.Dequeue();
        petal.gameObject.SetActive(true);
        return petal;
    }

    private void Create(Petal sample)
    {
        var pool = petalsPool[sample];
        // create petal
        Petal petal = Instantiate(sample, transform);
        petal.gameObject.SetActive(false);
        pool.Enqueue(petal);
    }

    private PetalRope GetRope()
    {
        if(ropePool.Count == 0)
            CreateRope();
        
        PetalRope rope = ropePool.Dequeue();
        rope.gameObject.SetActive(true);
        return rope;
    }

    private void CreateRope()
    {
        PetalRope rope = Instantiate(petalRopeSample, transform);
        rope.gameObject.SetActive(false);
        ropePool.Enqueue(rope);
    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // DOTO:: 테스트용 삭제 필요
            GeneratePetal(Petal.Type.Unknown, PlayerController.LocalPlayer.transform.position);
        }
    }

    Petal GetSample(Petal.Type type)
    {
        return petalsDict[type];
    }


    public bool GeneratePetal(Petal.Type type, Vector3 spawnPosition)
    {
        var boneManager = BoneGenerator.Instance;
        
        // get bone to attach petal
        if (currentBone is null || particleCount == particleCountMax + 1)
        {
            boneManager.TryGetNextBone(out currentBone);
            //Debug.Log("Get Next bone");
            particleCount = 0;
        }
        
        PetalRope petalRope = GetRope();
        petalRope.transform.SetParent(currentBone.transform);
        petalRope.bone = currentBone;
        petalRope.normalizePosition = particleCount / (float)particleCountMax;
        
        angle += Random.Range(angleMinStep,angleMaxStep) * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        petalRope.transform.localPosition = new Vector3(x, y, 0);
        
        // create petal
        Petal petal = Get(type);
        petal.rope =petalRope;
        petal.transform.position = spawnPosition;
        
        particleCount++;
        
        GameManager.Instance.OnGeneratedPetal();
        events.OnGeneratedPetal?.Invoke();
        return true;
    }


    public void OnLoadComplete()
    {
        GameManager.Instance.OnLoadComplete(this); 
    }
}
