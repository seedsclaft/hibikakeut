using System;

namespace Ryneus
{
    [Serializable]
    public class TraitsInfo
    {
        public HeroicData _master = null;
        public HeroicData Master => DataSystem.FindHeroic(HeroicId.Value);
        public ParameterInt HeroicId = new();
        public ParameterInt Level = new(1);
    }
}
