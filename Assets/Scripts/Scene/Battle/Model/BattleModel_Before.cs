using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        public ParameterInt SelectIndex = new(-1);
        public void CreateBattleData()
        {
            _actionIndex = 0;
            _battlers.Clear();
            _reserveBattlers.Clear();
            _battleRecords.Clear();

            var actorInfos = _sceneParam.ActorInfos;
            var idx = 1;
            foreach (var actorInfo in actorInfos)
            {
                var battlerInfo = new BattlerInfo(actorInfo, actorInfo.BattleIndex.Value);
                if (battlerInfo.ActorInfo == null)
                {
                    continue;
                }
                _battlers.Add(battlerInfo);
                idx++;
            }

            var enemies = _sceneParam.EnemyInfos;
            foreach (var enemy in enemies)
            {
                // 最新データに同期
                //var battlerInfo = new BattlerInfo(enemy.EnemyData,enemy.Level.Value,enemy.Index.Value,enemy.LineIndex,enemy.BossFlag);
                var battlerInfo = enemy;
                if (battlerInfo.Index.Value == 0)
                {
                    continue;
                }
                foreach (var kind in battlerInfo.Kinds)
                {
                    if (CurrentData.PlayerInfo.CheckEnemyWeakPointDict(battlerInfo.EnemyData.Id,kind))
                    {
                        battlerInfo.SetWeakPoint(kind);
                    }
                }
                _battlers.Add(battlerInfo);
            }
            // アルカナ
            if (PartyInfo.AritifactSkills().Count > 0)
            {
                var alcanaSkills = new List<SkillInfo>();
                alcanaSkills.AddRange(PartyInfo.AritifactSkills());
                var alcana = new BattlerInfo(alcanaSkills, true, 1);
                _battleRecords[alcana.Index.Value] = new BattleRecord(alcana.Index.Value);
                _battlers.Add(alcana);
            }

            _party = new UnitInfo();
            _party.SetBattlers(FieldBattlerInfos().FindAll(a => a.IsActor));
            _troop = new UnitInfo();
            _troop.SetBattlers(FieldBattlerInfos().FindAll(a => !a.IsActor));
            //_saveBattleInfo.SetParty(_party.CopyData());
            //_saveBattleInfo.SetTroop(_troop.CopyData());
        }


        public int SelectedCharacterIndex()
        {
            var index = 0;
            var idx = 0;
            foreach (var battlerInfo in ViewBattlerActors())
            {
                if (SelectIndex.Value == idx && battlerInfo.ActorInfo != null)
                {
                    index = battlerInfo.Index.Value;
                }
                idx++;
            }
            return index;
        }

        public bool SwapSelectIndex(int selectIndex)
        {
            BattlerInfo fromBattlerInfo = null;
            var fromIndex = SelectIndex.Value + 1;
            BattlerInfo toBattlerInfo = null;
            var toIndex = selectIndex + 1;
            var idx = 0;
            foreach (var battlerInfo in ViewBattlerActors())
            {
                if (selectIndex == idx)
                {
                    toBattlerInfo = battlerInfo;
                } else
                if (SelectIndex.Value == idx)
                {
                    fromBattlerInfo = battlerInfo;
                }
                idx++;
            }
            CurrentDeckInfo.SwapBattler(fromIndex, toBattlerInfo.ActorInfo != null ? toBattlerInfo.ActorInfo.ActorId.Value : -1, toIndex);
            bool adjust = CurrentDeckInfo.AdjustEditIndexes();

            foreach (var actorIdDict in CurrentDeckInfo.ActorIdDict)
            {
                var battlerInfo = _battlers.Find(a => a.ActorInfo != null && a.ActorInfo.ActorId.Value == actorIdDict.Value);
                if (battlerInfo != null)
                {
                    battlerInfo.Index.SetValue(actorIdDict.Key);
                    battlerInfo.SetLineIndex(actorIdDict.Key > 3 ? LineType.Back : LineType.Front);
                }
            }

            _party.SetBattlers(FieldBattlerInfos().FindAll(a => a.IsActor));
            SelectIndex.SetValue(-1);
            return adjust;
        }
    }
}
