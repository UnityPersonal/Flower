using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MyProceduralGrass : MonoBehaviour
{
    private static readonly int TERRAIN_POSITIONS = Shader.PropertyToID("_TerrainPositions");
    private static readonly int TERRAIN_TRIANGLES = Shader.PropertyToID("_TerrainTriangles");

    [Header("Rendering Properties")]
    public ComputeShader computeShader;
    
    private MeshFilter meshFilter;
    public Material material;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }
    
    [Range(-1.0f, 1.0f)]
    [Tooltip("Minimum random offset in the x- and z-directions.")]
    public float minOffset = -0.1f;
    [Range(-1.0f, 1.0f)]
    [Tooltip("Maximum random offset in the x- and z-directions.")]
    public float maxOffset = 0.1f;

    private GraphicsBuffer terrainTriangleBuffer;
    private GraphicsBuffer terrainVertexBuffer;
    
    private GraphicsBuffer transformMatrixBuffer;
    
    private GraphicsBuffer grassTriangleBuffer;
    private GraphicsBuffer grassVertexBuffer;
    private GraphicsBuffer grassUVBuffer;

    private int kernel;
    private uint threadGroupSize;
    private int terrainTriangleCount = 0;

    private Mesh terrainMesh;
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
        
        int[] terrainTriangles = terrainMesh.triangles;
        terrainTriangleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangles.Length, sizeof(int));
        terrainTriangleBuffer.SetData(terrainTriangles);
        
        terrainTriangleCount = terrainTriangles.Length / 3;
        
        computeShader.SetBuffer(kernel,TERRAIN_POSITIONS, terrainVertexBuffer);
        computeShader.SetBuffer(kernel,TERRAIN_TRIANGLES, terrainTriangleBuffer);
        
        transformMatrixBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, terrainTriangleCount, sizeof(float) * 16);
        computeShader.SetBuffer(kernel, "_TransformMatrices", transformMatrixBuffer);
        
        bounds = terrainMesh.bounds;
        bounds.center += transform.position;
        
        RunComputeShader();
    }
    
    void RunComputeShader()
    {
        computeShader.SetMatrix("_TerrainObjectToWorld", transform.localToWorldMatrix);
        computeShader.SetInt("_TerrainTriangleCount", terrainTriangleCount);
        computeShader.SetFloat("_MinOffset", minOffset);
        computeShader.SetFloat("_MaxOffset", maxOffset);
        
        // Run the compute shader's kernel function.
        computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSize, out _, out _);
        int threadGroups = Mathf.CeilToInt(terrainTriangleCount / threadGroupSize);
        computeShader.Dispatch(kernel, threadGroups, 1, 1);

        Graphics.DrawProcedural(material,
            bounds,
            MeshTopology.Points,
            null,
            1,
            instanceCount: terrainTriangleCount, 
            properties: properties, 
            castShadows: ShadowCastingMode.Off, 
            receiveShadows: false);
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

        Graphics.RenderPrimitivesIndexed(rp, MeshTopology.Points, grassTriangleBuffer, grassTriangleBuffer.count, instanceCount: terrainTriangleCount);
        Graphics.DrawProcedural(material, bounds, MeshTopology.Points, grassTriangleBuffer, grassTriangleBuffer.count, 
            instanceCount: terrainTriangleCount, 
            properties: properties, 
            castShadows: ShadowCastingMode.Off, 
            receiveShadows: false);
    }

    
    private void OnDestroy()
    {
        terrainTriangleBuffer.Dispose();
        terrainVertexBuffer.Dispose();
        transformMatrixBuffer.Dispose();
    }
}
