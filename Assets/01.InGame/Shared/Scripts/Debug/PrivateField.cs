using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivateField : MonoBehaviour
{
    public int publicInt;
    public int privateInt;
    
    public int PublicPropertyInt { get; set; }
    private int PrivatePropertyInt { get; set; }
    public int PropertyInt { get; private set; }

    private GameObject privateObject;

    IEnumerator Change()
    {
        while (gameObject.activeInHierarchy)
        {
            publicInt = Random.Range(0,10);
            privateInt = Random.Range(0,10);
            PublicPropertyInt = Random.Range(0,10);
            PrivatePropertyInt = Random.Range(0,10);
            PropertyInt = Random.Range(0,10);
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Change());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
