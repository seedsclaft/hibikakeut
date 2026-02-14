using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class MasterDates
    {
        private Dictionary<int, MasterData> _dates = new();
        private List<object> _masterDates = new();
        public static Dictionary<int, T> DictDates<T>(List<T> dates) where T : MasterData
        {
            var DictData = new Dictionary<int, T>();
            foreach (var masterData in dates)
            {
                DictData[masterData.Id] = masterData;
            }
            return DictData;
        }

        public static MasterDates MasterData<T>(List<T> dates) where T : MasterData
        {
            var m = new MasterDates();
            foreach (var masterData in dates)
            {
                m._dates[masterData.Id] = masterData;
                m._masterDates.Add(masterData);
            }
            return m;
        }

        public MasterData Find(int id)
        {
            return _dates.ContainsKey(id) ? _dates[id] : null;
        }

        public T Find<T>(int id) where T : MasterData
        {
            return _dates.ContainsKey(id) ? _dates[id] as T : null;
        }

        public List<T> FindAll<T>(Func<T, bool> func) where T : MasterData
        {
            return ToList<T>().Where(a => func(a)).ToList();
        }

        public List<T> ToList<T>() where T : MasterData
        {
            return _masterDates.Cast<T>().ToList();
        }
    }

    [Serializable]
    public class MasterData
    {
        public int Id;
    }

    public enum DataType
    {
        Actor = 0,
        Adventure,
        Enemies,
        Rules,
        Helps,
        Skills,
        Items,
        Stages,
        States,
        TextDates,
        Troops,
        PrizeSets,
        Animations,
        SkillTriggers,
        Achievements,
        EvaluatePrizes,
        Heroics
    }
}