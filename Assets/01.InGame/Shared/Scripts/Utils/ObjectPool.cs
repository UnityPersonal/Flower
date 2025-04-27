using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public abstract class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    [SerializeField] T smaplePrefab;
    [SerializeField] int poolSize = 10;
    
    private Queue<T> pool = new Queue<T>();

    public T Get()
    {
        if (pool.Count == 0)
        {
            CreateNewItem();
        }
        T item = pool.Dequeue();
        item.gameObject.SetActive(true);
        return item;
    }
    
    public void Return(T item)
    {
        item.gameObject.SetActive(false);
        pool.Enqueue(item);
    }
    
    protected virtual void Awake()
    {
        if (smaplePrefab == null)
        {
            Debug.LogError("Sample prefab is not assigned.");
            return;
        }
        
        for (int i = 0; i < poolSize; i++)
        {
            T item = CreateNewItem();
            pool.Enqueue(item);
        }
    }

    private T CreateNewItem()
    {
        var item = Instantiate(smaplePrefab, transform);
        GameObject obj = item as GameObject;
        obj.SetActive(false);
        
        return item;
    }
}
