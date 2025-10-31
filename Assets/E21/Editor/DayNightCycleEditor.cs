using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DayNightCycle))]
public class DayNightCycleEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DayNightCycle cycle = (DayNightCycle)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Time Control", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Dawn (6:00)"))
        {
            cycle.SetTime(6f);
        }
        if (GUILayout.Button("Noon (12:00)"))
        {
            cycle.SetTime(12f);
        }
        if (GUILayout.Button("Dusk (18:00)"))
        {
            cycle.SetTime(18f);
        }
        if (GUILayout.Button("Midnight (0:00)"))
        {
            cycle.SetTime(0f);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Normal Speed (1x)"))
        {
            cycle.SetTimeScale(1f);
        }
        if (GUILayout.Button("Fast (5x)"))
        {
            cycle.SetTimeScale(5f);
        }
        if (GUILayout.Button("Very Fast (10x)"))
        {
            cycle.SetTimeScale(10f);
        }
        EditorGUILayout.EndHorizontal();
    }
}