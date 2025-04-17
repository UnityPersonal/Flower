using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralParticlePlacement : MonoBehaviour
{
    public int particleCount = 100; // 파티클 개수
    public float radius = 1f; // 회오리 반지름
    public float heightStep = 0.1f; // 높이 증가량
    public float angleStep = 20f; // 각도 증가량 (도 단위)

    public float angleMinStep = 20f;
    public float angleMaxStep = 20f;
    
    Vector3[] positions;
    private int curIndex = 0;

    public bool IsEmpty()
    {
        return curIndex >= particleCount;
    }

    public Vector3 GetNextPlacement()
    {
        var position = positions[curIndex];
        curIndex++;
        return position;   
    }
    
    public void BuildPlacement()
    {
        positions = new Vector3[particleCount];
        float angle =0;
        for (int i = 0; i < particleCount; i++)
        {
            float randAngleStep = Random.Range(angleMinStep, angleMaxStep);
            // 각도와 높이 계산
            angle += angleStep * Mathf.Deg2Rad; // 라디안으로 변환
            float height = i * heightStep;

            // 데카르트 좌표로 변환
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);
            float z = -height;

            // 파티클 생성 및 위치 설정
            Vector3 position = new Vector3(x, y, z);
            
            positions[i] = position;
        }
    }

}
