using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusedInteractable : Interactable
{
    [SerializeField][Tooltip("The view point that the player should take when interacting with this object")]
    protected Transform focusedViewPoint;


    /// <summary>
    /// The view point that the player should take when interacting with this object
    /// </summary>
    public Pose FocusedViewPoint
    {
        get
        {
            return new Pose(focusedViewPoint.position, focusedViewPoint.rotation);
        }
    }

    public override void Interact(PlayerController player)
    {
        player.SetHeadPose(FocusedViewPoint);
        player.input.actions.FindActionMap("Movements").Disable();
    }

    public override void EndInteraction(PlayerController player)
    {
        player.ReinitializeHeadPose();
        player.input.actions.FindActionMap("Movements").Enable();
    }
}
