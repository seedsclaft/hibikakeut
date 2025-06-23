using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Ryneus.Status;

namespace Ryneus
{
    public class StatusPresenter : BasePresenter
    {
        private StatusModel _model = null;
        private StatusView _view = null;
        private CommandType _popupCommandType = CommandType.None;
        private bool _busy = false;
        public StatusPresenter(StatusView view)
        {
            _view = view;
            SetView(_view);
            _model = new StatusModel();
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetHelpWindow(_model.HelpText());
            _view.SetEvent((type) => UpdateCommand(type));

            _view.SetActiveArrows(_model.ActorInfos.Count > 1);
            _view.SetActiveLvUpInfo(_model.SceneParam.DisplayLvUpInfo.Value);
            _view.SetActiveDecide(_model.SceneParam.DisplayDecideButton.Value);
            _view.SetActiveCharacterList(_model.SceneParam.DisplayCharacterList.Value);
            _view.ChangeBackCommandActive(_model.SceneParam.DisplayBackButton.Value);
            CommandRefresh();
            ResetSelectSkill();
            _view.OpenAnimation(() =>
            {
                CheckTutorialState();
            });
        }

        private void UpdateCommand(ViewEvent viewEvent)
        {
            if (_busy /*|| _view.AnimationBusy*/)
            {
                return;
            }
            if (viewEvent.ViewCommandType.ViewCommandSceneType != ViewCommandSceneType.Status)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.ViewCommandType.CommandType);
            switch (viewEvent.ViewCommandType.CommandType)
            {
                case CommandType.LeftActor:
                    CommandLeftActor();
                    return;
                case CommandType.RightActor:
                    CommandRightActor();
                    return;
                case CommandType.Back:
                    CommandBack();
                    return;
                case CommandType.SelectActor:
                    CommandSelectActor((ActorInfo)viewEvent.Template);
                    return;
                case CommandType.DecideActor:
                    CommandDecideActor();
                    return;
                case CommandType.CancelActor:
                    return;
                case CommandType.SelectEquipSkill:
                    CommandSelectEquipSkill((SkillInfo)viewEvent.Template);
                    return;
                case CommandType.CancelEquipSkill:
                    CommandCancelSkill();
                    return;
                case CommandType.SelectChangeSkill:
                    CommandSelectChangeSkill((SkillInfo)viewEvent.Template);
                    return;
                case CommandType.CharacterList:
                    CommandCharacterList();
                    return;
                case CommandType.SelectCharacter:
                    CommandSelectCharacter((int)viewEvent.Template);
                    return;
                case CommandType.LvReset:
                    return;
                case CommandType.LevelUp:
                    CommandLevelUp();
                    return;
                case CommandType.ShowLearnMagic:
                    CommandShowLearnMagic();
                    return;
                case CommandType.LearnMagic:
                    CommandLearnMagic((SkillInfo)viewEvent.Template);
                    return;
                case CommandType.HideLearnMagic:
                    CommandHideLearnMagic();
                    return;
                case CommandType.SelectCommandList:
                    return;
                case CommandType.CallHelp:
                    CommandCallHelp();
                    return;
            }
            //CheckTutorialState(viewEvent.commandType);
        }

        private void CheckTutorialState(CommandType commandType = CommandType.None)
        {
            Func<TutorialData,bool> enable = (tutorialData) => 
            {
                var checkFlag = true;
                if (tutorialData.Param1 == 1000)
                {
                    // 初めて仲間加入画面を開く
                    //checkFlag = _view.DisplayDecide;
                }
                if (tutorialData.Param1 == 1200)
                {
                    // Activeの魔法を初めて入手するかステージ3の最初
                    checkFlag = _model.StageMembers().Find(a => a.LearnSkillIds().FindAll(b => DataSystem.FindSkill(b).SkillType == SkillType.Active).Count > 0) != null || _model.CurrentStage.StageId.Value == 3;
                }
                return checkFlag;
            };
            Func<TutorialData,bool> checkEnd = (tutorialData) => 
            {
                return true;
            };
            var tutorialViewInfo = new TutorialViewInfo
            {
                SceneType = (int)StatusType.Status + 200,
                CheckEndMethod = checkEnd,
                CheckMethod = enable,
                EndEvent = () => 
                {
                    _busy = false;
                    CheckTutorialState(commandType);
                }
            };
            _view.CommandCheckTutorialState(tutorialViewInfo);
        }

        private void CommandBack()
        {
            _view.CommandBack();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandSelectEquipSkill(SkillInfo skillInfo)
        {
            if (skillInfo.IsBattleSpecialSkill())
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(14010));
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SetSelectSkillInfo(skillInfo);
            // 選択する
            _view.CallChangeSkillList();
            _view.SetChangeSkillList(MakeListData(_model.ChangeAbleSkills(),0));
        }

        private void CommandCancelSkill()
        {
            if (_model.SceneParam.DisplayDecideButton.Value)
            {
                return;
            }
            if (_model.SelectSkillInfo != null)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                // 選択魔法のキャンセル
                ResetSelectSkill();
                return;
            }
        }

        private void CommandSelectChangeSkill(SkillInfo skillInfo)
        {
            if (_model.SceneParam.DisplayDecideButton.Value)
            {
                return;
            }
            if (!skillInfo.Enable)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                return;
            }

            // 変更する
            SoundManager.Instance.PlayStaticSe(SEType.Decide);

            // だれかが装備している
            var equipmentActor = _model.EquipmentSkill(skillInfo);
            if (equipmentActor != null && equipmentActor.ActorId.Value != _model.CurrentActor.ActorId.Value)
            {
                var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(14020, skillInfo.Master.Name) + DataSystem.GetReplaceText(14021, equipmentActor.Master.Name),(a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        _model.RemoveEquipSkill(equipmentActor,skillInfo.Id.Value);
                        _model.ChangeEquipSkill(skillInfo.Id.Value);
                        ResetSelectSkill();
                        _model.PartyInfo.StatusSkillChangeCount.GainValue(1);
                        CheckAchievements();
                    } else
                    {
                    }
                    _busy = false;
                });
                _view.CommandCallConfirm(confirmInfo);
                return;
            }

            _model.ChangeEquipSkill(skillInfo.Id.Value);
            ResetSelectSkill();
            _model.PartyInfo.StatusSkillChangeCount.GainValue(1);
            CheckAchievements();
        }

        private void ResetSelectSkill()
        {
            _model.SetSelectSkillInfo(null);
            _view.CallEquipSkillList();
            CommandRefreshMagicList();
        }

        private void CommandCharacterList()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var characterListInfo = new CharacterListInfo((a) => 
            {
                _view.CallSystemCommand(Base.CommandType.ClosePopup);
                _model.SelectActor(a);
                CommandRefresh();
                _busy = false;
            },
            () => 
            {
                CommandRefresh();
                _busy = false;
            });
            characterListInfo.SetActorInfos(_model.ActorInfos);

            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.CharacterList,
                template = characterListInfo,
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CallSystemCommand(Base.CommandType.CallPopupView,popupInfo);
            CheckTutorialState();
        }

        public void CommandSelectCharacter(int actorId)
        {
            _model.SelectActor(actorId);
            CommandRefresh();
        }

        private void CommandLevelUp()
        {
            if (_model.SceneParam.DisplayDecideButton.Value)
            {
                return;
            }
            _busy = true;
            _view.SetBusy(true);
            _model.PartyInfo.TacticsLvupCount.GainValue(1);
            CommandLevelUp(_model.CurrentActor,() =>
            {
                CheckAchievements();
                _busy = false;
                _view.SetBusy(false);
                CommandRefresh();
            });
        }

        private void CommandShowLearnMagic()
        {
            CommandRefresh();
        }

        private void CommandLearnMagic(SkillInfo skillInfo)
        {
            CommandLearnMagic(_model.CurrentActor,skillInfo,() =>
            {
                _view.CommandRefresh();
                CommandShowLearnMagic();
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            });
        }

        private void CommandHideLearnMagic()
        {
            CommandRefresh();
        }

        private void CommandSelectSkillTrigger(int actorId)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            var skillTriggerViewInfo = new SkillTriggerViewInfo(actorId,() => 
            {
                _busy = false;
                CommandRefresh();
            });
            _view.CommandCallSkillTrigger(skillTriggerViewInfo);
        }

        private void CommandSelectActor(ActorInfo actorInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SelectActor(actorInfo.ActorId.Value);
        }

        private void CommandDecideActor()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = true;
            // 確認後結果表示
            var confirmInfo = new ConfirmInfo(DataSystem.GetReplaceText(14030, _model.CurrentActor.Master.Name),(a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.CloseStatus);
                    var strategySceneInfo = _model.DecideActor();
                    strategySceneInfo.ReturnScene = GameSystem.SceneStackManager.Current;
                    _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
                } else
                {
                    _busy = false;
                }
            });
            _view.CommandCallConfirm(confirmInfo);
        }

        private void UpdatePopup(ConfirmCommandType confirmCommandType)
        {
            if (_popupCommandType == CommandType.SelectEquipSkill)
            {
                if (confirmCommandType == ConfirmCommandType.Yes)
                {
                    CommandRefresh();
                }
            }


            if (_popupCommandType == CommandType.DecideStage)
            {
                if (confirmCommandType == ConfirmCommandType.Yes)
                {
                    _view.CallSystemCommand(Base.CommandType.CloseStatus);

                    var makeSelectActorInfos = _model.MakeSelectActorInfos();
                    var makeSelectGetItemInfos = _model.MakeSelectGetItemInfos();
                    var strategySceneInfo = new StrategySceneInfo
                    {
                        GetItemInfos = makeSelectGetItemInfos,
                        ActorInfos = makeSelectActorInfos,
                        InBattle = false
                    };
                    _view.CommandGotoSceneChange(Scene.Strategy,strategySceneInfo);
                } else
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                    SetBusy(false);
                }
            }
        }

        private async Task CommandLeftActor()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SaveSelectedSkillId();
            _model.ChangeActorIndex(-1);
            CommandRefreshMagicList();
            CommandRefresh();
            await UniTask.DelayFrame(30);
            _busy = false;
        }

        private async Task CommandRightActor()
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SaveSelectedSkillId();
            _model.ChangeActorIndex(1);
            CommandRefreshMagicList();
            CommandRefresh();
            await UniTask.DelayFrame(30);
            _busy = false;
        }

        private void CommandRefresh()
        {
            _model.UpdateActorRemainCost();
            _view.SetActorInfo(_model.CurrentActor,_model.ActorInfos);
            _view.SetLvUpInfo(_model.LevelUpCost(),_model.Currency);
            _view.CommandRefresh();
        }

        private void CommandRefreshMagicList()
        {
            CommandRefresh();
            _view.SetEquipSkillList(MakeListData(_model.EquipSkills(),0));
        }

        private void SaveSelectedSkillId()
        {
            var selectedSkillId = _view.SelectedSkillId();
            if (selectedSkillId > -1)
            {
                _model.SetActorLastSkillId(selectedSkillId);
            }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            _view.SetBusy(busy);
        }

        private void CommandCallHelp()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Guide,
                template = "Status",
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }
    }
}