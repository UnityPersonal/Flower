using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginGameTrigger : MonoBehaviour
{
    [SerializeField] TriggerSensor triggerSensor;
    // Start is called before the first frame update
    void Start()
    {
        PlayerController.LocalPlayer.Lock();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            triggerSensor.OnTrigger();
        }
        
    }
}
