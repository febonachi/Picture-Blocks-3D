using System;
using UnityEditor;
using UnityEngine;

using Random = UnityEngine.Random;

namespace Utils{
    [Serializable] public class RandomizedFloat{
        [SerializeField] private bool randomize;
        [SerializeField] private float customValue;
        [SerializeField] private float randomValue;
        [SerializeField] private MinMaxValue minMaxValue;

        #region public properties
        public float value => randomize ? randomValue : customValue;
        public float randomizedValue => randomize ? minMaxValue.random : customValue;
        #endregion

        public static implicit operator RandomizedFloat(float value) {
            RandomizedFloat rf = new RandomizedFloat();
            rf.randomize = false;
            rf.customValue = value;
            rf.randomValue = value;
            return rf;
        }
        
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(RandomizedFloat))]
        public class RandomizedFloatPropertyDrawer : PropertyDrawer{
            private const int space = 5;
            
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){                
                EditorGUI.BeginProperty(position, label, property);
                int indentLevel = EditorGUI.indentLevel;

                position = EditorGUI.PrefixLabel(position, label);
                
                EditorGUI.indentLevel = 0;
                Rect toggleRect = new Rect(position.x, position.y, 24, position.height);
                EditorGUIUtility.labelWidth = toggleRect.width / 2;
                SerializedProperty randomize = property.FindPropertyRelative("randomize");
                EditorGUI.PropertyField(toggleRect, randomize, new GUIContent("R"));
                position.x += toggleRect.width + EditorGUIUtility.labelWidth + space;
                position.width -= toggleRect.width + toggleRect.width + EditorGUIUtility.labelWidth + space;
                if(randomize.boolValue) {
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("minMaxValue"), GUIContent.none);
                    if(!EditorApplication.isPlaying) { // generate random value only in editor mode
                        RandomizedFloat target = fieldInfo.GetValue(property.serializedObject.targetObject) as RandomizedFloat;
                        property.FindPropertyRelative("randomValue").floatValue = target.randomizedValue;
                    }else{
                        position.x += position.width + space;
                        position.width = toggleRect.width;
                        EditorGUI.LabelField(position, $"{property.FindPropertyRelative("randomValue").floatValue:F1}");
                    }
                } else EditorGUI.PropertyField(position, property.FindPropertyRelative("customValue"), new GUIContent("F"));

                EditorGUI.indentLevel = indentLevel;
                EditorGUI.EndProperty();
            }
        }
#endif
    }
}