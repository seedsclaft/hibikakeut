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
            _view.SetActiveDecide(_model.SceneParam.AddActor.Value);
            _view.SetActiveCharacterList(_model.SceneParam.DisplayCharacterList.Value);
            _view.ChangeBackCommandActive(_model.SceneParam.DisplayBackButton.Value);
            CommandRefresh();
            ResetSelectSkill();
            _view.OpenAnimation(() =>
            {
                //CheckTutorialState();
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
                case CommandType.ShowUseItem:
                    CommandShowUseItem();
                    return;
                case CommandType.UseItem:
                    CommandUseItem((ItemInfo)viewEvent.Template);
                    return;
                case CommandType.CancelUseItem:
                    CommandCancelUseItem();
                    return;
                case CommandType.ChangeEquipment:
                    CommandChangeEquipment();
                    return;
                case CommandType.SelectEquipment:
                    CommandSelectEquipment((int)viewEvent.Template);
                    return;
                case CommandType.CancelEquipment:
                    CommandCancelEquipment();
                    return;
                case CommandType.SelectChangeEquipment:
                    CommandSelectChangeEquipment((EquipmentInfo)viewEvent.Template);
                    return;
                case CommandType.CancelChangeEquipment:
                    CommandCancelChangeEquipment();
                    return;
                case CommandType.DetailEquipment:
                    CommandDetailEquipment((EquipmentInfo)viewEvent.Template);
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
                case CommandType.FilterPlus:
                    CommandFilterPlus();
                    return;
                case CommandType.FilterMinus:
                    CommandFilterMinus();
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
                    checkFlag = false;//_model.StageMembers().Find(a => a.LearnSkillIds().FindAll(b => DataSystem.FindSkill(b).SkillType == SkillType.Active).Count > 0) != null || _model.CurrentStage.StageId.Value == 3;
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
            CommandCancelSkill();
        }

        private void CommandSelectEquipSkill(SkillInfo skillInfo)
        {
            if (skillInfo.IsBattleSpecialSkill() || skillInfo.Master.SkillType == SkillType.Kind)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(14010));
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SetSelectSkillInfo(skillInfo);
            // 選択する
            _view.CallChangeSkillList();
            _view.UpdateUseItemBatch(false);
            _view.SetChangeSkillList(MakeListData(_model.ChangeAbleSkills(), 0), _model.FilterText());
        }

        private void CommandCancelSkill()
        {
            if (_model.SceneParam.AddActor.Value)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            if (_model.SelectSkillInfo != null)
            {
                // 選択魔法のキャンセル
                ResetSelectSkill();
                return;
            }
            _view.CommandBack(_model.SceneParam.BackEvent);
        }

        private void CommandSelectChangeSkill(SkillInfo skillInfo)
        {
            if (_model.SceneParam.AddActor.Value)
            {
                return;
            }
            if (!skillInfo.Enable)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                return;
            }

            // 変更する

            // だれかが装備している
            var equipmentActor = _model.EquipmentSkill(skillInfo);
            if (equipmentActor != null && equipmentActor.ActorId.Value != _model.CurrentActor.ActorId.Value)
            {
                CallConfirmView(DataSystem.GetReplaceText(14020, skillInfo.Master.Name) + DataSystem.GetReplaceText(14021, equipmentActor.Master.Name),(a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        _model.RemoveEquipSkill(equipmentActor,skillInfo.Id.Value);
                        _model.ChangeEquipSkill(skillInfo.Id.Value);
                        ResetSelectSkill();
                        _model.PartyInfo.PartyStatInfo.StatusSkillChangeCount.GainValue(1);
                        CheckAchievements();
                    } else
                    {
                    }
                    _busy = false;
                });
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.ChangeEquipSkill(skillInfo.Id.Value);
            ResetSelectSkill();
            _model.PartyInfo.PartyStatInfo.StatusSkillChangeCount.GainValue(1);
            CheckAchievements();
            CommandRefresh();
        }

        private void ResetSelectSkill()
        {
            _model.SetSelectSkillInfo(null);
            _view.CallEquipSkillList(_model.SceneParam.AddActor.Value);
            CommandRefreshMagicList(false);
        }

        private void CommandShowUseItem()
        {
            _busy = true;
            _view.CallUseItemList();
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            var useItemSceneInfo = new UseItemSceneInfo();
            useItemSceneInfo.UsableItemTypes = new()
            {
                UseItemType.Exp,
                UseItemType.AttributeUp,
                UseItemType.ClassChange,
                UseItemType.StatusUp
            };
            useItemSceneInfo.CurrentActor = _model.CurrentActor;
            CallPopupView(PopupType.UseItem, () =>
            {
                _busy = false;
                _view.CallEquipSkillList(_model.SceneParam.AddActor.Value);
                CommandRefresh();
                CommandRefreshMagicList(false);
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }, useItemSceneInfo);
        }

        private void CommandUseItem(ItemInfo itemInfo)
        {
            if (itemInfo == null)
            {
                return;
            }
            if (!_model.CanUseItem(itemInfo))
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(14160));
                return;
            }
            _model.PartyInfo.ConsuneItemNum(itemInfo.Id.Value, 1);
            // 経験値付与
            switch (itemInfo.Master.Param1)
            {
                case (int)UseItemType.Exp:
                    UseItemExp(itemInfo);
                    break;
                case (int)UseItemType.AttributeUp:
                    UseItemAttributeUp(itemInfo);
                    break;
                case (int)UseItemType.StatusUp:
                    UseItemStatusUp(itemInfo);
                    break;
                case (int)UseItemType.ClassChange:
                    UseItemClassChange(itemInfo);
                    break;
            }
        }

        private void UseItemExp(ItemInfo itemInfo)
        {
            var getExp = itemInfo.Master.Param2;
            if (_model.CurrentActor.Level <= itemInfo.Master.Param3)
            {
                getExp *= 2;
            }
            _busy = true;
            _model.PartyInfo.PartyStatInfo.TacticsLvupCount.GainValue(1);
            CommandExpUp(_model.CurrentActor, getExp, () =>
            {
                CheckAchievements();
                _busy = false;
                CommandRefreshMagicList(false);
            });
            CommandRefreshuseItemList();
        }

        private void UseItemAttributeUp(ItemInfo itemInfo)
        {
            var getAttibute = (AttributeType)itemInfo.Master.Param2;
            _busy = true;
            CommandAttributeUp(_model.CurrentActor, getAttibute, () =>
            {
                CheckAchievements();
                _busy = false;
                CommandRefreshMagicList(false);
            });
            CommandRefreshuseItemList();
        }

        private void UseItemStatusUp(ItemInfo itemInfo)
        {
            var statusType = (StatusParamType)itemInfo.Master.Param2;
            _busy = true;
            CommandStatusUp(_model.CurrentActor, statusType, itemInfo.Master.Param2, () =>
            {
                CheckAchievements();
                _busy = false;
                CommandRefresh();
            });
            CommandRefreshuseItemList();
        }

        private void UseItemClassChange(ItemInfo itemInfo)
        {
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.LevelUp);
            var beforeStatus = new StatusInfo();
            beforeStatus.SetParameter(_model.CurrentActor.CurrentStatus);
            var ClassChangeInfo = new ClassChangeInfo(_model.CurrentActor, beforeStatus);
            _model.CurrentActor.IsClassChenged.SetValue(true);
            CallPopupView(PopupType.ClassChange, () =>
            {
                CheckAchievements();
                _busy = false;
                CommandRefreshMagicList(false);
            }, ClassChangeInfo);
            CommandRefreshuseItemList();
        }

        private void CommandCancelUseItem()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.CallEquipSkillList(_model.SceneParam.AddActor.Value);
        }

        private void CommandChangeEquipment()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _view.CallEquipment();
            _view.SetEquipmentInfo(MakeListData(_model.ActorEquipmentInfos(), 0));
        }

        private void CommandSelectEquipment(int selectIndex)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.SelectEquipmentIndex.SetValue(selectIndex);
            _view.CallChangeEquipment();
            _view.SetChangeEquipmentInfo(MakeListData(_model.EquipmentInfos(), 0));
        }

        private void CommandCancelEquipment()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.CallEquipSkillList(_model.SceneParam.AddActor.Value);
            CommandRefreshMagicList(false);
        }

        private void CommandSelectChangeEquipment(EquipmentInfo equipmentInfo)
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            // だれかが装備している
            var equipmentActor = _model.EquipmentSkill(equipmentInfo);
            if (equipmentActor != null && equipmentInfo.EquipmentId.Value != 10 && equipmentActor.ActorId.Value != _model.CurrentActor.ActorId.Value)
            {
                CallConfirmView(DataSystem.GetReplaceText(14020, equipmentInfo.Master.Name) + DataSystem.GetReplaceText(14021, equipmentActor.Master.Name),(a) =>
                {
                    if (a == ConfirmCommandType.Yes)
                    {
                        _model.RemoveEquipment(equipmentActor, equipmentInfo);
                        _model.ChangeEquipment(equipmentInfo);
                        _view.CallEquipment();
                        _view.SetEquipmentInfo(MakeListData(_model.ActorEquipmentInfos(), 0));
                        _model.PartyInfo.PartyStatInfo.StatusSkillChangeCount.GainValue(1);
                        CheckAchievements();
                    }
                    _busy = false;
                });
                return;
            }
            if (_model.PartyInfo.EquipmentIds.Count == 0)
            {
                return;
            }
            _model.ChangeEquipment(equipmentInfo);
            _view.CallEquipment();
            _view.SetEquipmentInfo(MakeListData(_model.ActorEquipmentInfos(), 0));
            if (equipmentInfo.Master.Id != DataSystem.System.InitEquipmentId)
            {
                _model.PartyInfo.PartyStatInfo.StatusSkillChangeCount.GainValue(1);
                CheckAchievements();
            }
        }

        private void CommandCancelChangeEquipment()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            _view.CallEquipment();
            _view.SetEquipmentInfo(MakeListData(_model.ActorEquipmentInfos(), 0));
        }

        private void CommandDetailEquipment(EquipmentInfo equipmentInfo)
        {
            if (equipmentInfo == null)
            {
                return;
            }
            if (equipmentInfo.LearningInfos.Count == 0)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _busy = true;
            CallConfirmSkillDetailView("", equipmentInfo.SkillInfos(), (a) =>
            {
                _busy = false;
            });
        }

        private void CommandAutoSetSkill()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _model.AutoSetSkill();
            ResetSelectSkill();
            _model.PartyInfo.PartyStatInfo.StatusSkillChangeCount.GainValue(1);
            CheckAchievements();
            CommandRefresh();
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

            CallPopupView(PopupType.CharacterList, () =>
            {
                _busy = false;
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }, characterListInfo);
            CheckTutorialState();
        }

        public void CommandSelectCharacter(int actorId)
        {
            _model.SelectActor(actorId);
            CommandRefresh();
        }

        private void CommandLevelUp()
        {
            if (_model.SceneParam.AddActor.Value)
            {
                return;
            }
            // Nu不足
            if (_model.PartyInfo.Currency.Value <= 0)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Deny);
                CommandCautionInfo(DataSystem.GetText(38020));
                return;
            }
            _busy = true;
            _model.PartyInfo.PartyStatInfo.TacticsLvupCount.GainValue(1);
            CommandExpUp(_model.CurrentActor, 20, () =>
            {
                CheckAchievements();
                _busy = false;
                CommandRefresh();
            });
        }

        private void CommandShowLearnMagic()
        {
            CommandRefresh();
        }

        private void CommandLearnMagic(SkillInfo skillInfo)
        {
            /*
            CommandLearnMagic(_model.CurrentActor, skillInfo, () =>
            {
                _view.CommandRefresh();
                CommandShowLearnMagic();
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            });
            */
        }

        private void CommandHideLearnMagic()
        {
            CommandRefresh();
        }

        private void CommandFilterPlus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeFilterAttribute(true);
            _view.SetChangeSkillList(MakeListData(_model.ChangeAbleSkills(), 0), _model.FilterText());
        }

        public void CommandFilterMinus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.ChangeFilterAttribute(false);
            _view.SetChangeSkillList(MakeListData(_model.ChangeAbleSkills(), 0), _model.FilterText());
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
            if (_model.SelectSkillInfo != null)
            {
                // 選択魔法のキャンセル
                ResetSelectSkill();
            }
            CommandRefreshMagicList(true);
            CommandRefresh();
        }

        private void CommandDecideActor()
        {
            _busy = true;
            // 確認後結果表示
            CallConfirmView(DataSystem.GetReplaceText(14030, _model.CurrentActor.Master.Name), (a) =>
            {
                if (a == ConfirmCommandType.Yes)
                {
                    _model.DecideActor();
                    // Mainmenuに遷移
                    _view.CallSystemCommand(Base.CommandType.CloseStatus);
                    var sceneParam = new MainMenuSceneInfo
                    {
                        CommandIndex = 3
                    };
                    _view.CommandGotoSceneChange(Scene.MainMenu, sceneParam);
                    /*
                    _view.CallSystemCommand(Base.CommandType.CloseStatus);
                    var strategySceneInfo = _model.DecideActor();
                    strategySceneInfo.ReturnScene = GameSystem.SceneStackManager.Current;
                    var sceneParam = new MainMenuSceneInfo
                    {
                        CommandIndex = 3
                    };
                    strategySceneInfo.ReturnMainMenuSceneParam = sceneParam;
                    _view.CommandGotoSceneChange(Scene.Strategy, strategySceneInfo);
                    */
                }
                else
                {
                    _busy = false;
                }
            });
        }

        private void CommandLeftActor()
        {
            if (_model.ActorInfos.Count == 1)
            {
                return;
            }
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SaveSelectedSkillId();
            _model.ChangeActorIndex(-1);
            CommandRefreshMagicList(true);
            _view.SetEquipmentInfo(MakeListData(_model.ActorEquipmentInfos(), 0));
            CommandRefresh();
            //await UniTask.DelayFrame(16);
            _busy = false;
        }

        private void CommandRightActor()
        {
            if (_model.ActorInfos.Count == 1)
            {
                return;
            }
            _busy = true;
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SaveSelectedSkillId();
            _model.ChangeActorIndex(1);
            CommandRefreshMagicList(true);
            _view.SetEquipmentInfo(MakeListData(_model.ActorEquipmentInfos(), 0));
            CommandRefresh();
            //await UniTask.DelayFrame(16);
            _busy = false;
        }

        private void CommandRefresh()
        {
            _model.UpdateActorRemainCost();
            _view.SetActorInfo(_model.CurrentActor, _model.ActorInfos);
            _view.SetLvUpInfo(_model.LevelUpCost(), _model.Currency);
            _view.SetLvUpExpInfo(_model.LevelUpBeforeExp(), _model.LevelUpAfterExp());
            _view.UpdateUseItemBatch(_model.IsUseItemBatch());
            _view.UpdateChangeSkillBatch(_model.IsChangeSkillBatch());
            _view.SetActorTabList(MakeListData(_model.ActorInfos, _model.CurrentIndex.Value), _model.CurrentIndex.Value);
            _view.CommandRefresh();
        }

        private void CommandRefreshMagicList(bool resetListIndex)
        {
            CommandRefresh();
            _view.SetEquipSkillList(MakeListData(_model.Skills(), 0), resetListIndex);
        }

        private void SaveSelectedSkillId()
        {
            var selectedSkillId = _view.SelectedSkillId();
            if (selectedSkillId > -1)
            {
                _model.SetActorLastSkillId(selectedSkillId);
            }
        }

        private void CommandRefreshuseItemList()
        {
            CommandRefresh();
            _view.SetUseItemList(MakeListData(_model.UseItemInfos(), 0));
        }

        private void CommandCallHelp()
        {
            _busy = true;
            CallPopupView(PopupType.Guide, () =>
            {
                _busy = false;
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            }, "Status");
        }
    }
}