using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    using Option;
    public class OptionPresenter : BasePresenter
    {
        OptionView _view = null;

        OptionModel _model = null;
        private bool _busy = true;
        public OptionPresenter(OptionView view)
        {
            _view = view;
            _model = new OptionModel();
            _model.ChangeTempInputType(GameSystem.ConfigData.InputType);

            SetView(_view);
            SetModel(_model);
            Initialize();
            _busy = false;
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetOptionCategoryList(MakeListData(_model.OptionCategoryList()));
            _view.SetHelpWindow();
            CommandSelectCategory();
            _view.OpenAnimation();
        }
        
        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Option)
            {
                return;
            }
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.ChangeOptionValue:
                    CommandOptionValue((OptionInfo)viewEvent.template);
                    break;
                case CommandType.SelectCategory:
                    CommandSelectCategory();
                    break;
                case CommandType.SelectOptionList:
                    CommandSelectOptionList();
                    break;
                case CommandType.CancelOptionList:
                    CommandCancelOptionList();
                    break;
                case CommandType.DecideCategory:
                    CommandDecideCategory();
                    break;
            }
        }

        private void CommandOptionValue(OptionInfo data)
        {
            var inputKeyType = data.keyType;
            switch (data.OptionCommand.Key)
            {
                case "SCREEN_MODE":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeScreenMode(true);
                    } else
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeScreenMode(false);
                    }
                    break;
                case "SCREEN_SIZE":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeScreenSize(true);
                    } else
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeScreenSize(false);
                    }
                    break;
                case "BGM_VOLUME":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeBGMValue(Mathf.Min(1, SoundManager.Instance.BgmVolume + 0.05f));
                    } else
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeBGMValue(Mathf.Max(0, SoundManager.Instance.BgmVolume - 0.05f));
                    } else
                    if (inputKeyType == InputKeyType.Option1)
                    {
                        ConfigUtility.ChangeBGMMute(!SoundManager.Instance.BGMMute);
                    }
                    break;
                case "SE_VOLUME":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeSEValue(Mathf.Min(1, SoundManager.Instance.SeVolume + 0.05f));
                    } else
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeSEValue(Mathf.Max(0, SoundManager.Instance.SeVolume - 0.05f));
                    } else
                    if (inputKeyType == InputKeyType.Option1)
                    {
                        ConfigUtility.ChangeSEMute(!SoundManager.Instance.SeMute);
                    }
                    break;
                case "GRAPHIC_QUALITY":
                    if (inputKeyType == InputKeyType.Right)
                    {
                        ConfigUtility.ChangeGraphicIndex(1);
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeGraphicIndex(2);
                    };
                    break;
                case "EVENT_SKIP":
                    ConfigUtility.ChangeEventSkipIndex(inputKeyType == InputKeyType.Right);
                    break;
                case "COMMAND_END_CHECK":
                    ConfigUtility.ChangeCommandEndCheck(inputKeyType == InputKeyType.Left);
                    break;
                case "BATTLE_WAIT":
                    ConfigUtility.ChangeBattleWait(inputKeyType == InputKeyType.Left);
                    break;
                case "BATTLE_ANIMATION":
                    ConfigUtility.ChangeBattleAnimation(inputKeyType == InputKeyType.Right);
                    break;
                case "INPUT_TYPE":
                    var inputTypeIndex = (int)GameSystem.TempData.TempInputType;
                    if (inputKeyType == InputKeyType.Right)
                    {
                        inputTypeIndex++;
                        if (inputTypeIndex > 2)
                        {
                            inputTypeIndex = 0;
                        }
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        inputTypeIndex--;
                        if (inputTypeIndex <= -1)
                        {
                            inputTypeIndex = 2;
                        }
                    }
                    _model.ChangeTempInputType((InputType)inputTypeIndex);
                    break;
                case "BATTLE_AUTO":
                    ConfigUtility.ChangeBattleAuto(inputKeyType == InputKeyType.Right);
                    break;
                case "BATTLE_SPEED":
                    if (inputKeyType == InputKeyType.Right)
                    {                    
                        ConfigUtility.ChangeBattleSpeed(1);
                    }
                    if (inputKeyType == InputKeyType.Left)
                    {
                        ConfigUtility.ChangeBattleSpeed(-1);
                    }
                    break;
                case "TUTORIAL_CHECK":
                    ConfigUtility.ChangeTutorialCheck(inputKeyType == InputKeyType.Left);
                    break;
            }
            CommandRefresh();
        }

        private void CommandVolumeSlider(float volume)
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                if (data.OptionCommand.Key == "BGM_VOLUME")
                {
                    ConfigUtility.ChangeBGMValue(volume);
                } else
                if (data.OptionCommand.Key == "SE_VOLUME")
                {
                    ConfigUtility.ChangeSEValue(volume);
                }
                CommandRefresh();
            }
        }

        private void CommandVolumeMute(bool isMute)
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                if (data.OptionCommand.Key == "BGM_VOLUME")
                {
                    ConfigUtility.ChangeBGMMute(isMute);
                } else
                if (data.OptionCommand.Key == "SE_VOLUME")
                {
                    ConfigUtility.ChangeSEMute(isMute);
                }
                CommandRefresh();
            }
        }

        private void CommandChangeToggle(int toggleIndex)
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                switch (data.OptionCommand.Key)
                {
                    case "GRAPHIC_QUALITY":
                        if (toggleIndex == 1)
                        {
                            ConfigUtility.ChangeGraphicIndex(1);
                        }
                        if (toggleIndex == 0)
                        {
                            ConfigUtility.ChangeGraphicIndex(2);
                        };
                        break;
                    case "EVENT_SKIP":
                        ConfigUtility.ChangeEventSkipIndex(toggleIndex == 1);
                        break;
                    case "COMMAND_END_CHECK":
                        ConfigUtility.ChangeCommandEndCheck(toggleIndex == 0);
                        break;
                    case "BATTLE_WAIT":
                        ConfigUtility.ChangeBattleWait(toggleIndex == 0);
                        break;
                    case "BATTLE_ANIMATION":
                        ConfigUtility.ChangeBattleAnimation(toggleIndex == 1);
                        break;
                    case "INPUT_TYPE":
                        _model.ChangeTempInputType((InputType)toggleIndex);
                        break;
                    case "BATTLE_AUTO":
                        ConfigUtility.ChangeBattleAuto(toggleIndex == 1);
                        break;
                    case "BATTLE_SPEED":
                        ConfigUtility.SetBattleSpeed(ConfigUtility.SpeedList[toggleIndex+1]);
                        break;
                    case "TUTORIAL_CHECK":
                        ConfigUtility.ChangeTutorialCheck(toggleIndex == 0);
                        break;
                }
                CommandRefresh();
            }
        }

        private void CommandPlusMinus(int plusValue)
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                switch (data.OptionCommand.Key)
                {
                    case "SCREEN_SIZE":
                        if (plusValue > 0)
                        {
                            ConfigUtility.ChangeScreenSize(true);
                        } else
                        {
                            ConfigUtility.ChangeScreenSize(false);
                        }
                        break;
                }
                CommandRefresh();
            }
        }

        private void CommandRefresh()
        {
            _view.CommandRefresh();
        }

        private void CommandSelectCategory()
        {
            var categoryIndex = _view.OptionCategoryIndex + 1;
            if (categoryIndex >= 1 && categoryIndex < 3)
            {
                _view.SetOptionList(MakeListData(_model.OptionCommandData(
                    categoryIndex,
                    (a) => CommandVolumeSlider(a),
                    (a) => CommandVolumeMute(a),
                    (a) => CommandChangeToggle(a),
                    (a) => CommandPlusMinus(a)
                )));
                _view.CommandRefresh();
            }
        }

        private void CommandSelectOptionList()
        {
            var data = _view.OptionCommandInfo;
            if (data != null)
            {
                if (data.OptionCommand.ButtonType == OptionButtonType.Button)
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                }
            }
        }

        private void CommandDecideCategory()
        {
            _view.DecideCategory();
            CommandRefresh();
        }

        private void CommandCancelOptionList()
        {
            _view.CancelOptionList();
            CommandRefresh();
        }
    }
}