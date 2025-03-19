using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    using Tactics;
    public class TacticsView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BaseList tacticsCommandList = null;
        public SystemData.CommandData TacticsCommandData => tacticsCommandList.ListItemData<SystemData.CommandData>();
        [SerializeField] private BaseList battleMemberList = null;
        public ActorInfo SelectBattleMember => battleMemberList.ListItemData<ActorInfo>();
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;
        [SerializeField] private SymbolList symbolInfoList = null;
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
            InitializeBattleMemberList();
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
            InitializeSymbolInfoList();

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
            tacticsCommandList.SetInputHandler(InputKeyType.Option1,() => CallStatus());
            tacticsCommandList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(tacticsCommandList.gameObject);
            AddViewActives(tacticsCommandList);
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

        private void InitializeSymbolInfoList()
        {
            symbolInfoList.Initialize();
            SetInputHandler(symbolInfoList.gameObject);
            symbolInfoList.SetInputHandler(InputKeyType.Decide,OnClickSymbol);
            symbolInfoList.SetInputHandler(InputKeyType.Cancel,OnCancelSymbol);
            symbolInfoList.SetSymbolDetailInfoEvent(SymbolDetailInfo);
            symbolInfoList.SetInputHandler(InputKeyType.Option1,SymbolDetailInfo);
            AddViewActives(symbolInfoList);
            symbolInfoList.gameObject.SetActive(false);
        }

        public void SetSymbolList(List<ListData> symbolList,int seekIndex,int seek)
        {
            symbolInfoList.SetSeekIndex(seekIndex);
            symbolInfoList.SetData(symbolList,true);
            SetActivate(symbolInfoList);
        }

        private void OnClickSymbol()
        {
            if (symbolInfoList.ScrollRect.enabled == false) return;
            var data = symbolInfoList.SelectSymbolInfo();
            if (data != null)
            {
                CallViewEvent(CommandType.OnClickSymbol,data);
            }
        }

        private void OnCancelSymbol()
        {
            symbolInfoList.gameObject.SetActive(false);
            CallViewEvent(CommandType.OnCancelSymbol);
        }

        private void SymbolDetailInfo()
        {
            CallViewEvent(CommandType.SymbolDetailInfo,symbolInfoList.SelectSymbolInfo());
        }

        public void UpdatePartyInfo(PartyInfo partyInfo)
        {
            symbolInfoList.UpdatePartyInfo(partyInfo);
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

        public void SetTacticsCommand(List<ListData> menuCommands)
        {
            tacticsCommandList.SetData(menuCommands);
            UpdateHelpWindow();
            SetActivate(tacticsCommandList);
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
            symbolInfoList.gameObject.SetActive(true);
        }

        public void HideSymbolRecord()
        {
            symbolInfoList.gameObject.SetActive(false);
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
            if (symbolInfoList.gameObject.activeSelf && symbolInfoList.Active)
            {
                SetHelpInputInfo("RECORD_LIST");
            } else
            {
                SetHelpInputInfo("TACTICS");
            }
        }
    }

}