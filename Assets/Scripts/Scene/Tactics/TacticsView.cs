using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    using System;
    using Cysharp.Threading.Tasks;
    using Tactics;
    public class TacticsView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BaseList hexTiles = null;
        public HexField SelectHexField => hexTiles.ListItemData<HexField>();
        [SerializeField] private BaseList tacticsCommandList = null;
        public SystemData.CommandData TacticsCommandData => tacticsCommandList.ListItemData<SystemData.CommandData>();
        [SerializeField] private BaseList battleMemberList = null;
        public ActorInfo SelectBattleMember => battleMemberList.ListItemData<ActorInfo>();
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;
        [SerializeField] private MagicList alcanaSelectList = null;
        [SerializeField] private TextMeshProUGUI saveScoreText = null;
        [SerializeField] private TacticsAlcana tacticsAlcana = null;
        [SerializeField] private Button alcanaButton = null;
        [SerializeField] private Button stageHelpButton = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private _2dxFX_NoiseAnimated _2DxFX_NoiseAnimated = null;
        [SerializeField] private TextMeshProUGUI pastText = null;
        [SerializeField] private BattleStartAnim battleStartAnim = null;
        [SerializeField] private Effekseer.EffekseerEmitter effekseerEmitter = null;

        private bool _viewBusy = false;
        public void SetViewBusy(bool isBusy)
        {
            _viewBusy = isBusy;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Tactics);
            InitializeCommandList();
            //InitializeBattleMemberList();
            InitializeHexTileList();
            tacticsAlcana.gameObject.SetActive(false);
            alcanaButton.onClick.AddListener(() => CallAlcanaCheck());

            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            stageHelpButton.onClick.AddListener(() => 
            {
                CallViewEvent(CommandType.StageHelp);
            });

            alcanaSelectList.Initialize();
            HideSymbolRecord();
            alcanaSelectList.Hide();
            battleStartAnim.Reset();
            var presenter = new TacticsPresenter(this);
            presenter.CommandReturnStrategy();
        }

        private void InitializeCommandList()
        {
            tacticsCommandList.Initialize();
            tacticsCommandList.SetInputHandler(InputKeyType.Decide,() => CallTacticsCommand());
            tacticsCommandList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.CancellTacticsCommand));
            //tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CallStatus());
            tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(tacticsCommandList.gameObject);
            AddViewActives(tacticsCommandList);
        }

        public void SetTacticsCommand(List<ListData> menuCommands)
        {
            tacticsCommandList.gameObject.SetActive(true);
            SetActivate(tacticsCommandList);
            tacticsCommandList.SetData(menuCommands);
            UpdateHelpWindow();
        }

        public void EndTacticsCommand()
        {
            tacticsCommandList.gameObject.SetActive(false);
            SetActivate(hexTiles);
        }

        private void CallTacticsCommand()
        {
            var listData = tacticsCommandList.ListData;
            if (listData != null && listData.Enable)
            {
                SoundManager.Instance.PlayStaticSe(SEType.Decide);
                CallViewEvent(CommandType.CallTacticsCommand);
            }
        }

        private void CallStatus()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            CallViewEvent(CommandType.CallStatus);
        }

        private void InitializeBattleMemberList()
        {
            battleMemberList.Initialize();
            battleMemberList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.CallBattleMemberSelect));
            battleMemberList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.CallBattleMemberSelectEnd));
            battleMemberList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(battleMemberList.gameObject);
            AddViewActives(battleMemberList);
        }

        public void ActivateBattleMemberList()
        {
            SetActivate(battleMemberList);
            battleMemberList.UpdateSelectIndex(0);
            battleMemberList.Refresh();
        }

        public void DeactivateBattleMemberList()
        {
            battleMemberList.UpdateSelectIndex(-1);
            SetActivate(tacticsCommandList);
        }

        public void SetBattleMemberList(List<ListData> memberList)
        {
            battleMemberList.SetData(memberList);
        }

        public void RefreshBattleMemberList(List<ListData> memberList)
        {
            battleMemberList.RefreshListData(memberList);
        }

        private void InitializeHexTileList()
        {
            hexTiles.Initialize();
            hexTiles.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.SelectHexUnit));
            hexTiles.SetInputHandler(InputKeyType.Up,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Up));
            hexTiles.SetInputHandler(InputKeyType.Down,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Down));
            hexTiles.SetInputHandler(InputKeyType.Right,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Right));
            hexTiles.SetInputHandler(InputKeyType.Left,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Left));
            SetInputHandler(hexTiles.gameObject);
            AddViewActives(hexTiles);
        }

        public void SetHexTileList(List<ListData> hexInfos)
        {
            hexTiles.SetData(hexInfos,true,() => 
            {
                UpdateHexIndex(0,0);
            });
            SetActivate(hexTiles);
        }

        public void UpdateHexIndex(int x,int y)
        {
            hexTiles.UpdateSelectIndex(x + y * 8);
        }

        public void RefreshTiles()
        {
            hexTiles.Refresh();
        }

        public void SelectMoveBattler(List<Action> actions,HexUnitInfo hexUnitInfo)
        {
            MoveAction(actions,hexUnitInfo);
        }

        private async void MoveAction(List<Action> actions,HexUnitInfo hexUnitInfo)
        {
            if (actions.Count == 0)
            {
                RefreshTiles();
                UpdateHexIndex(hexUnitInfo.HexField.X,hexUnitInfo.HexField.Y);
                return;
            }
            actions[0]();
            RefreshTiles();
            UpdateHexIndex(hexUnitInfo.HexField.X,hexUnitInfo.HexField.Y);
            await UniTask.DelayFrame(10);
            actions.RemoveAt(0);
            if (actions.Count > 0)
            {
                MoveAction(actions,hexUnitInfo);
            } else
            {
                CallViewEvent(CommandType.EndMoveBattler);
            }
        }

        public void StartStageAnimation(Effekseer.EffekseerEffectAsset effekseerEffect)
        {
            if (effekseerEmitter == null)
            {
                return;
            }
            effekseerEmitter.Play(effekseerEffect);
            StartAnimation();
        }

        public void StartAnimation()
        {
            battleStartAnim.SetText("Ready!");
            battleStartAnim.StartAnim(false,0.5f);
        }

        private void CallSideMenu()
        {
            CallViewEvent(CommandType.SelectSideMenu);
        }

        

        private void UpdateHelpWindow()
        {
            var listData = tacticsCommandList.ListData;
            if (listData != null)
            {
                var commandData = (SystemData.CommandData)listData.Data;
                SetHelpText(commandData.Help);
            }
        }

        public void SetUIButton()
        {
            SetBackCommand(() => OnClickBack());
        }

        private void OnClickBack()
        {
            CallViewEvent(CommandType.Back);
        }

        public void SetHelpWindow()
        {
        }

        public void SetStageInfo(StageInfo stageInfo)
        {
            stageInfoComponent.UpdateInfo(stageInfo);
        }

        public void SetAlcanaInfo(List<SkillInfo> skillInfos)
        {
            alcanaInfoComponent.UpdateInfo(skillInfos);
        }

        public void SetTacticsCharaLayer(List<ActorInfo> actorInfos)
        {
        }

        public void SetSymbols(List<ListData> symbolInfos)
        {
            HideRecordList();
        }

        public void ShowSymbolRecord()
        {
        }

        public void HideSymbolRecord()
        {
        }

        private void CallBattleEnemy()
        {
            /*
            if (tacticsSymbolList.IsSelectSymbol())
            {
                var data = tacticsSymbolList.ListItemData<SymbolResultInfo>();
                if (data != null)
                {
                    if (data != null && data.SymbolType != SymbolType.None)
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Decide);
                        CallEvent(CommandType.SelectSymbol)
                        {
                            template = data
                        };
                        _commandData(eventData);
                    }
                }
            } else
            {
                var getItemInfos = tacticsSymbolList.SelectRelicInfos();
                if (getItemInfos != null && getItemInfos.Count > 0)
                {
                    CallEvent(CommandType.PopupSkillInfo)
                    {
                        template = getItemInfos
                    };
                    _commandData(eventData);
                } else
                {
                    var getItemInfo = tacticsSymbolList.GetItemInfo();
                    if (getItemInfo != null && (getItemInfo.IsSkill() || getItemInfo.IsAttributeSkill()))
                    {
                        CallEvent(CommandType.PopupSkillInfo)
                        {
                            template = new List<GetItemInfo>(){getItemInfo}
                        };
                        _commandData(eventData);
                    }
                    if (getItemInfo != null && getItemInfo.IsAddActor())
                    {
                        var data = tacticsSymbolList.ListItemData<SymbolResultInfo>();
                        if (data != null)
                        {
                            CallEvent(CommandType.CallAddActorInfo)
                            {
                                template = data
                            };
                            _commandData(eventData);
                        }
                    }
                }
            }
            */
        }

        private void OnClickEnemyInfo()
        {
        }

        private void OnClickParallel()
        {
        }


        public void ShowRecordList()
        {
            //symbolInfoList.ScrollRect.enabled = false;
        }

        public void HideRecordList()
        {
            //symbolInfoList.ScrollRect.enabled = true;
        }

        public void SetSaveScore(float saveScore)
        {
            saveScoreText?.SetText("+" + saveScore.ToString("F2"));
        }
        
        public void StartAlcanaAnimation(System.Action endEvent)
        {
            tacticsAlcana.StartAlcanaAnimation(endEvent);
        }

        private void CallAlcanaCheck()
        {
            CallViewEvent(CommandType.AlcanaCheck);
        }

        public void HideAlcanaList()
        {
            alcanaSelectList.Hide();
        }

        public void SetAlcanaSelectInfos(List<ListData> skillInfos)
        {
            SetBackEvent(() => OnClickBack());
            alcanaSelectList.SetData(skillInfos);
            alcanaSelectList.SetInputHandler(InputKeyType.Decide,() => 
            {
                var skillInfo = AlcanaSelectSkillInfo();
                if (skillInfo != null && skillInfo.Enable)
                {
                    CallViewEvent(CommandType.SelectAlcanaList,skillInfo);
                }
            });
            alcanaSelectList.Show();
        }

        public SkillInfo AlcanaSelectSkillInfo() 
        {
            return alcanaSelectList.ListItemData<SkillInfo>();
        }

        public void SetNuminous(int numinous)
        {
            //numinousText?.SetText(numinous.ToString());
        }

        public void SetPastMode(bool pastMode)
        {
            pastText?.gameObject.SetActive(pastMode);
            _2DxFX_NoiseAnimated.enabled = pastMode;
        }

        public void CommandSelectCharaLayer(int actorId)
        {
        }

        public void ActivateCommandList()
        {
            SetActivate(tacticsCommandList);
        }

        public void EndStatusCursor()
        {
        }

        public void InputHandler(List<InputKeyType> keyTypes, bool pressed)
        {
        }

        public void CommandRefresh()
        {
        }
    }

}