using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GrassInteraction : MonoBehaviour
{
    private static readonly int MAP_SIZE_OFFSET = Shader.PropertyToID("_MapSize_Offset");
    private static readonly int GLOBAL_GLORY_RT = Shader.PropertyToID("_GlobalGloryRT");
    private static readonly int GLOBAL_FORCE_RT = Shader.PropertyToID("_GlobalForceRT");
    private static readonly int GLOBAL_PAINT_RT = Shader.PropertyToID("_GlobalPaintRT");
    private static readonly int HAS_RT = Shader.PropertyToID("_HasRT");
    private static readonly int GLOBAL_EFFECT_RT = Shader.PropertyToID("_GlobalEffectRT");
    private static readonly int ORTHOGRAPHIC_CAM_SIZE = Shader.PropertyToID("_OrthographicCamSize");
    private static readonly int POSITION = Shader.PropertyToID("_Position");
    private static readonly int INTERACTION_DISTANCE = Shader.PropertyToID("_InteractionDistance");

    public Camera followCamera;
    public Transform transformToFollow;
    public RenderTexture rt;
    private float orthoMem = 0;
    [Space(10)]
    public RenderTexture pintRT;
    [Space(10)]
    public RenderTexture forceRT;
    [Space(10)]
    public RenderTexture gloryRT;

    public float interactionDistance;
    
    [FormerlySerializedAs("MapSize_Offset")] public Vector4 mapSizeOffset;
    
    void Awake()
    {
        orthoMem = followCamera.orthographicSize;
        Shader.SetGlobalFloat(ORTHOGRAPHIC_CAM_SIZE, orthoMem);
        Shader.SetGlobalTexture(GLOBAL_EFFECT_RT, rt);
        
        Shader.SetGlobalFloat(HAS_RT, 1);
        Shader.SetGlobalFloat(INTERACTION_DISTANCE ,interactionDistance);
        
        Shader.SetGlobalTexture(GLOBAL_PAINT_RT, pintRT);
        Shader.SetGlobalTexture(GLOBAL_FORCE_RT, forceRT);
        Shader.SetGlobalTexture(GLOBAL_GLORY_RT, gloryRT);
        
        Shader.SetGlobalVector(MAP_SIZE_OFFSET, mapSizeOffset);
    }

    void Update()
    {
        if (transformToFollow != null)
        {
            //transform.position = new Vector3(transformToFollow.position.x, transformToFollow.position.y + 20, transformToFollow.position.z);

            var followPos = transformToFollow.position;
            followPos += Vector3.up * 20;
            //followPos += transformToFollow.forward * 20;
            
            followCamera.transform.position = followPos;
            followCamera.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            Shader.SetGlobalVector(POSITION, transformToFollow.position);
        }
        
        Shader.SetGlobalFloat(ORTHOGRAPHIC_CAM_SIZE, orthoMem);
        Shader.SetGlobalTexture(GLOBAL_EFFECT_RT, rt);
        
        Shader.SetGlobalFloat(HAS_RT, 1);
        Shader.SetGlobalFloat(INTERACTION_DISTANCE ,interactionDistance);
        
        Shader.SetGlobalTexture(GLOBAL_PAINT_RT, pintRT);
        Shader.SetGlobalTexture(GLOBAL_FORCE_RT, forceRT);
        Shader.SetGlobalTexture(GLOBAL_GLORY_RT, gloryRT);
        
        Shader.SetGlobalVector(MAP_SIZE_OFFSET, mapSizeOffset);
        
    }
}
