using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Rig rig;
    public RigBuilder rigBuilder;
    public BoneRenderer boneRenderer;
    public float rotationSpeed = 60;
    public float moveSpeed = 3f;
    
    public Transform headTransform;
    
    DampedTransform[] dampedTransforms;
    PlayerBone[] playerBones;

    public Petal petalSample;
    public ParticleSystem particlesystem;

    void InitPetal()
    {
        for (int i = 0; i < 100; i++)
        {
            int randI = Random.Range(0, playerBones.Length);
            float3 randomSphere = Random.insideUnitSphere;
            
            var backwardBone = playerBones[randI];
            int fi = randI == 0 ? 0 : randI - 1;
            var forwardBone = playerBones[fi];
            
            // init petal 
            Petal petal = Instantiate(petalSample,Vector3.zero,Quaternion.identity);
            petal.forwardBone = forwardBone;
            petal.backwardBone = backwardBone;

            // init damped transform
            /*var dampObj = new GameObject("PetalDamp");
            var dampT = dampObj.AddComponent<DampedTransform>();
            dampT.data.constrainedObject = petal.transform;
            dampT.data.sourceObject = backwardBone.transform;
            dampT.data.dampPosition = 0.5f;
            dampT.data.dampRotation = 0.5f;
            
            dampObj.transform.SetParent(rig.transform);*/
        }
    }
    
    void Start()
    {
        StartCoroutine(UpdatePath());
        
        dampedTransforms = GetComponentsInChildren<DampedTransform>(true);
        playerBones= GetComponentsInChildren<PlayerBone>(true);
        
        //InitPetal();

        float angleEuler = 0f;
        List<Transform> boneTransforms = new List<Transform>();
        foreach (PlayerBone playerBone in GetComponentsInChildren<PlayerBone>())
        {
            // rotation angle 누적하기
            boneTransforms.Add(playerBone.transform);
            playerBone.currentAngle = angleEuler;
            angleEuler += 90f;
        }
        boneRenderer.transforms = boneTransforms.ToArray();
        
        rigBuilder.Build();
    }
    

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
        UpdateHeadRotation();
    }
    
    void UpdateHeadRotation()
    {
        //headTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
    }

    void UpdateInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(  Vector3.up, horizontal * rotationSpeed * Time.deltaTime  );
        float vertical = Input.GetAxis("Vertical");

        Vector2 inputAxis= new Vector2(horizontal, vertical);
        
        float dampWeight = 0f;
        if (inputAxis.magnitude > 0.01f)
        {
            dampWeight =  inputAxis.magnitude;
            if (particlesystem.isPaused)
            {
                particlesystem.Play();
            }
        }
        else
        {
            particlesystem.Pause();
        }
        
        
        
        foreach (DampedTransform dampedTransform in dampedTransforms)
        {
            dampedTransform.weight = 1;
        }
        
        transform.Translate(Vector3.forward * (vertical * Time.deltaTime * moveSpeed));
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
    
    
}
