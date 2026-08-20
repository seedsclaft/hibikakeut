using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class HelpDates : ScriptableObject
    {
        public List<HelpData> Data = new();
    }

    [Serializable]
    public class HelpData : MasterData
    {
        public string Key;
        public string GuideImagePath;
        public int CommonHelpId;
        public string Help;
        public string EnglishHelp;

        public string GetHelp()
        {
            if (GameSystem.GetLanguage() == Language.English)
            {
                return EnglishHelp;
            }
            return Help;
        }
    }
}