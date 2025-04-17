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
    
    public Transform headTransform;
    
    [Header("Solver Setting")]
    [SerializeField] private  ObiSolver obiSolver;
    [SerializeField] private float windStrength = 1f;

    
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
    
    private PlayerBone currentBone;
    
    private List<RotateBone> rotateBones = new List<RotateBone>();

    

    private void Awake()
    {
        localPlayer = this;
        mainCollider = GetComponent<Collider>();
    }

    void Start()
    {
        AddBone();
        StartCoroutine(UpdatePath());
    }
    
    void Update()
    {
        UpdateInput();
        UpdateHeadRotation();
        UpdateWind();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddBone();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddPetal();
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
    
    void UpdateWind()
    {
        var forward = -transform.forward * inputAxis.y;
        var right = -transform.right * inputAxis.x;
        
        obiSolver.ambientWind = (forward + right).normalized * windStrength;
    }
    
    void UpdateHeadRotation()
    {
        //headTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
    }

    public Vector2 inputAxis;

    void UpdateInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        transform.Rotate(  Vector3.up, horizontal * rotationSpeed * Time.deltaTime  );

        inputAxis= new Vector2(horizontal, vertical);
        
        float dampWeight = 0f;
        if (inputAxis.magnitude > 0.01f)
        {
            dampWeight =  inputAxis.magnitude;
        }
        //UpdateInteractionEffect(inputAxis);
        
        transform.Translate(Vector3.forward * (vertical * Time.deltaTime * moveSpeed));
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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Input.GetAxis("Mouse X");
        Input.GetAxis("Mouse Y");

        Vector2 inputAxis= new Vector2(horizontal, vertical);
        
        transform.Rotate(  Vector3.up, horizontal * rotationSpeed * Time.deltaTime  );
        
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
    
    public void AddBone()
    {
        if (playerBones.Count == 0)
        {
            var bone = Instantiate(boneSample, transform);
            
            rotateBones.Add(bone.rotateBone);
            playerBones.Add(bone);
        }
        else
        {
            var parent = playerBones.Last();
            var bone = Instantiate(boneSample, parent.transform);
            bone.transform.localPosition = Vector3.back * boneDistance;
            
            rotateBones.Add(bone.rotateBone);
            playerBones.Add(bone);
            
            var damper = Instantiate(boneDamperSample, rig.transform);
            damper.Setup(source : parent.transform, constrained: bone.transform);
        }
        
        RebuildBoneRenderer();
    }
    
    public void AddPetal()
    {
        int randIndex =  Random.Range(0, rotateBones.Count);
        Vector2 randsCircle = Random.insideUnitCircle;
        
        RotateBone rotateBone = rotateBones[randIndex];
        var petal = Instantiate(petalSample, rotateBone.transform);
        
        Vector3 localposition = Vector3.back * (Random.Range(0.0f,1f) * boneDistance);
        localposition += Vector3.right * (randsCircle.x * petalSpawnRadius); 
        localposition += Vector3.up * (randsCircle.y * petalSpawnRadius); 
        
        petal.transform.localPosition = localposition;
    }
    
}
