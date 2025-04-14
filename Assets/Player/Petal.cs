using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Petal : MonoBehaviour
{
    public Transform toFollow;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(toFollow.position, toFollow.forward,  180f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position,toFollow.position, Time.deltaTime);
    }
}
