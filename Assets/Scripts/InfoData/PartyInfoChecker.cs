using UnityEngine;

namespace Ryneus
{
    public class PartyInfoChecker : SingletonMonoBehaviour<PartyInfoChecker>
    {
        [SerializeField] private bool encountZero = false;
        [SerializeField] private bool moveDungeon = false;
        [SerializeField] private int dungeonId = -1;
        [SerializeField] private bool encountEnemy = false;
        [SerializeField] private bool encountBossEnemy = false;
        [SerializeField] private bool allLearnSkills = false;
        [SerializeField] private bool getAllItems = false;
        [SerializeField] private bool getAllEquipments = false;
        [SerializeField] private bool clearAchivements = false;
        [SerializeField] private PartyInfo partyInfo = null;
        [SerializeField] private DeckInfo deckInfo = null;
        public void UpdateInfo()
        {
            partyInfo = GameSystem.GameInfo.PartyInfo;
            deckInfo = partyInfo.CurrentDeckInfo;
        }

        private void Update()
        {
            UpdateEncountZero();
            UpdateMoveDungeon();
            UpdateEncountEnemy();
            UpdateEncountBossEnemy();
            UpdateAllLearnSkills();
            UpdateGetAllItems();
            UpdateGetAllEquipments();
            UpdateClearAchivements();
        }

        private void UpdateEncountZero()
        {
            if (deckInfo != null && encountZero)
            {
                deckInfo.Encount.SetValue(0);
            }
        }

        private void UpdateMoveDungeon()
        {
            if (deckInfo != null && moveDungeon)
            {
                var currentScene = GameSystem.Instance.CurrentScene;
                if (currentScene.GetType() == typeof(DungeonView))
                {
                    var stageData = DataSystem.FindStage(dungeonId);
                    var floor = DataSystem.FindDungeonFloor(dungeonId);
                    partyInfo.CurrentDeckInfo.SetPosition(dungeonId, floor.entrancePos.x, floor.entrancePos.y, (int)floor.enteringDir);
                    partyInfo.CurrentDeckInfo.StageNo.SetValue(stageData.StageNo);
            
                    GameSystem.Instance.Model.MakeStageInfo(dungeonId, false);
                    currentScene.CommandGotoSceneChange(Scene.Dungeon);
                }
                moveDungeon = false;
            }
        }

        private void UpdateEncountEnemy()
        {
            if (deckInfo != null && encountEnemy)
            {
                var currentScene = GameSystem.Instance.CurrentScene;
                if (currentScene.GetType() == typeof(DungeonView))
                {
                    var battleSceneInfo = new BattleSceneInfo
                    {
                        ActorInfos = partyInfo.CurrentDeckActorInfos(),
                        EnemyInfos = GameSystem.Instance.Model.ForceBattleTroopInfos(-1),
                        GetItemInfos = new(),
                        IsEnableDefeat = true,
                    };
                    //PlayBattleBgm();
                    currentScene.CallSystemCommand(Base.CommandType.FlashEffect);
                    currentScene.CallSystemCommand(Base.CommandType.PlayEffect);
                    currentScene.CommandChangeViewToTransition(null);
                    //_view.ChangeUIActive(false);
                    currentScene.CommandSceneChange(Scene.Battle, battleSceneInfo);
                    //SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                }
                encountEnemy = false;
            }
        }

        private void UpdateEncountBossEnemy()
        {
            if (deckInfo != null && encountBossEnemy)
            {
                var currentScene = GameSystem.Instance.CurrentScene;
                if (currentScene.GetType() == typeof(DungeonView))
                {
                    var stageEvents = GameSystem.Instance.Model.StageEventDates;
                    var findAll = stageEvents.FindAll(a => a.Type == StageEventType.ForceBossBattle);
                    if (findAll.Count > 0)
                    {
                        var battleSceneInfo = new BattleSceneInfo
                        {
                            ActorInfos = partyInfo.CurrentDeckActorInfos(),
                            EnemyInfos = GameSystem.Instance.Model.ForceBattleTroopInfos(findAll[^1].Param, findAll[^1].Param3),
                            GetItemInfos = new(),
                            IsEnableDefeat = true,
                        };
                        //PlayBattleBgm();
                        currentScene.CallSystemCommand(Base.CommandType.FlashEffect);
                        currentScene.CallSystemCommand(Base.CommandType.PlayEffect);
                        currentScene.CommandChangeViewToTransition(null);
                        //_view.ChangeUIActive(false);
                        currentScene.CommandSceneChange(Scene.Battle, battleSceneInfo);
                        //SoundManager.Instance.PlayStaticSe(SEType.BattleStart);
                    }
                }
                encountBossEnemy = false;
            }
        }

        private void UpdateAllLearnSkills()
        {
            if (partyInfo == null)
            {
                return;
            }
            if (allLearnSkills)
            {
                var skills = DataSystem.Dates[DataType.Skills].FindAll<SkillData>(a => a.Id > 1000 && a.Rank > 0);
                foreach (var skill in skills)
                {
                    if (skill.Id < 1000 || skill.Rank == 0)
                    {
                        continue;
                    }
                    foreach (var actorInfo in partyInfo.ActorInfos)
                    {
                        actorInfo.GainSkillMastary(skill.Id);
                    }
                }
                allLearnSkills = false;
            }
        }

        private void UpdateGetAllItems()
        {
            if (partyInfo == null)
            {
                return;
            }
            if (getAllItems)
            {
                foreach (var item in DataSystem.Dates[DataType.Items].ToList<ItemData>())
                {
                    var getItemData = new GetItemData
                    {
                        Type = GetItemType.Item,
                        Param1 = item.Id,
                        Param2 = 99
                    };
                    var getitemInfo = new GetItemInfo(getItemData);
                    partyInfo.AddGetItemInfo(getitemInfo);
                }
                getAllItems = false;
            }
        }

        private void UpdateGetAllEquipments()
        {
            
            if (partyInfo == null)
            {
                return;
            }
            if (getAllEquipments)
            {
                foreach (var equipment in DataSystem.Dates[DataType.Equipment].ToList<EquipmentData>())
                {
                    var getItemData = new GetItemData
                    {
                        Type = GetItemType.Equipment,
                        Param1 = equipment.Id,
                        Param2 = 1
                    };
                    var getitemInfo = new GetItemInfo(getItemData);
                    partyInfo.AddGetItemInfo(getitemInfo);
                }
                getAllEquipments = false;
            }
        }

        private void UpdateClearAchivements()
        {
            if (partyInfo == null)
            {
                return;
            }
            if (clearAchivements)
            {
                var achievementInfos = partyInfo.AchievementInfos;
                foreach (var achievementInfo in achievementInfos)
                {
                    if (achievementInfo.Master.Rank != partyInfo.MissionRank.Value)
                    {
                        continue;
                    }
                    achievementInfo.Achieved.SetValue(true);
                }
                clearAchivements = false;
            }
        }
    }
}
