using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace Ryneus
{
    public abstract class DataSystem
    {
        public static Dictionary<DataType, MasterDates> Dates = new();
        public static List<SkillTriggerData> SkillTriggers = new();

        public static List<TutorialData> TutorialDates = new();
        public static SystemData System;

        public static List<SoundData> BGM = new();
        public static List<SoundData> SE = new();


        public static List<SystemData.CommandData> TacticsCommand => System.TacticsCommandData;
        public static List<SystemData.CommandData> TitleCommand => System.TitleCommandData;
        public static List<SystemData.CommandData> StatusCommand => System.StatusCommandData;
        public static List<SystemData.OptionCommand> OptionCommand => System.OptionCommandData;

        public static Color PowerUpColor => new(128, 255, 128);
        public static string PowerUpColorTag => "<color=#E09018>";
        public static Color PowerDownColor => new(255, 128, 64);

        public static async UniTask<bool> LoadData()
        {
            Dates[DataType.Actor] = MasterDates.MasterData(ResourceSystem.LoadResource<ActorDates>("Data/Actors").Data);
            Dates[DataType.Adventure] = MasterDates.MasterData(ResourceSystem.LoadResource<AdvDates>("Data/Adventures").Data);
            Dates[DataType.Enemies] = MasterDates.MasterData(ResourceSystem.LoadResource<EnemyDates>("Data/Enemies").Data);
            Dates[DataType.Rules] = MasterDates.MasterData(ResourceSystem.LoadResource<RuleDates>("Data/Rules").Data);
            Dates[DataType.Helps] = MasterDates.MasterData(ResourceSystem.LoadResource<HelpDates>("Data/Helps").Data);
            Dates[DataType.Skills] = MasterDates.MasterData(ResourceSystem.LoadResource<SkillDates>("Data/Skills").Data);
            Dates[DataType.Items] = MasterDates.MasterData(ResourceSystem.LoadResource<ItemDates>("Data/Items").Data);
            Dates[DataType.Equipment] = MasterDates.MasterData(ResourceSystem.LoadResource<EquipmentDates>("Data/Equipments").Data);
            Dates[DataType.Stages] = MasterDates.MasterData(ResourceSystem.LoadResource<StageDates>("Data/Stages").Data);
            Dates[DataType.States] = MasterDates.MasterData(ResourceSystem.LoadResource<StateDates>("Data/States").Data);

            System = ResourceSystem.LoadResource<SystemData>("Data/System");
            Dates[DataType.TextDates] = MasterDates.MasterData(System.SystemTextData);
            Dates[DataType.Troops] = MasterDates.MasterData(ResourceSystem.LoadResource<TroopDates>("Data/Troops").Data);
            Dates[DataType.PrizeSets] = MasterDates.MasterData(ResourceSystem.LoadResource<PrizeSetDates>("Data/PrizeSets").Data);
            Dates[DataType.Animations] = MasterDates.MasterData(ResourceSystem.LoadResource<AnimationDates>("Data/Animations").Data);

            SkillTriggers = ResourceSystem.LoadResource<SkillTriggerDates>("Data/SkillTrigger").Data;
            Dates[DataType.Achievements] = MasterDates.MasterData(ResourceSystem.LoadResource<AchievementDates>("Data/Achievements").Data);
            Dates[DataType.EvaluatePrizes] = MasterDates.MasterData(ResourceSystem.LoadResource<EvaluatePrizeDates>("Data/EvaluatePrizes").Data);
            Dates[DataType.Heroics] = MasterDates.MasterData(ResourceSystem.LoadResource<HeroicDates>("Data/Heroics").Data);
            TutorialDates = ResourceSystem.LoadResource<TutorialDates>("Data/Tutorial").Data;
            BGM = ResourceSystem.LoadResource<SoundDates>("Data/BGM").Data;
            SE = ResourceSystem.LoadResource<SoundDates>("Data/SE").Data;

            await UniTask.WaitUntil(LoadedDates);
            return true;
        }

        private static bool LoadedDates()
        {
            return SE != null;
        }

        public static ActorData FindActor(int id)
        {
            return Dates[DataType.Actor].Find<ActorData>(id);
        }

        public static EnemyData FindEnemy(int id)
        {
            return Dates[DataType.Enemies].Find<EnemyData>(id);
        }

        public static SkillData FindSkill(int id)
        {
            return Dates[DataType.Skills].Find<SkillData>(id);
        }

        public static List<SkillData> SkillDates()
        {
            return Dates[DataType.Skills].ToList<SkillData>();
        }

        public static ItemData FindItem(int id)
        {
            return Dates[DataType.Items].Find<ItemData>(id);
        }

        public static EquipmentData FindEquipment(int id)
        {
            return Dates[DataType.Equipment].Find<EquipmentData>(id);
        }

        public static AnimationData FindAnimation(int id)
        {
            return Dates[DataType.Animations].Find<AnimationData>(id);
        }

        public static StageData FindStage(int id)
        {
            return Dates[DataType.Stages].Find<StageData>(id);
        }

        public static StageData FindNextStage(int currentStageId)
        {
            return Dates[DataType.Stages].Find<StageData>(currentStageId);
        }

        public static StateData FindState(int id)
        {
            return Dates[DataType.States].Find<StateData>(id);
        }

        public static Ariadne.FloorMapMasterData FindDungeonFloor(int id)
        {
            var master = ResourceSystem.LoadResource<Ariadne.DungeonMasterData>("Data/Dungeon" + id.ToString("D4"));
            return master.floorList[0];
        }

        public static TroopData FindTroop(int id)
        {
            return Dates[DataType.Troops].Find<TroopData>(id);
        }

        private static TextData GetTextData(int id)
        {
            return Dates[DataType.TextDates].Find<TextData>(id);
        }

        public static string GetText(int id)
        {
            var textData = GetTextData(id);
            if (textData != null)
            {
                return textData.Text;
            }
            LogOutput.Log(id + ":が不足");
            return id + ":が不足";
        }

        public static string GetHelp(int id)
        {
            var textData = GetTextData(id);
            return textData != null ? textData.Help : "";
        }

        public static string GetReplaceText(int id, params object[] args)
        {
            var textData = GetTextData(id);
            if (textData != null)
            {
                try
                {
                    return string.Format(textData.Text, args);
                }
                catch
                {
                    return textData.Text;
                }
            }
            return "";
        }

        public static string GetReplaceDecimalText(int value)
        {
            var numText = value.ToString();
            var index = 0;
            var charList = new List<string>();
            for (int i = numText.Length - 1; i >= 0; i--)
            {
                charList.Add(numText[i].ToString());
                index++;
                if (index % 3 == 0 && i != 0)
                {
                    charList.Add(",");
                }
            }
            charList.Reverse();
            var text = "";
            foreach (var character in charList)
            {
                text += character;
            }
            return text;
        }

        public static SoundData GetBGMByKey(string key)
        {
            var bGMData = BGM.Find(a => a.Key == key);
            if (bGMData != null)
            {
                return bGMData;
            }
            return null;
        }

        public static SoundData GetBGM(int bgmId)
        {
            var bGMData = BGM.Find(a => a.Id == bgmId);
            if (bGMData != null)
            {
                return bGMData;
            }
            return null;
        }
/*
        public static async UniTask<AudioClip> GetSE(string fileName)
        {
            string sePath = "Assets/Audios/SE/" + fileName + ".ogg";
            var result = await Ryneus.ResourceSystem.LoadAsset<AudioClip>(sePath);
            return result;
        }
*/
        public static List<ListData> HelpText(int id)
        {
            var data = Dates[DataType.Helps].Find(id) as HelpData;
            if (data != null)
            {
                var texts = data.Help.Split("\n").ToList();
                return ListData.MakeListData(texts);
            }
            return null;
        }
    }

    [Serializable]
    public class TextData : MasterData
    {
        public string Text;
        public string Help;
        public string Feature;
        public string Relief;
    }
}