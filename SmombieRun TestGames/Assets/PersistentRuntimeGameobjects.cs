using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentRuntimeGameobjects : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> AllGameObjects;
    void Start()
    {
        foreach (GameObject item in AllGameObjects)
        {
            DontDestroyOnLoad(item);
        }
        Destroy(gameObject);
    }
}
