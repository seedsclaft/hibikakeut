using UnityEngine;

namespace Ryneus
{
    public class BuildingInfo
    {
        public BuildingsData Master => DataSystem.Buildings.Find(a => a.Id == Id.Value);
        public ParameterInt Id = new();
    }
}
