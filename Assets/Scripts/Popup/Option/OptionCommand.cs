using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Ryneus
{
    public class OptionCommand : ListItem, IListViewItem
    {
        [SerializeField] private TextMeshProUGUI optionName;
        [SerializeField] private TextMeshProUGUI optionHelp;
        [SerializeField] private OptionVolume optionVolume;
        [SerializeField] private List<Toggle> optionToggles;
        [SerializeField] private List<TextMeshProUGUI> optionTexts;
        [SerializeField] private Button minusButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private TextMeshProUGUI resolution;


        private bool _isInitEvent = false;

        public void UpdateViewItem()
        {
            if (ListData == null)
            {
                return;
            }
            var optionInfo = ListItemData<OptionInfo>();
            var data = optionInfo.OptionCommand;
            UIComponent.SetText(optionName, data.Name);
            UIComponent.SetText(optionHelp, data.Help);
            SetResolutionText();
 
            UIComponent.SetActive(optionVolume.gameObject, data.ButtonType == OptionButtonType.Slider);
            optionToggles.ForEach(a => UIComponent.SetActive(a.gameObject, data.ButtonType == OptionButtonType.Toggle));

            UIComponent.SetActive(minusButton.gameObject, data.ButtonType == OptionButtonType.Resolution);
            UIComponent.SetActive(plusButton.gameObject, data.ButtonType == OptionButtonType.Resolution);
            UIComponent.SetActive(resolution.gameObject, data.ButtonType == OptionButtonType.Resolution);

            if (data.ToggleText1 > 0)
            {
                optionTexts[0].text = DataSystem.GetText(data.ToggleText1);
            }
            else
            {
                UIComponent.SetActive(optionToggles[0].gameObject, false);
            }
            if (data.ToggleText2 > 0)
            {
                optionTexts[1].text = DataSystem.GetText(data.ToggleText2);
            }
            else
            {
                UIComponent.SetActive(optionToggles[1].gameObject, false);
            }
            if (data.ToggleText3 > 0)
            {
                optionTexts[2].text = DataSystem.GetText(data.ToggleText3);
            }
            else
            {
                UIComponent.SetActive(optionToggles[2].gameObject, false);
            }
            UpdateOptionValues(data);

            if (!_isInitEvent)
            {
                if (optionInfo.SliderEvent != null && optionInfo.MuteEvent != null)
                {
                    optionVolume.Initialize(optionInfo);
                }
                if (optionInfo.ToggleEvent != null)
                {
                    var toggleIndex = 0;
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        var idx = toggleIndex;
                        optionToggles[i].onValueChanged.AddListener((a) =>
                        {
                            if (a)
                            {
                                optionInfo.ToggleEvent(idx, optionInfo);
                            }
                        });
                        toggleIndex++;
                    }
                }
                if (optionInfo.PlusMinusEvent != null)
                {
                    minusButton.onClick.AddListener(() =>
                    {
                        optionInfo.PlusMinusEvent(-1, optionInfo);
                    });
                    plusButton.onClick.AddListener(() =>
                    {
                        optionInfo.PlusMinusEvent(1, optionInfo);
                    });
                }
                _isInitEvent = true;
            }
        }

        private void UpdateOptionValues(SystemData.OptionCommand optionCommand)
        {
            switch (optionCommand.Key)
            {
                case "SCREEN_MODE":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.ScreenMode == false ? 0 : 1));
                    }
                    return;
                case "SCREEN_SIZE":
                    SetResolutionText();
                    return;
                case "BGM_VOLUME":
                    optionVolume.UpdateValue(SoundManager.Instance.BgmVolume, SoundManager.Instance.BGMMute);
                    return;
                case "SE_VOLUME":
                    optionVolume.UpdateValue(SoundManager.Instance.SeVolume, SoundManager.Instance.SeMute);
                    return;
                case "GRAPHIC_QUALITY":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        var notify = (i == 0 && GameSystem.OptionData.GraphicIndex == 2) || (i == 1 && GameSystem.OptionData.GraphicIndex == 1);
                        optionToggles[i].SetIsOnWithoutNotify(notify);
                    }
                    return;
                case "EVENT_SKIP":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.EventSkipIndex == true ? 1 : 0));
                    }
                    return;
                case "EVENT_TEXT_SKIP":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.EventTextSkipIndex == true ? 1 : 0));
                    }
                    return;
                case "COMMAND_END_CHECK":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.CommandEndCheck == true ? 0 : 1));
                    }
                    return;
                case "BATTLE_WAIT":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.BattleTurnSkip == true ? 1 : 0));
                    }
                    return;
                case "BATTLE_ANIMATION":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.BattleAnimationSkip == true ? 1 : 0));
                    }
                    return;
                case "BATTLE_FIRST_PASSIVE_WAIT":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.BattlePassiveAnimationSkip == true ? 1 : 0));
                    }
                    return;
                case "BATTLE_CAMERA_MOVE":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.BattleCameraMove == true ? 1 : 0));
                    }
                    return;
                case "INPUT_TYPE":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (int)GameSystem.TempData.TempInputType);
                    }
                    return;
                case "BATTLE_AUTO":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.BattleAuto == true ? 1 : 0));
                    }
                    return;
                case "BATTLE_SPEED":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(OptionUtility.SpeedList[i + 1] == GameSystem.OptionData.BattleSpeed);
                    }
                    return;
                case "TUTORIAL_CHECK":
                    for (int i = 0; i < optionToggles.Count; i++)
                    {
                        optionToggles[i].SetIsOnWithoutNotify(i == (GameSystem.OptionData.TutorialCheck == true ? 0 : 1));
                    }
                    return;
            }
        }

        private void SetResolutionText()
        {
            UIComponent.SetText(resolution, GameSystem.OptionData.ScreenWidth.ToString() + " x " + GameSystem.OptionData.ScreenHeight.ToString());
        }
    }
}