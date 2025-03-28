using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class DebugBattleData : MonoBehaviour
    {
        public TMP_InputField consoleInputField = null;
        [SerializeField] private List<int> inBattleActorIds = null;
        [SerializeField] private int troopId = 0;
        [SerializeField] private int troopLv = 0;
        [SerializeField] private bool testBattle = false;
        public bool TestBattle => testBattle;
        [SerializeField] private string advName = "";
        public string AdvName => advName;

        void Start()
        {
    #if UNITY_EDITOR
            consoleInputField.onEndEdit.AddListener((a) => CallConsoleCommand(a));
            consoleInputField.gameObject.SetActive(true);
    #else
            gameObject.SetActive(false);
    #endif
        }

        public void MakeBattleActor()
        {
            /*
            GameSystem.CurrentStageData = new SaveStageInfo();
            var currentStageData = GameSystem.CurrentStageData;
            currentStageData.Initialize();
            currentStageData.InitializeStageData(1);
            if (testBattle)
            {
                var TestBattleData = Resources.Load<TestBattleData>("Data/TestBattle").TestBattleDates;
                var ActorIndex = 1;
                var symbolInfoList = new List<SymbolResultInfo>();
                foreach (var TestBattle in TestBattleData)
                {
                    if (TestBattle.IsActor)
                    {
                        var actorData = DataSystem.FindActor(TestBattle.BattlerId);
                        currentStageData.AddTestActor(actorData,TestBattle.Level);
                        var actorInfo = currentStageData.Party.ActorInfos.Find(a => a.ActorId == actorData.Id);
                        actorInfo.SetBattleIndex(ActorIndex);
                        actorInfo.SetLineIndex(TestBattle.IsFront ? LineType.Front : LineType.Back);
                        ActorIndex++;
                        symbolInfoList.AddRange(OpeningStageSymbolInfos(actorData.Id));
                    } else
                    {    
                        symbolInfoList.AddRange(DebugStageSymbolInfos(TestBattle.BattlerId));
                    }
                }
                foreach (var symbolInfo1 in symbolInfoList)
                {
                    currentStageData.Party.SetSymbolResultInfo(symbolInfo1,false);
                }
                var troopInfo = currentStageData.CurrentStage.TestTroops(troopId,troopLv);
                var stageSymbol = new StageSymbolData
                {
                    StageId = 1,
                    Seek = 1,
                    SeekIndex = 0
                };
                var symbolInfo = new SymbolInfo(stageSymbol);
                symbolInfo.SetTroopInfo(troopInfo);
                var record = new SymbolResultInfo(symbolInfo);
                currentStageData.Party.SetSymbolResultInfo(record);
            } else
            {
                foreach (var actor in DataSystem.Actors)
                { 
                    if (inBattleActorIds.Contains(actor.Key))
                    {
                        currentStageData.AddTestActor(actor.Value,0);
                    }
                }
                var idx = 1;
                foreach (var actorInfo in currentStageData.Party.ActorInfos)
                {
                    actorInfo.SetBattleIndex(idx);
                    idx++;
                }
                currentStageData.CurrentStage.TestTroops(troopId,troopLv);
            }
            */
        }

        
        public List<SymbolResultInfo> OpeningStageSymbolInfos(int actorId)
        {
            var symbolInfos = new List<SymbolResultInfo>();
            var symbols = DataSystem.FindStage(0).StageSymbols;
            foreach (var symbol in symbols)
            {
                var symbolInfo = new SymbolInfo(symbol);
                var getItemInfos = new List<GetItemInfo>();
                if (symbol.PrizeSetId > 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        prizeSet.GetItem.Param1 = actorId;
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        getItemInfos.Add(getItemInfo);
                    }
                }
                symbolInfo.AddGetItemInfos(getItemInfos);
                //symbolInfo.SetSelected(true);
                symbol.SeekIndex = actorId;
                //var record = new SymbolResultInfo(symbolInfo);
                //record.SetSelected(true);
                //symbolInfos.Add(record);
            }
            return symbolInfos;
        }
        public List<SymbolResultInfo> DebugStageSymbolInfos(int skillId)
        {
            var symbolInfos = new List<SymbolResultInfo>();
            var symbols = DataSystem.FindStage(0).StageSymbols;
            foreach (var symbol in symbols)
            {
                var symbolInfo = new SymbolInfo(symbol);
                var getItemInfos = new List<GetItemInfo>();
                if (symbol.PrizeSetId > 0)
                {
                    var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                    foreach (var prizeSet in prizeSets)
                    {
                        prizeSet.GetItem.Param1 = skillId;
                        var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                        getItemInfos.Add(getItemInfo);
                    }
                }
                symbolInfo.AddGetItemInfos(getItemInfos);
                //symbolInfo.SetSelected(true);
                symbol.SeekIndex = skillId;
                //var record = new SymbolResultInfo(symbolInfo);
                //record.SetSelected(true);
                //symbolInfos.Add(record);
            }
            return symbolInfos;
        }
    #if UNITY_EDITOR
        private BattleModel _model;
        private BattlePresenter _presenter;
        private BattleView _view;

        public void SetDebugger(BattleModel model,BattlePresenter presenter,BattleView view)
        {
            _model = model;
            _presenter = presenter;
            _view = view;
        }
        
        void CallConsoleCommand(string inputText)
        {
            if (_presenter == null) return;
            if (consoleInputField.text == "T0")
            {
                _presenter.SetDebug(true);
            }
            if (consoleInputField.text == "T1")
            {
                _presenter.SetDebug(false);
            }
            if (consoleInputField.text.Contains("AC"))
            {/*
                var replace = consoleInputField.text.Replace("AC","");
                var command = replace.Split(",");
                if (command.Length != 2) return;
                var battlerInfo = _model.GetBattlerInfo(int.Parse( command[0] ));
                if (battlerInfo == null) return;
                var skillInfo = battlerInfo.Skills.Find(a => a.Master.Id == int.Parse( command[1] ));
                if (skillInfo == null) return;
                _model.SetActionBattler(battlerInfo.Index);
                ActionInfo actionInfo = _model.MakeActionInfo(battlerInfo,skillInfo,false,false);
                _model.AddActionInfo(actionInfo,false);
                _presenter.MakeActionResultInfoTargetIndexes(_model.MakeAutoSelectIndex(actionInfo));
                */
            }
        }

        private void Update() {
        }
    #endif
    }
}