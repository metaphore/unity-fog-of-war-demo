using UnityEditor;
using UnityEngine;

namespace Fow.Utils
{
    public class LayerPropertyAttributeEditor
    {
        [CustomPropertyDrawer(typeof(LayerPropertyAttribute))]
        class LayerAttributeEditor : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            }
        }
    }
}