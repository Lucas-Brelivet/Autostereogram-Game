using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour
{
    /// <summary>
    /// Call this function to interact with this object
    /// </summary>
    /// <param name="player">The player</param>
    public abstract void Interact(PlayerController player);

    /// <summary>
    /// call this function to end the interaction
    /// </summary>
    /// <param name="player">The player</param>
    public abstract void EndInteraction(PlayerController player);
    
}
