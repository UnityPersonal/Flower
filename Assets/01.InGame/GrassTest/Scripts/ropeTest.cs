using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class ropeTest : MonoBehaviour
{
    public GameObject particlePrefab;
    public ObiSolver solver;

    private ObiRope[] ropes;
    List<Transform> particles = new List<Transform>();
    
    // Start is called before the first frame update
    void Start()
    {
        ropes = solver.GetComponentsInChildren<ObiRope>();
        foreach (var rope in ropes)
        {
            for (int i = 0; i < rope.particleCount; i++)
            {
                if(i==0)
                    continue;
                
                if(i%3 !=0)
                    continue;
                var particle =Instantiate(particlePrefab, rope.GetParticlePosition(i) , Quaternion.identity );
                particles.Add(particle.transform);
            }
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log($"Update Particle {particles.Count}");
        int id = 0;
        foreach (var rope in ropes)
        {
            for (int i = 0; i < rope.particleCount; i++)
            {
                if(i==0)
                    continue;
                
                if(i%3 !=0)
                    continue;
                int solverIndex = rope.solverIndices[i];
                var particle = particles[id];
                particle.position = rope.solver.positions[solverIndex];
                id++;
            }
        }
    }
}
