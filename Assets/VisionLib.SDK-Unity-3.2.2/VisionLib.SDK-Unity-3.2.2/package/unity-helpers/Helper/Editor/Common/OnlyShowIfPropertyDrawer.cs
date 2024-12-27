using UnityEditor;
using UnityEngine;

namespace Visometry.Helpers
{
    [CustomPropertyDrawer(typeof(OnlyShowIfAttribute))]
    public class OnlyShowIfPropertyDrawer : PropertyDrawer
    {
        private bool showProperty = true;
        private SerializedProperty propertyThatManagesMe;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.showProperty = DrawProperty(property);

            if (!this.showProperty)
            {
                return;
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!this.showProperty)
            {
                return 0;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool DrawProperty(SerializedProperty property)
        {
            OnlyShowIfAttribute onlyShowIfAttribute = attribute as OnlyShowIfAttribute;
            string fieldToCheckName = onlyShowIfAttribute.fieldToCheck;
            bool showOnValue = onlyShowIfAttribute.showOnValue;

            if (this.propertyThatManagesMe == null)
            {
                this.propertyThatManagesMe =
                    property.serializedObject.FindProperty(fieldToCheckName);
            }

            if (this.propertyThatManagesMe == null)
            {
                Debug.LogWarning(
                    "Target value '" + fieldToCheckName +
                    "' of the 'OnlyShowIf' attribute does not exist.");
                return true;
            }

            if (this.propertyThatManagesMe.type != "bool")
            {
                Debug.LogWarning(
                    "Target value '" + fieldToCheckName +
                    "' of the 'OnlyShowIf' attribute must be a bool property.");
                return true;
            }
            return showOnValue
                ? this.propertyThatManagesMe.boolValue
                : !this.propertyThatManagesMe.boolValue;
        }
    }
}
