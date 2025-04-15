using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class Petal : MonoBehaviour
{
    public ObiRope rope;
    public int particleIndex;
    
    public PlayerBone forwardBone;
    public PlayerBone backwardBone;
    [Range(0, 1)] public float normalizedPosition;
    public float radius;
    
    public float angleOffset;
    
    public MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rope.solverIndices.count <= 0)
        {
            Debug.Log("Rope has no solver indices");
            meshRenderer.material.color = Color.blue;
            return;
        }
        
        int solverIndex = rope.solverIndices[particleIndex];
        if (rope.solver.positions.count <= 0)
        {
            Debug.Log("Rope has no positions");
            meshRenderer.material.color = Color.blue;
            return;
        }
        meshRenderer.material.color = Color.red;


        var localPosition  = rope.solver.positions[solverIndex];
        var orient = rope.solver.orientations[solverIndex];
        var worldPosition = LocalToWorldPosition(localPosition, rope.solver.transform);
        
        transform.position = worldPosition;
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
    }
    
    // 로컬 좌표를 월드 좌표로 변환하는 함수
    private Vector3 LocalToWorldPosition(Vector4 localPosition, Transform solverTransform)
    {
        return solverTransform.TransformPoint(new Vector3(localPosition.x, localPosition.y, localPosition.z));
    }
}
