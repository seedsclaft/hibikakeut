using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Ryneus
{
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Tactics;
    using Unity.VisualScripting;

    public class TacticsView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BaseList hexTiles = null;
        public HexField SelectHexField => hexTiles.ListItemData<HexField>();
        [SerializeField] private BaseList tacticsCommandList = null;
        public SystemData.CommandData TacticsCommandData => tacticsCommandList.ListItemData<SystemData.CommandData>();
        [SerializeField] private BaseList battleMemberSelectList = null;
        [SerializeField] private UnitInfoComponent actorUnitInfo = null;
        [SerializeField] private UnitInfoComponent enemyUnitInfo = null;
        [SerializeField] private HexUnitComponent fieldUnitInfo = null;
        [SerializeField] private StageInfoComponent stageInfoComponent = null;
        [SerializeField] private AlcanaInfoComponent alcanaInfoComponent = null;
        [SerializeField] private MagicList alcanaSelectList = null;
        [SerializeField] private TextMeshProUGUI saveScoreText = null;
        [SerializeField] private TacticsAlcana tacticsAlcana = null;
        [SerializeField] private Button alcanaButton = null;
        [SerializeField] private Button stageHelpButton = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private BattleStartAnim battleStartAnim = null;
        [SerializeField] private Effekseer.EffekseerEmitter effekseerEmitter = null;

        
        public override void Initialize()
        {
            base.Initialize();
            SetViewCommandSceneType(ViewCommandSceneType.Tactics);
            InitializeCommandList();
            InitializeHexTileList();
            InitializeBattleMemberSelect();
            actorUnitInfo.gameObject.SetActive(false);
            enemyUnitInfo.gameObject.SetActive(false);
            fieldUnitInfo.gameObject.SetActive(false);
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
            alcanaSelectList.Hide();
            battleStartAnim.Reset();
            var presenter = new TacticsPresenter(this);
            //presenter.CommandReturnStrategy();
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

        public void ShowUnitStatus(HexUnitInfo hexUnitInfo)
        {
            if (hexUnitInfo == null || hexUnitInfo.UnitInfo.BattlerInfos.Count == 0)
            {
                actorUnitInfo.gameObject.SetActive(false);
                enemyUnitInfo.gameObject.SetActive(false);
                return;
            }
            if (hexUnitInfo.TeamId.Value == (int)TeamIdType.Home)
            {
                actorUnitInfo.gameObject.SetActive(true);
                enemyUnitInfo.gameObject.SetActive(false);
                actorUnitInfo.UpdateInfo(hexUnitInfo.UnitInfo);
            } else
            if (hexUnitInfo.TeamId.Value == (int)TeamIdType.Away)
            {
                enemyUnitInfo.gameObject.SetActive(true);
                actorUnitInfo.gameObject.SetActive(false);
                enemyUnitInfo.UpdateInfo(hexUnitInfo.UnitInfo);
            }
        }

        public void ShowFieldStatus(HexUnitInfo hexUnitInfo)
        {
            if (hexUnitInfo == null || hexUnitInfo.FieldText() == "")
            {
                fieldUnitInfo.gameObject.SetActive(false);
                return;
            }
            fieldUnitInfo.gameObject.SetActive(true);
            fieldUnitInfo.UpdateInfo(hexUnitInfo);
        }

        private void InitializeHexTileList()
        {
            hexTiles.Initialize();
            hexTiles.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.SelectHexUnit));
            hexTiles.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.CancelHexUnit));
            hexTiles.SetInputHandler(InputKeyType.Up,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Up));
            hexTiles.SetInputHandler(InputKeyType.Down,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Down));
            hexTiles.SetInputHandler(InputKeyType.Right,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Right));
            hexTiles.SetInputHandler(InputKeyType.Left,() => CallViewEvent(CommandType.MoveHexMap,InputKeyType.Left));
            hexTiles.SetSelectedHandler(() => 
            {
                if (!hexTiles.Active)
                {
                    return;
                }
                CallViewEvent(CommandType.SelectHexMap,SelectHexField);
            });
            SetInputHandler(hexTiles.gameObject);
            AddViewActives(hexTiles);
        }

        public void SetHexTileList(List<ListData> hexInfos,int columnCount)
        {
            hexTiles.SetGridColumnCount(columnCount);
            hexTiles.SetData(hexInfos,true,() => 
            {
                var buttons = hexTiles.GetComponentsInChildren<Button>();
                var scrollRect = hexTiles.GetComponentInChildren<ScrollRect>();
                foreach (var button in buttons)
                {
                    var multi = button.GetComponent<MultiScroller>();
                    if (multi == null)
                    {
                        button.AddComponent<MultiScroller>();
                        multi = button.GetComponent<MultiScroller>();
                    }
                    multi.SetScrollEvent(scrollRect);
                }
            });
            SetActivate(hexTiles);
        }

        public void DeActivateHexTiles()
        {
            SetActivate(null);
        }

        public void RefreshTiles(int x,int y)
        {
            Debug.Log(x+":"+y);
            hexTiles.UpdateSelectIndex(x + y * hexTiles.GridColumnCount());
            hexTiles.Refresh(x + y * hexTiles.GridColumnCount());
        }

        public void UpdateTileItems()
        {
            hexTiles.UpdateAllItems();
        }

        public void SelectMoveBattler(List<Action> actions,HexUnitInfo hexUnitInfo)
        {
            MoveAction(actions,hexUnitInfo);
        }

        private async void MoveAction(List<Action> actions,HexUnitInfo hexUnitInfo)
        {
            if (actions.Count == 0)
            {
                //RefreshTiles(hexUnitInfo.HexField.X,hexUnitInfo.HexField.Y);
                CallViewEvent(CommandType.EndMoveBattler);
                return;
            }
            actions[0]();
            RefreshTiles(hexUnitInfo.HexField.X,hexUnitInfo.HexField.Y);
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

        public void HealUnits(List<HexUnitInfo> healUnits,List<List<int>> hpHeals)
        {
            HealUnit(healUnits,hpHeals);
        }
        
        public async void HealUnit(List<HexUnitInfo> healUnits,List<List<int>> hpHeals)
        {
            if (healUnits.Count == 0)
            {
                enemyUnitInfo.gameObject.SetActive(false);
                actorUnitInfo.gameObject.SetActive(false);
                CallViewEvent(CommandType.EndHealUnits);
                return;
            }
            var hexUnitInfo = healUnits[0];
            var hpHeal = hpHeals[0];
            RefreshTiles(hexUnitInfo.HexField.X,hexUnitInfo.HexField.Y);
            await UniTask.DelayFrame(60);
            if (hexUnitInfo.TeamId.Value == (int)TeamIdType.Home)
            {
                actorUnitInfo.gameObject.SetActive(true);
                enemyUnitInfo.gameObject.SetActive(false);
                actorUnitInfo.HealAnimation(hexUnitInfo.UnitInfo,hpHeal);
            } else
            if (hexUnitInfo.TeamId.Value == (int)TeamIdType.Away)
            {
                enemyUnitInfo.gameObject.SetActive(true);
                actorUnitInfo.gameObject.SetActive(false);
                enemyUnitInfo.HealAnimation(hexUnitInfo.UnitInfo,hpHeal);
            }


            healUnits.RemoveAt(0);
            hpHeals.RemoveAt(0);
            if (healUnits.Count > 0)
            {
                HealUnit(healUnits,hpHeals);
            } else
            {
                enemyUnitInfo.gameObject.SetActive(false);
                actorUnitInfo.gameObject.SetActive(false);
                CallViewEvent(CommandType.EndHealUnits);
            }
        }

        public void LostBattlerUnit(List<HexUnitInfo> hexUnitInfos)
        {
            LostAction(hexUnitInfos);
        }

        private async void LostAction(List<HexUnitInfo> hexUnitInfos)
        {
            if (hexUnitInfos.Count == 0)
            {
                //RefreshTiles(hexUnitInfo.HexField.X,hexUnitInfo.HexField.Y);
                return;
            }
            RefreshTiles(hexUnitInfos[0].HexField.X,hexUnitInfos[0].HexField.Y);
            await UniTask.DelayFrame(10);
            // マスを探す
            var listItems = hexTiles.GetComponentsInChildren<ListItem>().ToList();
            var lost = listItems.Find(a => a.Index == (hexUnitInfos[0].HexField.X + hexUnitInfos[0].HexField.Y * hexTiles.GridColumnCount()));
            var tile = lost.gameObject.GetComponent<HexTile>();
            tile.LostUnit();
            await UniTask.DelayFrame(30);
            hexUnitInfos.RemoveAt(0);
            tile.InitLost();
            if (hexUnitInfos.Count > 0)
            {
                LostAction(hexUnitInfos);
            } else
            {
                CallViewEvent(CommandType.EndLostBattler);
            }
        }

        private void InitializeBattleMemberSelect()
        {
            battleMemberSelectList.Initialize();
            battleMemberSelectList.SetInputHandler(InputKeyType.Decide,() => CallViewEvent(CommandType.DecideBattleMemberSelect,battleMemberSelectList.ListItemData<BattleSceneInfo>()));
            battleMemberSelectList.SetInputHandler(InputKeyType.Cancel,() => CallViewEvent(CommandType.CancelBattleMemberSelect));
            battleMemberSelectList.SetSelectedHandler(() => UpdateHelpWindow());
            SetInputHandler(battleMemberSelectList.gameObject);
            AddViewActives(battleMemberSelectList);
            battleMemberSelectList.gameObject.SetActive(false);
        }

        public void BattleMemberSelect(List<ListData> battleSceneInfos)
        {
            battleMemberSelectList.gameObject.SetActive(true);
            SetActivate(battleMemberSelectList);
            battleMemberSelectList.SetData(battleSceneInfos);
        }

        public void CancelBattleMemberSelect()
        {
            battleMemberSelectList.gameObject.SetActive(false);
            SetActivate(tacticsCommandList);
        }

        public void StartStageAnimation(Effekseer.EffekseerEffectAsset effekseerEffect)
        {
            if (effekseerEmitter == null)
            {
                return;
            }
            effekseerEmitter.Play(effekseerEffect);
            StartReadyAnimation();
        }

        public void StartReadyAnimation()
        {
            battleStartAnim.SetText("Ready!");
            battleStartAnim.StartAnim(false,0.0f);
        }

        public void StartAnimation(string text,Action endEvent = null)
        {
            battleStartAnim.SetText(text);
            battleStartAnim.StartAnim(false,0.0f,endEvent);
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

        public void SetAlcanaInfo(List<SkillInfo> skillInfos)
        {
            alcanaInfoComponent.UpdateInfo(skillInfos);
        }

        public void SetTacticsCharaLayer(List<ActorInfo> actorInfos)
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

        public void UpdateStageInfo(StageInfo stageInfo)
        {
            stageInfoComponent.UpdateInfo(stageInfo);
        }
    }

    namespace Tactics
    {
        public enum CommandType
        {
            None = 0,
            CallTacticsCommand,
            CancellTacticsCommand,
            CallStatus,
            SelectHexUnit,
            CancelHexUnit,
            SymbolDetailInfo,
            PopupSkillInfo,
            DecideBattleMemberSelect,
            CancelBattleMemberSelect,
            CallEnemyInfo,
            CallAddActorInfo,
            Back,
            SelectSideMenu,
            StageHelp,
            ScorePrize,
            AlcanaCheck,
            SelectAlcanaList,
            HideAlcanaList,
            EndShopSelect,
            SelectCharaLayer,
            SelectHexMap,
            MoveHexMap,
            EndMoveBattler,
            EndLostBattler,
            EndHealUnits,
            EndAnimation
        }
    }
}