using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

namespace Ryneus
{
    public class EnemyInfoComponent : MonoBehaviour
    {
        [SerializeField] private Image mainThumb;
        [SerializeField] private Image faceThumb;
        public Image FaceThumb => faceThumb;
        [SerializeField] private Image gridThumb;
        [SerializeField] private EnemySpriteSize enemySpriteSize;
        public Image MainThumb => mainThumb;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lv;
        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private StatusInfoComponent needStatusInfoComponent;
        [SerializeField] private TextMeshProUGUI gridKey;
        [SerializeField] private List<GameObject> actorOnlyGameObjects;
        [SerializeField] private List<SkillAttributeItem> weakPoints;

        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            if (battlerInfo == null)
            {
                Clear();
                return;
            }
            var enemyData = battlerInfo.EnemyData;
            UpdateData(enemyData);
            UIComponent.SetText(lv, battlerInfo.Level);
            if (statusInfoComponent != null)
            {
                HideActorOnly();
                statusInfoComponent.UpdateInfo(battlerInfo.Status);
                statusInfoComponent.UpdateHp(battlerInfo.Hp.Value, battlerInfo.MaxHp);
                statusInfoComponent.UpdateMp(battlerInfo.Mp.Value, battlerInfo.MaxMp);
            }
            if (needStatusInfoComponent != null)
            {
                UpdateNeedStatus(battlerInfo);
            }
            if (gridKey != null)
            {
                UpdateGridKey(battlerInfo.EnemyIndex.Value);
            }
            if (weakPoints != null)
            {
                UpdateWeakPoints(battlerInfo.WeakPoints);
            }
        }

        private void UpdateMainThumb(Image image, string imagePath, int x, int y, float scale, bool nativeSize)
        {
            UIComponent.SetImage(image, ResourceSystem.EnemySpritePath(imagePath), () =>
            {
                UIComponent.SetActive(image, true);
                var rect = image.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                if (nativeSize)
                {
                    UpdateNativeSize();
                }
            });
        }

        private void UpdateFaceThumb(Image image, string imagePath, int x, int y, float scale, bool nativeSize)
        {
            UIComponent.SetImage(image, ResourceSystem.EnemySpritePath(imagePath), () =>
            {
                UIComponent.SetActive(image, true);
                var rect = image.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                if (nativeSize)
                {
                    UpdateNativeSize();
                }
            });
        }

        public void UpdateData(EnemyData enemyData)
        {
            if (enemyData == null)
            {
                Clear();
                return;
            }
            if (mainThumb != null)
            {
                UpdateMainThumb(mainThumb, enemyData.ImagePath, 0, 0, 1.0f, false);
                mainThumb.gameObject.GetComponent<RectTransform>().localScale = new Vector2(enemyData.ImageScale,enemyData.ImageScale);
            }
            if (faceThumb != null)
            {
                UpdateFaceThumb(faceThumb, enemyData.ImagePath, 0, 0, 1.0f, true);
            }
            if (gridThumb != null)
            {
                UpdateMainThumb(gridThumb, enemyData.ImagePath, 0, 0, 1.0f, true);
            }
            if (enemySpriteSize != null)
            {
                enemySpriteSize.UpdateEnemy();
            }
            UIComponent.SetText(nameText, enemyData.Name);
        }

        public void UpdateGridKey(int index)
        {
            var textId = 16800 + index;
            UIComponent.SetText(gridKey, DataSystem.GetText(textId));
        }

        private void UpdateWeakPoints(List<KindType> kindTypes)
        {
            for (int i = 0;i < weakPoints.Count;i++)
            {
                UIComponent.SetActive(weakPoints[i].gameObject, kindTypes.Count > i);
                if (kindTypes.Count <= i)
                {
                    continue;
                }
                weakPoints[i].SetListData(new ListData(kindTypes[i]), i);
                weakPoints[i].UpdateViewItem();
                weakPoints[i].SetUnSelect();
            }
        }

        public void UpdateNativeSize()
        {
            if (mainThumb != null)
            {
                mainThumb.SetNativeSize();
            }
            if (gridThumb != null)
            {
                //gridThumb.SetNativeSize();
            }
        }

        public void Clear()
        {
            UIComponent.SetActive(mainThumb, false);
            UIComponent.ClearText(nameText);
            UIComponent.ClearText(gridKey);
        }

        private void HideActorOnly()
        {
            foreach (var actorOnlyGameObject in actorOnlyGameObjects)
            {
                UIComponent.SetActive(actorOnlyGameObject, false);
            }
        }

        private void UpdateNeedStatus(BattlerInfo battlerInfo)
        {
            if (needStatusInfoComponent != null)
            {
                var NeedStatus = new StatusInfo();
                NeedStatus.SetParameter(
                    battlerInfo.EnemyData.HpGrowth,
                    battlerInfo.EnemyData.MpGrowth,
                    battlerInfo.EnemyData.AtkGrowth,
                    battlerInfo.EnemyData.DefGrowth,
                    battlerInfo.EnemyData.SpdGrowth,
                    battlerInfo.EnemyData.MovGrouth,
                    0,
                    0
                    );
                needStatusInfoComponent.UpdateInfo(NeedStatus);
            }
        }
    }
}
