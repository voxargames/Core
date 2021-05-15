using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : CommonBehaviour  where T : Singleton<T>
{
    public static T Instance { get; private set; }

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning($"Trying to instantiate more tha one instance of {typeof(T).FullName}");
            return;
        }

        Instance = gameObject.GetComponent<T>();
        DontDestroyOnLoad(gameObject);                        
    }    
}
