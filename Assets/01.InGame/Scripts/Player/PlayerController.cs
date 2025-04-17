using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;


public class PathLog
{
    public PathLog toNext;
    
    public Vector3 Position;
    public Vector3 Direction;
}

public class PlayerController : MonoBehaviour
{
    public static PlayerController localPlayer;
    public float rotationSpeed = 60;
    public float moveSpeed = 3f;

    [Header("Movement Setting")]
    [SerializeField] private float moveSpeedMin = 1f;
    [SerializeField] private float moveSpeedMax = 10f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float currentSpeed;
    
    public float Speed => currentSpeed;
    public float NormalizedSpeed => (currentSpeed - moveSpeedMin) / (moveSpeed - moveSpeedMin);
    public Vector2 SpeedRange => new Vector2(moveSpeedMin, moveSpeedMax);
    
    public Transform headTransform;
    
    [Header("Solver Setting")]
    [SerializeField] private  ObiSolver obiSolver;
    [SerializeField] private float windStrength = 1f;
    [SerializeField] private float gravityStrength = 1f;
    
    [Header("Petal settings")]
    [SerializeField] private  Petal petalSample;
    [SerializeField] private  ParticleSystem particlesystem;

    [SerializeField] private float petalSpawnRadius = 1f;
    
    public Collider mainCollider { get; private set; }

    [Header("Bone Settings")]
    
    [SerializeField] private PlayerBone boneSample;

    [SerializeField] private float boneDistance = 2f;
    private readonly List<PlayerBone> playerBones = new List<PlayerBone>();
    
    [Header("Rig Setting")]
    public Rig rig;
    public RigBuilder rigBuilder;
    public BoneRenderer boneRenderer;
    
    [Header("Damper Settings")]
    [SerializeField] private BoneDamper boneDamperSample;
    
    private void Awake()
    {
        localPlayer = this;
        mainCollider = GetComponent<Collider>();
    }

    void Start()
    {
        for(int i = 0 ;  i < 20; i++)
            AddBone();
        RebuildBoneRenderer();

        for (int i = 0; i < particleMaxCount; i++)
        {
            AddPetal_v3();
        }
        
        
        StartCoroutine(UpdatePath());
    }
    
    void Update()
    {
        UpdateInput();
        
        UpdateRotation();
        UpdateMovement();
        
        UpdateWind();
        UpdateGravity();

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddPetal_v3();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            var petals = GetComponentsInChildren<Petal>(true);
            foreach (var petal in petals)
            {
                Destroy(petal.gameObject);
            }
        }
    }

    void UpdateGravity()
    {
        obiSolver.gravity = -transform.forward * (9.8f * gravityStrength);
    }
    
    void UpdateWind()
    {
        var forward = -transform.forward * inputAxis.y;
        var right = -transform.right * inputAxis.x;
        
        obiSolver.ambientWind = (forward + right).normalized * windStrength;
    }
    

    public Vector2 inputAxis;

    void UpdateInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        inputAxis= new Vector2(horizontal, vertical);
    }

    void UpdateInteractionEffect(Vector2 inputAxis)
    {
        if (inputAxis.magnitude > 0.01f)
        {
            if (particlesystem.isPaused)
            {
                particlesystem.Play();
            }
        }
        else
        {
            particlesystem.Pause();
        }
    }

    void UpdateRotation()
    {
        transform.Rotate(  Vector3.up, inputAxis.x * rotationSpeed * Time.deltaTime  );
    }

    void UpdateMovement()
    {
        if (Input.GetKey(KeyCode.Space))
        {
             currentSpeed += acceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed -= acceleration * Time.deltaTime;
        }
        
        currentSpeed = Mathf.Clamp(currentSpeed, moveSpeedMin, moveSpeedMax);
        transform.Translate(Vector3.forward * (currentSpeed * Time.deltaTime));
    }

    public int maxLogCount = 50;
    List<PathLog> logs = new List<PathLog>();

    public PathLog CreatePathLog()
    {
        PathLog log = new PathLog();
        log.Position = transform.position;
        log.Direction = transform.forward;
        return log;
    }

    IEnumerator UpdatePath()
    {
        while (gameObject.activeInHierarchy)
        {
            PathLog log = CreatePathLog();
            if (logs.Count >= maxLogCount)
            {
                logs.RemoveAt(0);
            }
            logs.Add(log);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnDrawGizmos()
    {
        if (logs.Count > 0)
        {
            Gizmos.color = Color.green;
            for(int i = 0; i < logs.Count - 1; i++)
            {
                Gizmos.DrawLine(logs[i].Position + Vector3.up, logs[i + 1].Position+ Vector3.up);
            }
            Gizmos.DrawLine(logs.Last().Position + Vector3.up, transform.position+ Vector3.up);
        }
    }
    
    void RebuildBoneRenderer()
    {
        List<Transform> boneTransforms = new List<Transform>(playerBones.Count);
        foreach (var bone in playerBones)
        {
            boneTransforms.Add(bone.transform);
        }
        boneRenderer.transforms = boneTransforms.ToArray();
        rigBuilder.Build();
    }

    private int particleMaxCount = 0;
    public void AddBone()
    {
        PlayerBone bone;
        if (playerBones.Count == 0)
        {
            bone = Instantiate(boneSample, transform);
            
        }
        else
        {
            var parent = playerBones.Last();
            bone = Instantiate(boneSample, parent.transform);
            bone.transform.localPosition = Vector3.back * boneDistance;
            
            var damper = Instantiate(boneDamperSample, rig.transform);
            damper.Setup(source : parent.transform, constrained: bone.transform);
        }

        particleMaxCount += bone.placement.particleCount;
        
        playerBones.Add(bone);
        var placement = bone.placement;
        placement.radius = petalSpawnRadius;
        placement.heightStep = boneDistance / (float)placement.particleCount;
        placement.BuildPlacement();
        
        RebuildBoneRenderer();
    }
    
    private int currentBoneIndex;
    public void AddPetal_v3()
    {
        if (currentBoneIndex >= playerBones.Count)
        {
            Debug.Log("Full Particle");
            return;
        }
        
        var currentBone = playerBones[currentBoneIndex];
        
        Vector3 localposition;
        if (currentBone.placement.TryGetPosition(out localposition) == false)
        {
            currentBoneIndex++;
            
            AddPetal_v3();
            return;
        }
        
        currentBone.transform.localPosition = localposition;
        RotateBone rotateBone = currentBone.rotateBone;
        var petal = Instantiate(petalSample, rotateBone.transform);
        petal.transform.localPosition = localposition;
        petal.transform.forward = (rotateBone.transform.position - petal.transform.position).normalized;
    }
    
}
