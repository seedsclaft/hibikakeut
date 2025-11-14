using UnityEngine;
using TMPro;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ryneus
{
    [ExecuteAlways]
    public class TextColorEdit : MonoBehaviour
    {
        [SerializeField] private bool saveTexts = false;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private bool checkTexts = false;
        [SerializeField] private bool initTexts = false;
        public List<TextMeshProUGUI> editTexts = new();

# if UNITY_EDITOR
        void OnRenderObject()
        {
            if (Application.isPlaying)
            {
                return;
            }
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }
            if (initTexts)
            {
                initTexts = false;
                editTexts.Clear();
            }
            if (checkTexts)
            {
                checkTexts = false;
                CheckTexts(transform);
            }
            if (saveTexts)
            {
                saveTexts = false;
                foreach (var text in editTexts)
                {
                    text.color = textColor;
                }
            }
        }

        private void CheckTexts(Transform targetTransform)
        {
            foreach (Transform children in targetTransform)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(children.gameObject))
                {
                    Debug.Log("これはプレハブインスタンスです。");
                }
                else
                {
                    Debug.Log("これはプレハブインスタンスではありません。");
                    Debug.Log(children.gameObject.name);
                    var text = children.gameObject.GetComponent<TextMeshProUGUI>();
                    if (text != null && !editTexts.Contains(text))
                    {
                        editTexts.Add(text);
                    }
                    if (children.childCount > 0)
                    {
                        for (int i = 0; i < children.childCount; i++)
                        {
                            CheckTexts(children.GetChild(i).parent);
                        }
                    }
                }
            }
        }
#endif
    }
}
