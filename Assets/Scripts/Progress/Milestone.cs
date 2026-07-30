using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public enum EventType { Canon, Random }
    
[Serializable]
public class Milestone
{
    [HideInInspector] public EventType eventType;
    [HideInInspector] public BlackjackEvent gameEvent;
    [HideInInspector] public int moneyAmount;
    [HideInInspector] public int maxTurns;
    [HideInInspector] public List<GameObject> keepsakes;
    [HideInInspector] public CardChoiceEvent cardChoiceEvent;
}

[CustomEditor(typeof(Milestone))]
public class MilestoneEventEditor : Editor
{
    private SerializedProperty eventTypeProperty;
    private SerializedProperty gameEventProperty;
    private SerializedProperty moneyAmountProperty;
    private SerializedProperty maxTurnsProperty;
    private SerializedProperty keepsakesProperty;

    private void OnEnable()
    {
        eventTypeProperty = serializedObject.FindProperty("eventType");
        gameEventProperty = serializedObject.FindProperty("gameEvent");
        moneyAmountProperty = serializedObject.FindProperty("moneyAmount");
        maxTurnsProperty = serializedObject.FindProperty("maxTurns");
        keepsakesProperty = serializedObject.FindProperty("keepsakes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(eventTypeProperty);

        EventType eventType = eventTypeProperty.GetEnumValue<EventType>();
        if (eventType == EventType.Canon)
            EditorGUILayout.PropertyField(gameEventProperty);

        EditorGUILayout.PropertyField(moneyAmountProperty);
        EditorGUILayout.PropertyField(maxTurnsProperty);
        EditorGUILayout.PropertyField(keepsakesProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
