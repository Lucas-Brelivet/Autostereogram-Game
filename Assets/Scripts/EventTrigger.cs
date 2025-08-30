using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component that invokes an event on the EventManager in OnTriggerEnter
/// </summary>
[RequireComponent(typeof(Collider))]
public class EventTrigger : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The name of the event to be invoked")]
    private string eventName;

    [SerializeField]
    [Tooltip("Only invokes the event if the object entering the trigger has one of those tags. " +
        "If the list is empty, doesn't check the tag")]
    private List<string> tags = new List<string>();


    private void OnTriggerEnter(Collider collider)
    {
        if (tags.Count == 0 || tags.Contains(collider.tag))
        {
            EventManager.Instance.InvokeEvent(eventName);
        }
    }
}

