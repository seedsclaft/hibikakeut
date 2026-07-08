using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class GuideModel : BaseModel
    {
        private List<HelpData> _guideDates = null;
        private ParameterInt _currentIndex = new();
        public HelpData GuideData => _guideDates.Count > _currentIndex.Value ? _guideDates[_currentIndex.Value] : null;
        
        public GuideModel()
        {
            GuideSceneInfo SceneParam = (GuideSceneInfo)GameSystem.SceneStackManager.LastTemplate;
            _guideDates = DataSystem.Dates[DataType.Helps].FindAll<HelpData>(a => a.Key == SceneParam.GuideKey);
            if (SceneParam.SelectHelpId > 0)
            {
                _currentIndex.SetValue(_guideDates.FindIndex(a => a.Id == SceneParam.SelectHelpId));
            }
        }

        public Sprite GuideSprite()
        {
            return ResourceSystem.LoadGuideSprite(GuideData?.GuideImagePath);
        }

        public List<ListData> GuideTextList()
        {
            return DataSystem.HelpText(GuideData.Id);
        }

        public bool NeedLeftPage()
        {
            return _currentIndex.Value > 0;
        }

        public bool NeedRightPage()
        {
            return _currentIndex.Value != _guideDates.Count-1 && _guideDates.Count != 1;
        }

        public void PageLeft()
        {
            _currentIndex.GainValue(-1, 0);
        }

        public void PageRight()
        {
            _currentIndex.GainValue(1, 0, _guideDates.Count - 1);
        }

        public int CallHelpId()
        {
            return GuideData.CommonHelpId;
        }
    }

    public class GuideSceneInfo
    {
        public string GuideKey = "";
        public int SelectHelpId = 0; // 最初に表示したいId
    }
}
