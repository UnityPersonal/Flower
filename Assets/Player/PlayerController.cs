using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;


public class PlayerController : MonoBehaviour
{
    public Rig rig;
    public RigBuilder rigBuilder;
    public BoneRenderer boneRenderer;
    public float rotationSpeed = 60;
    public float moveSpeed = 3f;
    
    DampedTransform[] dampedTransforms;
    PlayerBone[] playerBones;

    public Petal petalSample;
    

    void Start()
    {
        StartCoroutine(UpdatePath());
        
        dampedTransforms = GetComponentsInChildren<DampedTransform>(true);
        playerBones= GetComponentsInChildren<PlayerBone>(true);
        
        
        for (int i = 0; i < 100; i++)
        {
            int randI = Random.Range(0, playerBones.Length);
            float3 randomSphere = Random.insideUnitSphere;
            
            Transform bone = playerBones[randI].transform;
            // init petal 
            Petal petal = Instantiate(petalSample, bone);
            Vector3 localOffset = bone.up * randomSphere.y;
            localOffset += bone.right * randomSphere.x;
            localOffset += -bone.forward * Mathf.Abs(randomSphere.z) * 2f;
            
            petal.transform.localPosition = localOffset;
            petal.toFollow = bone;
            
            

            // init damped transform
            var dampObj = new GameObject("PetalDamp");
            var dampT = dampObj.AddComponent<DampedTransform>();
            dampT.data.constrainedObject = petal.transform;
            dampT.data.sourceObject = bone;
            dampT.data.dampPosition = 0.1f;
            dampT.data.dampRotation = 0.5f;
            
            dampObj.transform.SetParent(rig.transform);
        }

        List<Transform> boneTransforms = new List<Transform>();
        foreach (PlayerBone playerBone in GetComponentsInChildren<PlayerBone>())
        {
            boneTransforms.Add(playerBone.transform);
        }
        boneRenderer.transforms = boneTransforms.ToArray();
        
        rigBuilder.Build();
    }
    

    // Update is called once per frame
    void Update()
    {
        UpdateInput();
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
        }
        
        foreach (DampedTransform dampedTransform in dampedTransforms)
        {
            dampedTransform.weight = dampWeight;
        }
        
        transform.Translate(Vector3.forward * (vertical * Time.deltaTime * moveSpeed));
    }

    public class PathLog
    {
        public Vector3 Position;
        public Vector3 Direction;
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
