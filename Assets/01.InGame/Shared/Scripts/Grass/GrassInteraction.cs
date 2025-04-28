using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GrassInteraction : MonoBehaviour
{
    //
    private static readonly int GLOBAL_EFFECT_RT = Shader.PropertyToID("_GlobalEffectRT");
    private static readonly int ORTHOGRAPHIC_CAM_SIZE = Shader.PropertyToID("_OrthographicCamSize");
    private static readonly int POSITION = Shader.PropertyToID("_Position");
    private static readonly int INTERACTION_DISTANCE = Shader.PropertyToID("_InteractionDistance");

    [SerializeField] public Camera followEffectCamera;
    [SerializeField] public Camera paintCamera;
    [SerializeField] public Camera forceCamera;
    [SerializeField] public Camera gloryCamera;
    
    public Transform transformToFollow;
    public RenderTexture rt;
    private float orthoMem = 0;
    [Space(10)]
    public RenderTexture paintRT;
    [Space(10)]
    public RenderTexture forceRT;
    [Space(10)]
    public RenderTexture gloryRT;

    public float interactionDistance;
    
    [FormerlySerializedAs("MapSize_Offset")] public Vector4 mapSizeOffset;
    
    
    
    void Awake()
    {
        orthoMem = followEffectCamera.orthographicSize;
        Shader.SetGlobalFloat(ORTHOGRAPHIC_CAM_SIZE, orthoMem);
        
        if(paintCamera != null)
            Shader.SetGlobalFloat("_PaintOrthoSize", paintCamera.orthographicSize);
        if(forceCamera != null)
            Shader.SetGlobalFloat("_ForceOrthoSize", forceCamera.orthographicSize);
        if(gloryCamera != null)
            Shader.SetGlobalFloat("_GloryOrthoSize", gloryCamera.orthographicSize);
        
        Shader.SetGlobalTexture(GLOBAL_EFFECT_RT, rt);
        
        Shader.SetGlobalFloat(INTERACTION_DISTANCE ,interactionDistance);
        
    }

    void Update()
    {
        if (transformToFollow != null)
        {
            //transform.position = new Vector3(transformToFollow.position.x, transformToFollow.position.y + 20, transformToFollow.position.z);

            var followPos = transformToFollow.position;
            followPos += Vector3.up * 200;
            //followPos += transformToFollow.forward * 20;
            
            followEffectCamera.transform.position = followPos;
            followEffectCamera.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));

            if (paintCamera != null)
            {
                paintCamera.transform.position = followPos;
                Shader.SetGlobalFloat("_PaintOrthoSize", paintCamera.orthographicSize);
            }

            if (forceCamera != null)
            {
                forceCamera.transform.position = followPos;
                Shader.SetGlobalFloat("_ForceOrthoSize", forceCamera.orthographicSize);
            }

            if (gloryCamera != null)
            {
                gloryCamera.transform.position = followPos;
                Shader.SetGlobalFloat("_GloryOrthoSize", gloryCamera.orthographicSize);
            }
            
            Shader.SetGlobalVector("_Position", transformToFollow.position);
        }
        
        if(followEffectCamera.orthographicSize != orthoMem)
            Shader.SetGlobalFloat(ORTHOGRAPHIC_CAM_SIZE, orthoMem);
        
        
        Shader.SetGlobalTexture(GLOBAL_EFFECT_RT, rt);
        
        Shader.SetGlobalFloat("_InteractionDistance" ,interactionDistance);
        
    }
}
