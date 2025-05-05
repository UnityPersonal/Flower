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
    private static PlayerController localPlayer = null;
    public static PlayerController LocalPlayer
    {
        get
        {
            if (localPlayer == null)
            {
                localPlayer = FindObjectOfType<PlayerController>(true);
            }

            return localPlayer;
        }

        set
        {
            localPlayer = value;
        }
    }

    [Header("Movement Setting")]
    [SerializeField] private float moveSpeedMin = 1f;
    [SerializeField] private float moveSpeedMax = 10f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float currentSpeed;
    
    [Space(10)]
    [Header("Rotation Setting")]
    public float rotationSpeed = 60;
    public float rotationSpeedMin = 60;
    public float rotationSpeedMax = 60;
    
    [Space(10)]
    [Header("Collision Setting")]
    public float YOffsetMin = 2f;
    public float rayCassDistance = 5f;
    
    [SerializeField] private float maxLookAngle = 60f;
    private float verticalLookRotation = 0f;
    private float horizontalLookRotation = 0f;
    
    public float Speed => currentSpeed;
    public float NormalizedSpeed => (currentSpeed - moveSpeedMin) / (moveSpeedMax - moveSpeedMin);
    public Vector2 SpeedRange => new Vector2(moveSpeedMin, moveSpeedMax);
    
    public Transform headTransform;
    
    [Space(10)]
    [Header("Solver Setting")]
    [SerializeField] private  ObiSolver obiSolver;
    [SerializeField] private float windStrength = 1f;
    [SerializeField] private float gravityStrength = 1f;

    [Space(10)]
    [Header("Grass Interaction Settings")]
    [SerializeField] private ParticleSystem particlesystem;
    
    public Collider mainCollider { get; private set; }
    public Vector2 inputAxis { get; private set; }

    private void Awake()
    {
        mainCollider = GetComponent<Collider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        StartCoroutine(UpdatePath());
    }

    private bool locked = false;
    public void Lock()
    {
        locked = true;
        currentSpeed = moveSpeedMin;
    }

    public void Unlock()
    {
        locked = false;
    }
    
    void Update()
    {
        UpdateInput();

        if (locked==false)
        {
            UpdateRotation();
        }
        UpdateMovement();
        UpdateBoundary();
        
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
        float horizontalSpeed = Mathf.Lerp(rotationSpeedMax, rotationSpeedMin, NormalizedSpeed); 
        horizontalLookRotation += inputAxis.x * horizontalSpeed * Time.deltaTime;
        Quaternion yawRotation = Quaternion.AngleAxis(horizontalLookRotation, Vector3.up);

        verticalLookRotation -= inputAxis.y * rotationSpeed * Time.deltaTime;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);

        Quaternion pitchRotation = Quaternion.AngleAxis(verticalLookRotation, Vector3.right);
        transform.localRotation = quaternion.identity* yawRotation * pitchRotation;
        
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

    void UpdateBoundary()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit yhit, rayCassDistance, LayerMask.GetMask("Ground")))
        {
            float offsetY = (YOffsetMin - yhit.distance);
            transform.Translate(Vector3.up* Mathf.Clamp(offsetY, 0, offsetY), Space.World);
        }

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit fhit, rayCassDistance, LayerMask.GetMask("Ground")))
        {
            float offsetY = (YOffsetMin - fhit.distance);
            transform.Translate(-transform.forward * Mathf.Clamp(offsetY, 0, offsetY), Space.World);
        }

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
