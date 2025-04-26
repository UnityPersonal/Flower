using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class RobeFollower : MonoBehaviour
{
    public ObiRope rope;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int paricleCount = rope.particleCount;
        if (paricleCount == 0)
        {
            return;
        }
        
        Debug.Log("Update Particle " + paricleCount);
        int lastIndex = 0;
        int solverIndex = rope.solverIndices[lastIndex];

        Vector3 position = rope.solver.positions[solverIndex];
        transform.position = position + rope.transform.position;
    }
}
