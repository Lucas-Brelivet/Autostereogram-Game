using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyPadButton : Clickable
{
    [SerializeField][Tooltip("How should the button move when pressed, in object coordinates")]
    private Vector3 pressedPositionOffset = Vector3.zero;

    [SerializeField][Tooltip("Event to invoke when this button is pressed")]
    UnityEvent onPress;

    public override void ClickDown()
    {
        transform.Translate(pressedPositionOffset);
        onPress?.Invoke();
    }

    public override void ClickUp()
    {
        transform.Translate(-pressedPositionOffset);
    }
}
