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
        [SerializeField] private TextMeshProUGUI useCount;
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
                UIComponent.SetText(description, convertHelpText);
                if (descriptionListObj != null && descriptionListTarget != null)
                {
                    var length = Math.Max(3, convertHelpText.Split("\n").Length);
                    var height = 32 + 24 * length;
                    descriptionListObj.GetComponent<RectTransform>().sizeDelta = new Vector2(440, height);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionListTarget.GetComponent<RectTransform>());
                    descriptionScrollRect.vertical = length > 3;
                }
            }
            UIComponent.SetActive(selectable, skillInfo.LearningState == LearningState.SelectLearn);
            if (learningCost != null)
            {
                UIComponent.SetActive(learningCost, skillInfo.LearningCost.Value > 0);
                UIComponent.SetText(learningCost, skillInfo.LearningCost);
                UIComponent.SetActive(battleCountTurn?.gameObject, false);
            }
            else
            {
                UIComponent.SetActive(battleCountTurn?.gameObject, skillInfo.Master.SkillType == SkillType.Active || (skillInfo.Master.SkillType == SkillType.Passive && skillInfo.Master.CountTurn > 0));
                UIComponent.SetText(battleCountTurn, skillInfo.CountTurn);
            }
            if (learningText != null)
            {
                if (skillInfo.LearningState == LearningState.NotLearnedByAlchemy)
                {
                    UIComponent.SetActive(learningText.transform.parent.gameObject, skillInfo.LearningState == LearningState.NotLearnedByAlchemy);
                    UIComponent.SetText(learningText, DataSystem.GetText(381));
                }
                else
                if (skillInfo.LearningState == LearningState.NotLearn)
                {
                    UIComponent.SetActive(learningText.transform.parent.gameObject, skillInfo.LearningState == LearningState.NotLearn);
                    if (skillInfo.LearningLv.Value >= 0)
                    {
                        UIComponent.SetText(learningText, DataSystem.GetReplaceText(2500, skillInfo.LearningLv.Value.ToString()));
                    }
                    else
                    {
                        UIComponent.SetText(learningText, DataSystem.GetText(2510));
                    }
                }
                else
                {
                    UIComponent.SetActive(learningText.transform.parent.gameObject, false);
                }
            }
            if (skillExpGauge != null)
            {
                var displayExp = !skillInfo.Master.IsBattleSpecialSkill() && skillInfo.Master.SkillType != SkillType.Kind && skillInfo.Master.SkillType != SkillType.Equip && skillInfo.Master.Rank > RankType.ActiveRank1 && !skillInfo.PrimitiveLearned.Value;
                UIComponent.SetActive(skillExpGauge.gameObject, displayExp);
                skillExpGauge.UpdateGauge(skillInfo.ExpRate.Value);
                if (learningCost != null && displayExp && skillInfo.ExpRate.Value >= 1)
                {
                    learningCost.text = DataSystem.PowerUpColorTag + learningCost.text + "</color>";
                }
            }
            if (useCount != null && skillInfo.RemainUseCount() != 99)
            {
                UIComponent.SetActive(useCount, true);
                UIComponent.SetText(useCount, DataSystem.GetReplaceText(2610, skillInfo.RemainUseCount().ToString()));
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
            if (skillData == null)
            {
                Clear();
            }
            if (icon != null)
            {
                UIComponent.SetActive(icon, true);
                UpdateSkillIcon(skillData.IconIndex);
            }
            if (iconBack != null)
            {
                UIComponent.SetActive(iconBack, true);
                UpdateSkillIconBack(skillData.Attribute);
            }
            if (nameText != null)
            {
                UIComponent.SetText(nameText, skillData.Name);
                if (nameAndMpCost)
                {
                    nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth, nameText.preferredHeight);
                }
            }
            var mpCostText = skillData.SkillType == SkillType.Active ? "(" + skillData.MpCost.ToString() + ")" : "";
            UIComponent.SetText(mpCost, mpCostText);
            UIComponent.SetText(type, skillData.SkillType.ToString());
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
            UIComponent.SetActive(countTurnRoot, skillData.SkillType == SkillType.Active || (skillData.SkillType == SkillType.Passive && skillData.CountTurn > 0));
            UIComponent.SetText(countTurn, skillData.CountTurn.ToString());
            if (rank != null)
            {
                UIComponent.SetActive(rank, true);
                UIComponent.SetActive(rank.gameObject.transform.parent.gameObject, true);
                UIComponent.SetActive(useCount, false);
                UpdateSkillRank(skillData.Rank);
            }
            if (lineImage != null)
            {
                UpdateLineImage();
            }
            var rangeTextId = skillData.Range == RangeType.S ? 2210 : 2220;
            UIComponent.SetText(range, DataSystem.GetText(rangeTextId));
        }

        private void UpdateSkillIcon(MagicIconType iconIndex)
        {
            UIComponent.SetActive(icon, true);
            var spriteAtlas = ResourceSystem.LoadSpellIcons();
            if (icon != null)
            {
                icon.sprite = spriteAtlas.GetSprite(iconIndex.ToString());
            }
        }

        private void UpdateSkillIconBack(AttributeType attributeType)
        {
            UIComponent.SetActive(iconBack, true);

            if (iconBack != null)
            {
                iconBack.sprite = ResourceSystem.LoadSpellIconBase(attributeType);
            }
        }

        private void UpdateLineImage()
        {
            UIComponent.SetActive(lineImage, true);
            if (nameAndMpCost)
            {
                nameText.rectTransform.sizeDelta = new Vector2(nameText.preferredWidth, nameText.preferredHeight);
                lineImage.rectTransform.sizeDelta = new Vector2(nameText.rectTransform.sizeDelta.x, lineImage.rectTransform.sizeDelta.y);
            }
        }

        private void UpdateSkillRank(RankType rankType)
        {
            var textId = 2300 + (SkillData.ConvertRankCost(rankType) * 10);
            if (rankType == RankType.ActiveRank1)
            {
                textId = 2310;
            }
            else
            if (rankType >= RankType.Uniq)
            {
                textId = 2340;
            }
            else
            if (rankType >= RankType.RelicRank1)
            {
                textId = 2330;
            }
            UIComponent.SetText(rank, DataSystem.GetText(textId));
        }

        public void SetName(string name)
        {
            UIComponent.SetText(nameText, name);
        }

        public void Clear()
        {
            UIComponent.SetActive(icon, false);
            UIComponent.SetActive(iconBack, false);
            UIComponent.ClearText(nameText);
            UIComponent.ClearText(mpCost);
            UIComponent.ClearText(type);
            if (skillTypeBgRoot != null)
            {
                foreach (Transform child in skillTypeBgRoot.transform)
                {
                    UIComponent.SetActive(child.gameObject, false);
                }
            }
            UIComponent.ClearText(description);
            UIComponent.SetActive(lineImage, false);
            UIComponent.SetActive(learningCost, false);
            UIComponent.ClearText(learningCost);
            UIComponent.SetActive(range, false);
            UIComponent.ClearText(range);
            UIComponent.SetActive(countTurnRoot, false);
            UIComponent.SetActive(rank, false);
            rank?.gameObject?.transform.parent.gameObject.SetActive(false);
            if (learningText != null)
            {
                learningText.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}