using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassInteraction : MonoBehaviour
{
    public Transform transformToFollow;
    public RenderTexture rt;
    public string GlobalTexName = "_GlobalEffectRT";
    public string GlobalOrthoName = "_OrthographicCamSize";
    private float orthoMem = 0;
    
    void Awake()
    {
        orthoMem = GetComponent<Camera>().orthographicSize;
        Shader.SetGlobalFloat(GlobalOrthoName, orthoMem);
        Shader.SetGlobalTexture(GlobalTexName, rt);
        Shader.SetGlobalFloat("_HasRT", 1);
    }

    void Update()
    {
        if (transformToFollow != null)
        {
            //transform.position = new Vector3(transformToFollow.position.x, transformToFollow.position.y + 20, transformToFollow.position.z);

            var followPos = transformToFollow.position;
            followPos += Vector3.up * 20;
            //followPos += transformToFollow.forward * 20;
            
            transform.position = followPos;
            Shader.SetGlobalVector("_Position", transformToFollow.position);
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
        
        Shader.SetGlobalFloat(GlobalOrthoName, orthoMem);
        Shader.SetGlobalTexture(GlobalTexName, rt);
        Shader.SetGlobalFloat("_HasRT", 1);
    }
}
