using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SceneEffect
{
    public enum EffectType
    {
        ChangeGameState,
        PlaySoundClip,
        WaitForSeconds,
        FunctionCall,
    }

    [SerializeField] [HideInInspector] private EffectType _type = EffectType.FunctionCall;
    public EffectType Type => _type;

    [SerializeField] [HideInInspector] public float Time = 0f;
    [SerializeField] [HideInInspector] public UnityEvent Function;
    [SerializeField] [HideInInspector] public GameState GameState;
    [SerializeField] [HideInInspector] public bool IsBGM = true;
    [SerializeField] [HideInInspector] public AudioClip Clip;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneEffect))]
public class SceneEffectDrawer : PropertyDrawer
{
    private int DEFAULT_HEIGHT = 18;
    private int EXTEND_HEIGHT = 24;
    private UnityEventDrawer _eventDrawer;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        int originIndent = EditorGUI.indentLevel;
        var typeProperty = property.FindPropertyRelative("_type");

        Rect folderRect = new Rect();
        folderRect = EditorGUI.IndentedRect(new Rect(position.x, position.y, position.width, DEFAULT_HEIGHT));

        position.y += DEFAULT_HEIGHT + 2;
        property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(folderRect, property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            Rect contentRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("Effect Type"));
            Rect fieldRect = EditorGUI.IndentedRect(new Rect(contentRect.x, contentRect.y, contentRect.width, DEFAULT_HEIGHT));

            EditorGUI.PropertyField(fieldRect, typeProperty, GUIContent.none);
            position.y += DEFAULT_HEIGHT + 2; // 2. 첫 번째 필드 위치 설정

            switch ((SceneEffect.EffectType)typeProperty.enumValueIndex)
            {
                case SceneEffect.EffectType.ChangeGameState:
                    DrawField("GameState", position, property);
                    break;
                case SceneEffect.EffectType.PlaySoundClip:
                    DrawField("IsBGM", position, property);
                    position.y += DEFAULT_HEIGHT + 2; // 2. 첫 번째 필드 위치 설정
                    DrawField("Clip", position, property);
                    break;
                case SceneEffect.EffectType.WaitForSeconds:
                    DrawField("Time", position, property);
                    break;
                case SceneEffect.EffectType.FunctionCall:
                    DrawField("Function", position, property);
                    break;
            }
        }


        EditorGUI.indentLevel = originIndent;
        EditorGUI.EndFoldoutHeaderGroup();
        EditorGUI.EndProperty();
    }

    private void DrawField(string fieldName, Rect position, SerializedProperty property, string inspectorName = "")
    {
        if (inspectorName == "") inspectorName = SplitCamelCase(fieldName);
        Rect contentRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent(inspectorName));
        Rect fieldRect = EditorGUI.IndentedRect(new Rect(contentRect.x, contentRect.y, contentRect.width, DEFAULT_HEIGHT));
        EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative(fieldName), GUIContent.none);
    }
    public static string SplitCamelCase(string str)
    {
        return Regex.Replace(
            Regex.Replace(
                str,
                @"(\P{Ll})(\P{Ll}\p{Ll})",
                "$1 $2"
            ),
            @"(\p{Ll})(\P{Ll})",
            "$1 $2"
        );
    }
    private float GetEventHeight(SerializedProperty property)
    {
        if (_eventDrawer == null)
            _eventDrawer = new UnityEventDrawer();
        return _eventDrawer.GetPropertyHeight(property, new GUIContent("Function"));
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.isExpanded)
        {
            var effectType = (SceneEffect.EffectType)property.FindPropertyRelative("_type").enumValueIndex;

            if (effectType == SceneEffect.EffectType.FunctionCall)
                return (EXTEND_HEIGHT + 2) * 2 + GetEventHeight(property.FindPropertyRelative("Function"));

            return (EXTEND_HEIGHT + 2) * 3;
        }
        else
            return (EXTEND_HEIGHT + 2);
    }
}
#endif