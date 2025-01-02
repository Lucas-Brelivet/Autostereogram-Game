using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SasDoor : MonoBehaviour
{
    [SerializeField] private Animator animator;


    public void OpenFrontDoor()
    {
        animator.SetTrigger("OpenFrontDoor");
        Debug.Log("OpenFrontDoor Trigger Set");
    }

    public void CloseFrontDoor()
    {
        animator.SetTrigger("CloseFrontDoor");
    }

    public void OpenBackDoor()
    {
        animator.SetTrigger("OpenBackDoor");
    }

    public void CloseBackDoor()
    {
        animator.SetTrigger("CloseBackDoor");
    }
}
