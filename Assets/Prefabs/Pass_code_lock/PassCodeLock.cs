using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PassCodeLock : FocusedInteractable
{
    [SerializeField]
    private TextMeshPro screen;

    [SerializeField]
    private UnityEvent onCorrectPasswordEntered;

    [SerializeField]
    private int maxCharacterCount = 4;

    private string password = "0000";


    public override void Interact(PlayerController player)
    {
        base.Interact(player);
        player.input.actions.FindActionMap("MouseInteraction").Enable();
    }
    public override void EndInteraction(PlayerController player)
    {
        base.EndInteraction(player);
        player.input.actions.FindActionMap("MouseInteraction").Disable();
    }

    public void WriteText(string text)
    {
        string textToWrite = text.Substring(0, Mathf.Min(text.Length, maxCharacterCount - screen.text.Length));
        screen.text += textToWrite;
    }


    public void BackSpace()
    {
        if(screen.text.Length > 0)
        {
            screen.text = screen.text.Remove(screen.text.Length-1);
        }
    }

    public void Enter()
    {
        if(screen.text == password)
        {
            onCorrectPasswordEntered?.Invoke();
            Debug.Log("Correct password");
        }
        
        Debug.Log("Wrong password");
    }

    public void SetPassword(string password)
    {
        if(password.Length <= maxCharacterCount)
        {
            this.password = password;
        }
    }

    
}
