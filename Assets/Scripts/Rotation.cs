using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    [SerializeField]
    private Vector3 axis = Vector3.up;

    [SerializeField]
    private float angularSpeed = 20;

    [SerializeField]
    private Space relativeTo;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axis, angularSpeed * Time.deltaTime, relativeTo);
    }
}
