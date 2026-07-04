using UnityEditor;
using UnityEngine;

namespace Ryneus
{
    [CustomPropertyDrawer(typeof(ParameterBool))]
    public class ParameterBoolDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 描画の開始（1つのプロパティとして一括処理するため）
            EditorGUI.BeginProperty(position, label, property);

            // 子要素である「_value」プロパティを見つける
            SerializedProperty valueProp = property.FindPropertyRelative("_value");

            if (valueProp != null)
            {
                // 元々のクラス名（ラベル）のまま、int型の入力フィールドとして描画する
                // これにより「▶」がなくなり、通常の int と同じ見た目になります
                EditorGUI.PropertyField(position, valueProp, label);
            }
            else
            {
                // 万が一プロパティが見つからない場合のフォールバック描画
                EditorGUI.LabelField(position, label.text, "Error: _value not found");
            }

            EditorGUI.EndProperty();
        }
    }

}
