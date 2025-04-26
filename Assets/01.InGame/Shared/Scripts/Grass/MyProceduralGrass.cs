using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MyProceduralGrass : MonoBehaviour
{
    private static readonly int TERRAIN_POSITIONS = Shader.PropertyToID("_TerrainPositions");
    private static readonly int TERRAIN_NORMALS = Shader.PropertyToID("_TerrainNormals");
    private static readonly int TERRAIN_TRIANGLES = Shader.PropertyToID("_TerrainTriangles");
    private static readonly int TRANSFORM_MATRICES = Shader.PropertyToID("_TransformMatrices");
    private static readonly int TERRAIN_OBJECT_TO_WORLD = Shader.PropertyToID("_TerrainObjectToWorld");
    private static readonly int TERRAIN_TRIANGLE_COUNT = Shader.PropertyToID("_TerrainTriangleCount");
    private static readonly int MIN_OFFSET = Shader.PropertyToID("_MinOffset");
    private static readonly int MAX_OFFSET = Shader.PropertyToID("_MaxOffset");

    [Header("Rendering Properties")]
    public ComputeShader computeShader;
    
    private Mesh terrainMesh;
    [Tooltip("Mesh for individual grass blades.")]
    public Mesh grassMesh;
    [Tooltip("Material for rendering each grass blade.")]
    public Material material;
    
    private void Awake()
    {
        terrainMesh = GetComponent<MeshFilter>().sharedMesh;
    }
    
    [Range(-1.0f, 1.0f)]
    [Tooltip("Minimum random offset in the x- and z-directions.")]
    public float minOffset = -0.1f;
    [Range(-1.0f, 1.0f)]
    [Tooltip("Maximum random offset in the x- and z-directions.")]
    public float maxOffset = 0.1f;

    private GraphicsBuffer terrainTriangleBuffer;
    private GraphicsBuffer terrainVertexBuffer;
    private GraphicsBuffer terrainNormalBuffer;

    private GraphicsBuffer transformMatrixBuffer;
    
    private GraphicsBuffer grassTriangleBuffer;
    private GraphicsBuffer grassVertexBuffer;
    private GraphicsBuffer grassUVBuffer;

    private int kernel;
    private uint threadGroupSize;
    private int terrainTriangleCount = 0;

  
    
    private MaterialPropertyBlock properties;
    private Bounds bounds;

    private void Start()
    {
        kernel = computeShader.FindKernel("CalculateBladePositions");
        terrainMesh = GetComponent<MeshFilter>().sharedMesh;

        if (terrainMesh == null)
        {
            Debug.LogError($"terrain mesh null ");
        }
        
        Vector3[] terrainVertices = terrainMesh.vertices;
        terrainVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainVertices.Length, sizeof(float) * 3);
        terrainVertexBuffer.SetData(terrainVertices);
        
        Vector3[] terrainNormals = terrainMesh.normals;
        terrainNormalBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainVertices.Length, sizeof(float) * 3);
        terrainNormalBuffer.SetData(terrainNormals);
            
        int[] terrainTriangles = terrainMesh.triangles;
        terrainTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangles.Length, sizeof(int));
        terrainTriangleBuffer.SetData(terrainTriangles);
        
        terrainTriangleCount = terrainTriangles.Length / 3;
        
        Debug.Log($"terrainTriangleCount: {terrainTriangleCount}");
        
        computeShader.SetBuffer(kernel,TERRAIN_POSITIONS, terrainVertexBuffer);
        computeShader.SetBuffer(kernel,TERRAIN_NORMALS, terrainNormalBuffer);
        computeShader.SetBuffer(kernel,TERRAIN_TRIANGLES, terrainTriangleBuffer);
        
        // Grass data for RenderPrimitives.
        Vector3[] grassVertices = grassMesh.vertices;
        grassVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassVertices.Length, sizeof(float) * 3);
        grassVertexBuffer.SetData(grassVertices);

        int[] grassTriangles = grassMesh.triangles;
        grassTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassTriangles.Length, sizeof(int));
        grassTriangleBuffer.SetData(grassTriangles);

        Vector2[] grassUVs = grassMesh.uv;
        grassUVBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassUVs.Length, sizeof(float) * 2);
        grassUVBuffer.SetData(grassUVs);
        
        transformMatrixBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangleCount, sizeof(float) * 16);
        computeShader.SetBuffer(kernel, TRANSFORM_MATRICES, transformMatrixBuffer);
        
        bounds = terrainMesh.bounds;
        bounds.center += transform.position;
        
        RunComputeShader();
        
        
    }
    
    void RunComputeShader()
    {
        computeShader.SetMatrix(TERRAIN_OBJECT_TO_WORLD, transform.localToWorldMatrix);
        computeShader.SetInt(TERRAIN_TRIANGLE_COUNT, terrainTriangleCount);
        computeShader.SetFloat(MIN_OFFSET, minOffset);
        computeShader.SetFloat(MAX_OFFSET, maxOffset);
        
        // Run the compute shader's kernel function.
        computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt(terrainTriangleCount / threadGroupSize);
        computeShader.Dispatch(kernel, threadGroups, 1, 1);
        
    }
    
    // Run a single draw call to render all the grass blade meshes each frame.
    private void Update()
    {
        RenderParams rp = new RenderParams(material);
        rp.worldBounds = bounds;
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_TransformMatrices", transformMatrixBuffer);
        rp.matProps.SetBuffer("_Positions", grassVertexBuffer);
        rp.matProps.SetBuffer("_UVs", grassUVBuffer);

        Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Triangles, grassTriangleBuffer, grassTriangleBuffer.count, instanceCount: terrainTriangleCount);
        Graphics.DrawProcedural(material, bounds, MeshTopology.Triangles, grassTriangleBuffer, grassTriangleBuffer.count, 
            instanceCount: terrainTriangleCount, 
            properties: properties, 
            castShadows: ShadowCastingMode.Off, 
            receiveShadows: false);
    }

    
    private void OnDestroy()
    {
        // Dispose the buffers after use
        terrainTriangleBuffer.Dispose();
        terrainVertexBuffer.Dispose();
        transformMatrixBuffer.Dispose();
    }
}
