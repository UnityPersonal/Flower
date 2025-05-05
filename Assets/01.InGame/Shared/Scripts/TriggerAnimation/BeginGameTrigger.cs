using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class BeginGameTrigger : MonoBehaviour
{
    [SerializeField] TriggerSensor triggerSensor;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    // Start is called before the first frame update
    void Start()
    {
        PlayerController.LocalPlayer.Lock();
        PlayerController.LocalPlayer.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            triggerSensor.OnTrigger();
            virtualCamera.Priority = 0;
            PlayerController.LocalPlayer.Unlock();
            PlayerController.LocalPlayer.gameObject.SetActive(true);
            
            enabled = false;
        }
        
    }
}
