using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        public ParameterInt SelectIndex = new(-1);
        public Dictionary<string, Effekseer.EffekseerEffectAsset> EffectAssets = new();
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

        public async Task LoadEffects()
        {
            // 必要なアセットをロード
            var addressPathes = new List<string>
            {
                //"NA_Effekseer/NA_curse_001",
                "MAGICALxSPIRAL/WHead1",
                "NA_Effekseer/NA_Fire_001",
                "tktk01/Cure1"
            };

            // Actor用
            foreach (var battlerInfo in _battlers)
            {
                foreach (var skillInfo in battlerInfo.Skills)
                {
                    var animation = DataSystem.Animations.Find(a => a.Id == skillInfo.Master.AnimationId);
                    if (animation != null && animation.AnimationPath != "" && !addressPathes.Contains(animation.AnimationPath))
                    {
                        addressPathes.Add(animation.AnimationPath);
                    }
                }
            }
            List<Task<Effekseer.EffekseerEffectAsset>> tasks = new();
            foreach (var addressPath in addressPathes)
            {
                var result = ResourceSystem.LoadResourceEffectAsset(addressPath);
                tasks.Add(result);
            }

            await UniTask.WaitUntil(() => !tasks.Exists(a => !a.IsCompleted));

            foreach (var addressPath in addressPathes)
            {
                var result = ResourceSystem.LoadResourceEffectAsset(addressPath);
                EffectAssets[addressPath] = result.Result;
            }
                
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
                }
                else
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
