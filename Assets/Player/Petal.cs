using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;
using Random = UnityEngine.Random;

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
        rope.OnSimulationEnd += PetalUpdate;
        
        Vector3 rand3 = Random.insideUnitSphere;
        rand3.z = 60f;
        transform.localRotation = Quaternion.Euler(rand3);

    }

    public int index = 0;
    // Update is called once per frame
    void PetalUpdate(ObiActor actor, float simulatedTime, float substepTime)
    {
        if (!rope.isLoaded)
        {
            Debug.Log("No rope loaded");
            return;
        }

        var localPosition = GetParticlePosition();
        var worldPosition = LocalToWorldPosition(localPosition, rope.solver.transform);
        transform.position = worldPosition;
        transform.forward = Camera.main.transform.forward;
    }
    
    private Vector3 GetParticlePosition()
    {
        if (rope == null || rope.solver == null)
        {
            Debug.LogError("Rope or solver is not assigned.");
            meshRenderer.material.color = Color.blue;
            return Vector3.zero;
        }

        if (index < 0 || index >= rope.particleCount)
        {
            Debug.LogError("Index out of range.");
            meshRenderer.material.color = Color.blue;
            return Vector3.zero;
        }
        var endElement = rope.elements[0];
        int endParticleIndex = endElement.particle1;
        

        
        //int solverIndex = rope.solverIndices[index];
        return rope.solver.positions[endParticleIndex];
    }
    
    // 로컬 좌표를 월드 좌표로 변환하는 함수
    private Vector3 LocalToWorldPosition(Vector4 localPosition, Transform solverTransform)
    {
        return solverTransform.TransformPoint(new Vector3(localPosition.x, localPosition.y, localPosition.z));
    }
}
