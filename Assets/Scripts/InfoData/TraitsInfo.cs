using System;

namespace Ryneus
{
    [Serializable]
    public class TraitsInfo
    {
        private HeroicData _master = null;
        public HeroicData Master => _master ??= DataSystem.FindHeroic(HeroicId.Value);
        public ParameterInt HeroicId = new();
        public ParameterInt Level = new(1);

        public float AllParam()
        {
            return Level.Value * Master.Param;
        }

        public string AllParamtext()
        {
            var allParam = AllParam();
            if (allParam > 0)
            {
                return "+" + allParam;
            }
            return allParam.ToString();
        }
    }
}
