using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillInfoComponent : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image iconBack;
        [SerializeField] private bool nameAndMpCost;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI mpCost;
        [SerializeField] private Image lineImage;
        [SerializeField] private Transform skillTypeBgRoot;
        [SerializeField] private TextMeshProUGUI type;
        [SerializeField] private TextMeshProUGUI value;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private GameObject descriptionListObj;
        [SerializeField] private GameObject descriptionListTarget;
        [SerializeField] private ScrollRect descriptionScrollRect;
        [SerializeField] private TextMeshProUGUI range;
        [SerializeField] private TextMeshProUGUI learningCost;
        [SerializeField] private GameObject countTurnRoot;
        [SerializeField] private TextMeshProUGUI countTurn;
        [SerializeField] private TextMeshProUGUI battleCountTurn;
        [SerializeField] private TextMeshProUGUI learningText;
        [SerializeField] private TextMeshProUGUI rank;
        [SerializeField] private GameObject selectable;
        [SerializeField] private GameObject selectedAlcana;
        [SerializeField] private StatusGaugeAnimation skillExpGauge;
        [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;

        public void UpdateInfo(SkillInfo skillInfo)
        {
            if (skillInfo == null)
            {
                Clear();
                return;
            }
            UpdateData(skillInfo.Id.Value);
            if (description != null)
            {
                var convertHelpText = skillInfo.ConvertHelpText();
                description?.SetText(convertHelpText);
                if (descriptionListObj != null && descriptionListTarget != null)
                {
                    var length = Math.Max(3, convertHelpText.Split("\n").Length);
                    var height = 32 + 24 * length;
                    descriptionListObj.GetComponent<RectTransform>().sizeDelta = new Vector2(440,height);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionListTarget.GetComponent<RectTransform>());
                    descriptionScrollRect.vertical = length > 3;
                }
            }
            if (selectable != null)
            {
                selectable.SetActive(skillInfo.LearningState == LearningState.SelectLearn);
            }
            if (learningCost != null)
            {
                learningCost.gameObject.SetActive(skillInfo.LearningCost.Value > 0);
                learningCost.SetText(skillInfo.LearningCost.Value.ToString());
                battleCountTurn?.gameObject?.SetActive(false);
            } else
            {
                battleCountTurn?.gameObject?.SetActive(skillInfo.Master.SkillType == SkillType.Active || (skillInfo.Master.SkillType == SkillType.Passive && skillInfo.Master.CountTurn > 0));
                battleCountTurn?.SetText(skillInfo.CountTurn.Value.ToString());
            }
            if (learningText != null)
            {
                if (skillInfo.LearningState == LearningState.NotLearnedByAlchemy)
                {
                    learningText.transform.parent.gameObject.SetActive(skillInfo.LearningState == LearningState.NotLearnedByAlchemy);
                    learningText.SetText(DataSystem.GetText(381));
                } else
                if (skillInfo.LearningState == LearningState.NotLearn)
                {
                    learningText.transform.parent.gameObject.SetActive(skillInfo.LearningState == LearningState.NotLearn);
                    learningText.SetText(DataSystem.GetReplaceText(2500, skillInfo.LearningLv.Value.ToString()));
                } else
                {
                    learningText.transform.parent.gameObject.SetActive(false);
                }
            }
            if (skillExpGauge != null)
            {
                var displayExp = !skillInfo.Master.IsBattleSpecialSkill() && skillInfo.Master.SkillType != SkillType.Equip && skillInfo.Master.Rank > RankType.ActiveRank1 && !skillInfo.PrimitiveLearned.Value;
                skillExpGauge.gameObject.SetActive(displayExp);
                skillExpGauge.UpdateGauge(skillInfo.ExpRate.Value);
                if (learningCost != null && displayExp && skillInfo.ExpRate.Value >= 1)
                {
                    learningCost.text = DataSystem.PowerUpColorTag + learningCost.text + "</color>";
                }
            }
        }

        public void UpdateData(int skillId)
        {
            if (skillId == 0)
            {
                Clear();
                return;
            }
            var skillData = DataSystem.FindSkill(skillId);
            if (skillData != null)
            {
                if (icon != null)
                {
                    icon.gameObject.SetActive(true);
                    UpdateSkillIcon(skillData.IconIndex);
                }
                if (iconBack != null)
                {
                    iconBack.gameObject.SetActive(true);
                    UpdateSkillIconBack(skillData.Attribute);
                }
                if (nameText != null)
                {
                    nameText.SetText(skillData.Name);
                    if (nameAndMpCost)
                    {
                        nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
                    }
                }
                var mpCostText = skillData.SkillType == SkillType.Active ? "(" + skillData.MpCost.ToString() + ")" : "";
                mpCost?.SetText(mpCostText);
                type?.SetText(skillData.SkillType.ToString());
                if (skillTypeBgRoot != null)
                {
                    var count = 1;
                    var skillType = (int)skillData.SkillType;
                    foreach (Transform child in skillTypeBgRoot.transform)
                    {
                        child.gameObject.SetActive(skillType == count);
                        count++;
                    }
                }
                if (countTurnRoot != null)
                {
                    countTurnRoot.SetActive(skillData.SkillType == SkillType.Active || (skillData.SkillType == SkillType.Passive && skillData.CountTurn > 0));
                }
                countTurn?.SetText(skillData.CountTurn.ToString());
                rank?.gameObject?.SetActive(true);
                rank?.gameObject?.transform.parent.gameObject.SetActive(true);
                UpdateSkillRank(skillData.Rank);
            } else
            {
                Clear();
            }
            if (lineImage != null)
            {
                UpdateLineImage();
            }
            if (range != null)
            {
                //range.gameObject.SetActive(true);
                var rangeTextId = skillData.Range == RangeType.S ? 2210 : 2220;
                range.SetText(DataSystem.GetText(rangeTextId));
            }
        }

        private void UpdateSkillIcon(MagicIconType iconIndex)
        {
            icon.gameObject.SetActive(true);
            var spriteAtlas = ResourceSystem.LoadSpellIcons();
            if (icon != null)
            {
                icon.sprite = spriteAtlas.GetSprite(iconIndex.ToString());
                //icon.sprite = ResourceSystem.LoadSBuffIcon((int)iconIndex);
            }
        }

        private void UpdateSkillIconBack(AttributeType attributeType)
        {
            iconBack.gameObject.SetActive(true);

            //var spriteAtlas = ResourceSystem.LoadSpellIcons();
            if (iconBack != null)
            {
                //iconBack.sprite = spriteAtlas.GetSprite(attributeType.ToString());
                iconBack.sprite = ResourceSystem.LoadSpellIconBase(attributeType);
            }
        }

        private void UpdateLineImage()
        {
            lineImage.gameObject.SetActive(true);
            if (nameAndMpCost)
            {
                nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth,nameText.preferredHeight);
                lineImage.rectTransform.sizeDelta = new Vector2(nameText.rectTransform.sizeDelta.x,lineImage.rectTransform.sizeDelta.y);
            }
        }

        private void UpdateSkillRank(RankType rankType)
        {
            var textId = 2300 + (SkillData.ConvertRankCost(rankType) * 10);
            if (rankType == RankType.ActiveRank1)
            {
                textId = 2310;
            } else
            if (rankType >= RankType.Uniq)
            {
                textId = 2340;
            } else
            if (rankType >= RankType.RelicRank1)
            {
                textId = 2330;
            }
            rank?.SetText(DataSystem.GetText(textId));
        }

        public void SetName(string name)
        {
            nameText.SetText(name);
        }

        public void Clear()
        {
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }
            if (iconBack != null)
            {
                iconBack.gameObject.SetActive(false);
            }
            nameText?.SetText("");
            mpCost?.SetText("");
            type?.SetText("");
            if (skillTypeBgRoot != null)
            {
                foreach (Transform child in skillTypeBgRoot.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
            description?.SetText("");
            if (lineImage != null)
            {
                lineImage.gameObject.SetActive(false);
            }
            if (learningCost != null)
            {
                learningCost?.gameObject.SetActive(false);
                learningCost?.SetText("");
            }
            range?.gameObject.SetActive(false);
            range?.SetText("");
            countTurn?.gameObject?.SetActive(false);
            rank?.gameObject?.SetActive(false);
            rank?.gameObject?.transform.parent.gameObject.SetActive(false);
            if (learningText != null)
            {
                learningText.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}