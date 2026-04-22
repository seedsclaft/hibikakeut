using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Utage;
using System.Linq;

namespace Ryneus
{
    public class AdvTextSizeFormat : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshPro messageText;
        [SerializeField] private AdvPage advPage;
        private int _needSizeFix = 0;
        private float _initY = 0;
        private string _lastLayerName = "";

        private void Awake()
        {
            _initY = this.gameObject.GetComponent<RectTransform>().localPosition.y;
            advPage.OnBeginPage.AddListener((a) =>
            {
                foreach (var command in a.CurrentData.CommandList)
                {
                    if (command.GetType() == typeof(Utage.AdvCommandCharacter))
                    {
                        var c = command as Utage.AdvCommandCharacter;
                        _lastLayerName = c.ParseCell<string>(AdvColumnName.Arg3);
                    }
                }
                var f = a.CurrentData.CommandList;
            });
        }

        public void ResetFormatText()
        {
            var vectorZero = new Vector2(0, _initY);
            messageText.rectTransform.sizeDelta = vectorZero;
            rectTransform.sizeDelta = vectorZero;
            //messageText.rectTransform.localPosition = vectorZero;
            rectTransform.localPosition = vectorZero;
        }

        public void UpdateFormatText()
        {
            messageText.rectTransform.sizeDelta = new Vector2(messageText.preferredWidth, messageText.preferredHeight);
            rectTransform.sizeDelta = new Vector2(messageText.rectTransform.sizeDelta.x + 80, messageText.rectTransform.sizeDelta.y + 12);
            if (messageText.preferredWidth == 0)
            {
                _needSizeFix = 1;
            }
            if (_lastLayerName == "Character2")
            {
                //messageText.rectTransform.localPosition = new Vector2(240, 0);
                rectTransform.localPosition = new Vector2(240, _initY);
            }
            if (_lastLayerName == "Character0")
            {
                //messageText.rectTransform.localPosition = new Vector2(240, 0);
                rectTransform.localPosition = new Vector2(-240, _initY);
            }
        }

        public void OnDrawGraphicObject()
        {
            
        }

        private void LateUpdate()
        {
            if (_needSizeFix == 1)
            {
                _needSizeFix--;
                UpdateFormatText();
            }
        }
    }
}
