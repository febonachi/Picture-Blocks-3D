using System;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Grid))]
public class GridEditor : Editor {
    private Grid grid;

    private SerializedProperty placeOnAwake;
    // private SerializedProperty hideOnAwake;
    private SerializedProperty prefab;
    private SerializedProperty cellHolder;
    private SerializedProperty scale;
    private SerializedProperty spacing;
    private SerializedProperty dimension;

    void OnEnable() {
        grid = target as Grid;

        placeOnAwake = serializedObject.FindProperty(nameof(placeOnAwake));
        // hideOnAwake = serializedObject.FindProperty(nameof(hideOnAwake));
        prefab = serializedObject.FindProperty(nameof(prefab));
        cellHolder = serializedObject.FindProperty(nameof(cellHolder));
        scale = serializedObject.FindProperty(nameof(scale));
        spacing = serializedObject.FindProperty(nameof(spacing));
        dimension = serializedObject.FindProperty(nameof(dimension));
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(placeOnAwake, new GUIContent("Place on Awake"));
        // EditorGUILayout.PropertyField(hideOnAwake, new GUIContent("Hide on Awake"));
        if(placeOnAwake.boolValue == true) EditorGUILayout.PropertyField(prefab, new GUIContent("Prefab"));
        EditorGUILayout.PropertyField(cellHolder, new GUIContent("Cell Holder"));
        EditorGUILayout.PropertyField(scale, new GUIContent("Scale"));
        EditorGUILayout.Slider(spacing, 0f, 10f, new GUIContent("Spacing"));
        EditorGUILayout.PropertyField(dimension, new GUIContent("Dimension"));

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Spawn cells")) Array.ForEach(Selection.gameObjects, co => co.GetComponent<Grid>()?.spawnGridCells());
        if (GUILayout.Button("Clear")) Array.ForEach(Selection.gameObjects, co => co.GetComponent<Grid>()?.clear());
    }
}

