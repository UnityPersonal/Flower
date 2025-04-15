using System.Collections.Generic;
using UnityEngine;

public class ParticleAttributeSetter : MonoBehaviour
{
    public Transform setterTransform;
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    private List<Vector4> customData = new List<Vector4>();

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
        
        var customDataModule = ps.customData;
        customDataModule.enabled = true;
        customDataModule.SetVectorComponentCount(ParticleSystemCustomData.Custom1, 4); // Vector4 사용
    }

    void Update()
    {
        int particleCount = ps.particleCount;
        if (particleCount == 0) return;

        int lastIndex = particleCount - 1;
        var particle = particles[lastIndex];
        
        customData.Clear();
        ps.GetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
        var pos = setterTransform.position;

        int setCount = 0;
        
        for(int i = 0; i < customData.Count; i++)
        {
            if (customData[i].w < 0.5f)
            {
                customData[i] = new Vector4(pos.x, pos.y, pos.z, 1);
                setCount++;
            }
        }
        
        ps.SetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
        Debug.Log($"update custom data count: {setCount} / {particleCount}");
        

    }
}