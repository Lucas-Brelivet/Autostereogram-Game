using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject("Singleton");
                instance = gameObject.AddComponent<T>();
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = (T)this;
    }
}
