using UnityEngine;

public abstract class BlackjackEvent : ScriptableObject
{
    public enum EventSeverity { Low, Medium, High }

    public string eventName;

    public EventSeverity severity;

    public abstract void Apply(BlackjackGame game);
}
