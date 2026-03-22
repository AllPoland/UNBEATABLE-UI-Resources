using UBUI.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SerializedReference<>))]
public class SerializedReferenceDrawer: PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new VisualElement();

        SerializedProperty valueProperty = property.FindPropertyRelative("Value");
        PropertyField valueField = new PropertyField(valueProperty, property.displayName);
        container.Add(valueField);

        return container;
    }
}