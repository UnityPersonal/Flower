using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class PlayerBone : MonoBehaviour
{
    public PlayerBone parentBone;
    
    public float currentAngle;
    public float rotationSpeed = 100f;

    public ObiRope[] ropes;

    ObiRopeCursor ropeCursor;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var rope in ropes)
        {
            Vector2 randCircle = Random.insideUnitCircle;
            Vector3 offset = randCircle.x * transform.right;
            offset += randCircle.y * transform.up;
            
            rope.transform.localPosition = offset;
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        currentAngle += rotationSpeed * Time.deltaTime;
    }
}
