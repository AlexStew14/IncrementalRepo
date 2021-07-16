using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CustomEvent : UnityEvent<System.Object> { }

public class EventManager : MonoBehaviour
{
    private Dictionary<string, CustomEvent> _eventDictionary;

    private static EventManager eventManager;

    public static EventManager Instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    Debug.LogError("No active EventManager in Scene!");
                }
                else
                {
                    eventManager.Init();
                }
            }
            return eventManager;
        }
    }

    private void Init()
    {
        if (_eventDictionary == null)
        {
            _eventDictionary = new Dictionary<string, CustomEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction<System.Object> listener)
    {
        CustomEvent thisEvent = null;
        if (Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new CustomEvent();
            thisEvent.AddListener(listener);
            Instance._eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<System.Object> listener)
    {
        if (Instance == null) return;
        CustomEvent thisEvent = null;
        if (Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName, System.Object arg = null)
    {
        CustomEvent thisEvent = null;
        if (Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            Debug.Log(eventName + " triggered.");
            thisEvent.Invoke(arg);
        }
    }
}