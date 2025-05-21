using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariadne
{
    /// <Summary>
    /// Definition of dungeon parts data.
    /// These parts do not depend on map attributes.
    /// </Summary>
    [CreateAssetMenu(fileName = "DungeonBaseParts", menuName = "Ariadne/DungeonBasePartsData", order = AriadneMenuOrder.DungeonBasePartsData)]
    public class DungeonBasePartsData : ScriptableObject
    {
        public GameObject sizeBaseObj;
        public GameObject outsideWallObj;
        public GameObject groundObj;
        public GameObject ceilingObj;
    }
}