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

    [Header("Grass Interaction Settings")]
    [SerializeField] private ParticleSystem particlesystem;
    
    public Collider mainCollider { get; private set; }

    private void Awake()
    {
        localPlayer = this;
        mainCollider = GetComponent<Collider>();
    }

    void Start()
    {
        StartCoroutine(UpdatePath());
    }
    
    void Update()
    {
        UpdateInput();
        
        UpdateRotation();
        UpdateMovement();
        
        UpdateWind();
        UpdateGravity();
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


    [SerializeField] private float maxLookAngle = 60f;
    private float verticalLookRotation = 0f;
    private float horizontalLookRotation = 0f;
    void UpdateRotation()
    {
        horizontalLookRotation += inputAxis.x * rotationSpeed * Time.deltaTime;
        Quaternion yawRotation = Quaternion.AngleAxis(horizontalLookRotation, Vector3.up);

        verticalLookRotation -= inputAxis.y * rotationSpeed * Time.deltaTime;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        Quaternion pitchRotation = Quaternion.AngleAxis(verticalLookRotation, Vector3.right);
        transform.localRotation = quaternion.identity* yawRotation * pitchRotation;
        
        
        
        
        /*transform.Rotate(  Vector3.up, inputAxis.x * rotationSpeed * Time.deltaTime  );
        transform.Rotate(  Vector3.right, inputAxis.y * rotationSpeed * Time.deltaTime  );

        var angles = inputAxis * (rotationSpeed * Time.deltaTime);
        
        
        Quaternion rot = Quaternion.Euler(-angles.y, angles.x, 0 );
        
        transform.localRotation = transform.localRotation * rot;
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.x = Mathf.Clamp(localEulerAngles.x, -90f, 90f);
        transform.localEulerAngles = localEulerAngles;*/
    }

    public float rayCassDistance = 5f;

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
        
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayCassDistance, LayerMask.GetMask("Ground")))
        {
            currentSpeed = Mathf.Lerp(currentSpeed, -1, moveSpeed * Time.deltaTime);
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

}
