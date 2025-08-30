using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntrance : MonoBehaviour
{
    [SerializeField]
    private SasDoor sasDoor;
    // Start is called before the first frame update
    void Start()
    {
        sasDoor?.OpenBackDoor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
