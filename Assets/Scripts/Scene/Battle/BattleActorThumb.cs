using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class BattleActorThumb : MonoBehaviour
    {
        [SerializeField] private GameObject effectFront = null;
        [SerializeField] private GameObject effectBack = null;
        [SerializeField] private Image mainThumb = null;

        public void SetActorData(ActorData actorData)
        {
            var scale = actorData.Scale;
            var rect = mainThumb.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(actorData.X, actorData.Y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            rect.sizeDelta = new Vector3(mainThumb.mainTexture.width, mainThumb.mainTexture.height, 1);
            effectFront.GetComponent<RectTransform>().localScale = new (scale, scale);
            effectBack.GetComponent<RectTransform>().localScale = new (scale, scale);
        }

        public void SetAwaken(bool isAwaken)
        {
            effectFront.SetActive(isAwaken);
            effectBack.SetActive(isAwaken);
        }
    }
}
