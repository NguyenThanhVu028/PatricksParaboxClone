using System.Collections.Generic;
using UnityEngine;

public class EventPublisher
{
    protected Dictionary<string, List<IEventListener>> _normalListeners = new();
    protected Dictionary<string, List<IEventListener>> _onceTimeListeners = new();
    protected Dictionary<string, (float, object)> _previousEvents = new();

    /// <summary>
    /// Normal listeners. Only listen for event fired AFTER subscription.
    /// </summary>
    /// <param name="listener"></param>
    public void AddListener(string eventName, IEventListener listener)
    {
        if (listener == null)
        {
            return;
        }
        _normalListeners ??= new();
        _normalListeners[eventName] ??= new();
        _normalListeners[eventName].Add(listener);
    }
    /// <summary>
    /// These listeners only listen to the event once, can listen for event that has already fired in THE SAME FRAME.
    /// </summary>
    /// <param name="listener"></param>
    public void AddListenerOnce(string eventName, IEventListener listener)
    {
        if (listener == null)
        {
            return;
        }
        // Invoke the listener immediately if the event has already been fired
        if (_previousEvents.TryGetValue(eventName, out var eventDetails))
        {
            // Remove the record if the stored event has expired
            if (eventDetails.Item1 != Time.timeSinceLevelLoad)
            {
                _previousEvents.Remove(eventName);
                return;
            }
            listener.ReceiveEvent(eventName, _previousEvents[eventName]);
        }
        else
        {
            _onceTimeListeners ??= new();
            _onceTimeListeners[eventName] ??= new();
            _onceTimeListeners[eventName].Add(listener);
        }
    }

    public void InvokeEvent(string eventName, object value)
    {
        _previousEvents[eventName] = (Time.timeSinceLevelLoad, value);

        if (_normalListeners.TryGetValue(eventName, out List<IEventListener> listeners))
        {
            listeners.RemoveAll(item => item == null);
            foreach (var listener in listeners)
            {
                listener?.ReceiveEvent(eventName, value);
            }
        }

        if (_onceTimeListeners.TryGetValue(eventName, out List<IEventListener> onceListeners))
        {
            onceListeners.RemoveAll(item => item == null);
            foreach (var listener in onceListeners)
            {
                listener?.ReceiveEvent(eventName, value);
            }

            _onceTimeListeners.Clear();
        }
    }
}

public interface IEventListener
{
    public void ReceiveEvent(string eventName, object value);
}
