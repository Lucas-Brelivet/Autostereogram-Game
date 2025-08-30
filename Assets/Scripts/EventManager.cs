using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A class that allows one to subscribe to events and invoke them.
/// An event is identified by a string and is created when a listener subscribes to it.
/// When someone invokes an event with the corresponding string, all the listener methods are called
/// </summary>
public class EventManager : Singleton<EventManager>
{
    /// <summary>
    /// A dictionary containing argument less events identified by strings
    /// </summary>
    private Dictionary<string, UnityEventBase> events = new Dictionary<string, UnityEventBase>();


    /// <summary>
    /// Subscribe a UnityAction to an event with the given eventName. Creates that event if it doesn't already exist
    /// </summary>
    /// <param name="eventName">A string used to identify the event</param>
    /// <param name="subscriber">The action to be done when the event is invoked</param>
    public void SubscribeToEvent(string eventName, UnityAction subscriber)
    {
        UnityEvent evt = GetUnityEvent(eventName);
        if (evt == null)
        {
            evt = new UnityEvent();
            events[eventName] = evt;
        }
        evt.AddListener(subscriber);
    }
    public void SubscribeToEvent<T>(string eventName, UnityAction<T> subscriber)
    {
        UnityEvent<T> evt = GetUnityEvent<T>(eventName);
        if (evt == null)
        {
            evt = new UnityEvent<T>();
            events[eventName] = evt;
        }
        evt.AddListener(subscriber);
    }

    /// <summary>
    /// Unsubscribe the given UnityAction from the event with the given eventName, if it exists
    /// </summary>
    /// <param name="eventName">A string used to identify the event</param>
    /// <param name="subscriber">The action to be unsubscribed from the event</param>
    public void UnsubscribeFromEvent(string eventName, UnityAction subscriber)
    {
        UnityEvent evt = GetUnityEvent(eventName);
        if (evt != null)
        {
            evt.RemoveListener(subscriber);
        }
    }
    public void UnsubscribeFromEvent<T>(string eventName, UnityAction<T> subscriber)
    {
        UnityEvent<T> evt = GetUnityEvent<T>(eventName);
        if (evt != null)
        {
            evt.RemoveListener(subscriber);
        }
    }

    /// <summary>
    /// Invoke the event with the given eventName
    /// </summary>
    /// <param name="eventName">A string used to identify the event</param>
    public void InvokeEvent(string eventName)
    {
        UnityEvent evt = GetUnityEvent(eventName);
        if (evt != null)
        {
            evt.Invoke();
        }
    }
    /// <summary>
    /// Invoke the event with the given eventName, passing it the argument eventArgs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventName">A string used to identify the event</param>
    /// <param name="eventArg">The argument to pass to the event</param>
    public void InvokeEvent<T>(string eventName, T eventArg)
    {
        UnityEvent<T> evt = GetUnityEvent<T>(eventName);
        if (evt != null)
        {
            evt.Invoke(eventArg);
        }
    }

    /// <summary>
    /// Get the UnityEventBase with the given eventName from the events lists if it exists and cast it as a UnityEvent.
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    /// <returns>The event, or null if it was not found or couldn't be cast as a UnityEvent</returns>
    private UnityEvent GetUnityEvent(string eventName)
    {
        UnityEventBase evtBase;
        if (events.TryGetValue(eventName, out evtBase))
        {
            UnityEvent evt = evtBase as UnityEvent;
            if (evt != null)
            {
                return evt;
            }
            else
            {
                Debug.LogWarning("An event with the same name but a different argument type already exists. It will be overwritten");
            }
        }
        return null;
    }
    /// <summary>
    /// Get the UnityEventBase with the given eventName from the events lists if it exists and cast it as a UnityEvent<T>.
    /// </summary>
    /// <param name="eventName">The name of the event</param>
    /// <returns>The event, or null if it was not found or couldn't be cast as a UnityEvent<T></returns>
    private UnityEvent<T> GetUnityEvent<T>(string eventName)
    {
        UnityEventBase evtBase;
        if (events.TryGetValue(eventName, out evtBase))
        {
            UnityEvent<T> evt = evtBase as UnityEvent<T>;
            if (evt != null)
            {
                return evt;
            }
            else
            {
                Debug.LogWarning("An event with the same name but a different argument type already exists. It will be overwritten");
            }
        }
        return null;
    }
}

