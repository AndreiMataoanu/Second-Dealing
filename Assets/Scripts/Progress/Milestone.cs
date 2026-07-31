using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public enum MilestoneType { CanonEvent, RandomEvent, FinalGoal }
    
[Serializable]
public class Milestone
{
    [HideInInspector] public MilestoneType milestoneType;
    [HideInInspector] public BlackjackEvent gameEvent;
    [HideInInspector] public int moneyAmount;
    [HideInInspector] public int maxTurns = 5;
    [HideInInspector] public List<GameObject> keepsakes;
}

[CustomPropertyDrawer(typeof(Milestone))]
public class MilestonePropertyDrawer : PropertyDrawer
{
    private SerializedProperty eventTypeProperty;
    private SerializedProperty gameEventProperty;
    private SerializedProperty moneyAmountProperty;
    private SerializedProperty maxTurnsProperty;
    private SerializedProperty keepsakesProperty;

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        eventTypeProperty = property.FindPropertyRelative("milestoneType");
        gameEventProperty = property.FindPropertyRelative("gameEvent");
        moneyAmountProperty = property.FindPropertyRelative("moneyAmount");
        maxTurnsProperty = property.FindPropertyRelative("maxTurns");
        keepsakesProperty = property.FindPropertyRelative("keepsakes");
        
        EditorGUI.LabelField(rect, $"Milestone");

        OnInspectorGUI();
    }

    private void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(eventTypeProperty);

        if ((MilestoneType)eventTypeProperty.enumValueIndex == MilestoneType.CanonEvent)
            EditorGUILayout.PropertyField(gameEventProperty);

        EditorGUILayout.PropertyField(moneyAmountProperty);
        EditorGUILayout.PropertyField(maxTurnsProperty);
        
        if ((MilestoneType)eventTypeProperty.enumValueIndex != MilestoneType.FinalGoal)
            EditorGUILayout.PropertyField(keepsakesProperty);
    }
}
