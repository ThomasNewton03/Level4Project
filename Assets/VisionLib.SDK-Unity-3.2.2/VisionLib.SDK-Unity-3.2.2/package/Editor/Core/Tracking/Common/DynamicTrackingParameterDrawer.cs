using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Visometry.VisionLib.SDK.Core
{
    public abstract class DynamicTrackingParameterDrawer<TParameterValue> : PropertyDrawer
    {
        protected const float indentValue = 15;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            const float foldoutWidth = 5;
            const float checkboxWidth = 20;

            var indentation = EditorGUI.indentLevel *
                              DynamicTrackingParameterDrawer<float>.indentValue;

            label = EditorGUI.BeginProperty(position, label, property);
            using var verticalScope = new EditorGUILayout.VerticalScope();
            var maxWidth = position.width - 35f;
            var bodyRect = GUILayoutUtility.GetRect(100f, maxWidth, 15, 20);
            var enabledCheckboxRect = new Rect(
                new Vector2(bodyRect.position.x + foldoutWidth, bodyRect.position.y),
                new Vector2(checkboxWidth, bodyRect.height));
            var foldoutRect = new Rect(
                bodyRect.position,
                new Vector2(bodyRect.width * 0.4f, bodyRect.height));
            var labelRect = new Rect(
                new Vector2(bodyRect.position.x + (checkboxWidth + foldoutWidth), bodyRect.position.y),
                new Vector2(bodyRect.width * 0.4f - (checkboxWidth + foldoutWidth) + indentation, bodyRect.height));
            var contentRect = new Rect(
                new Vector2(bodyRect.position.x + bodyRect.width * 0.4f + indentation, bodyRect.position.y),
                new Vector2(bodyRect.width * 0.6f - indentation, bodyRect.height));

            var paramHandler = (IParameterHandler) property.serializedObject.targetObject;
            var parameter = GetField<DynamicTrackingParameter<TParameterValue>>(property);
            var valueProperty = ExtractValueProperty(property);
            var enabledProperty = ExtractUseValueFromUnityProperty(property);

            enabledProperty.boolValue = EditorGUI.Toggle(
                enabledCheckboxRect,
                GUIContent.none,
                enabledProperty.boolValue);
            property.isExpanded = EditorGUI.Foldout(
                foldoutRect,
                property.isExpanded,
                GUIContent.none,
                true);
            EditorGUI.LabelField(
                labelRect,
                new GUIContent(parameter.GetDescriptiveName(), parameter.GetDescription()),
                EditorStyles.wordWrappedLabel);

            if (enabledProperty.boolValue)
            {
                DrawParameterSetterField(contentRect, parameter, valueProperty);
            }
            else if (paramHandler.ActiveInBackend())
            {
                DrawDisabledParameterValueLabel(contentRect, ToDisplayString(parameter));
            }

            if (!property.isExpanded)
            {
                return;
            }
            EditorGUI.indentLevel++;
            DrawIndentedSection(property);
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        protected abstract void DrawParameterSetterField(
            Rect contentRect,
            DynamicTrackingParameter<TParameterValue> parameter,
            SerializedProperty property);

        protected virtual string ToDisplayString(
            DynamicTrackingParameter<TParameterValue> parameter)
        {
            return parameter.GetValue().ToString();
        }

        private static void DrawDisabledParameterValueLabel(Rect contentRect, string valueString)
        {
            GUI.Label(
                contentRect,
                new GUIContent(
                    valueString,
                    "The current value of the parameter in the backend. " +
                    "To edit the parameter value, enable this parameter."),
                EditorStyles.wordWrappedLabel);
        }

        protected virtual void DrawIndentedSection(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(ExtractEventProperty(property));
        }

        private static SerializedProperty ExtractUseValueFromUnityProperty(
            SerializedProperty dynamicTrackingParameterProperty)
        {
            return dynamicTrackingParameterProperty.FindPropertyRelative("useValueFromUnity");
        }

        protected static SerializedProperty ExtractValueProperty(
            SerializedProperty dynamicTrackingParameterProperty)
        {
            return dynamicTrackingParameterProperty.FindPropertyRelative("value");
        }

        protected static SerializedProperty ExtractEventProperty(
            SerializedProperty dynamicTrackingParameterProperty)
        {
            return dynamicTrackingParameterProperty.FindPropertyRelative("onValueChanged");
        }

        private static T GetField<T>(SerializedProperty property)
        {
            object targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();

            var path = property.propertyPath.Split('.');
            FieldInfo field = null;
            for (var i = 0; i < path.Length; i++)
            {
                field = GetFieldInTypeOrBaseType(targetObjectClassType, path[i]);
                if (field == null || i + 1 == path.Length)
                {
                    break;
                }

                // Extract Object with specified name
                targetObject = field.GetValue(targetObject);
                targetObjectClassType = targetObject.GetType();
            }
            return (T) field?.GetValue(targetObject);
        }

        private static FieldInfo GetFieldInTypeOrBaseType(Type type, string fieldName)
        {
            const BindingFlags fieldBindingFlags = BindingFlags.Instance | BindingFlags.Public |
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.FlattenHierarchy;
            FieldInfo field;
            do
            {
                field = type.GetField(fieldName, fieldBindingFlags);
                type = type.BaseType;
            }
            while (field == null && type != typeof(object));
            return field;
        }
    }
}
