using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ariadne
{
    /// <Summary>
    /// When dungeon parts data is missing, set default dungeon parts data.
    /// Dungeon parts data is used for drawing dungeon.
    /// </Summary>
    [Obsolete("Default dungeon parts will be loaded automatically. You don't have to use this class.")]
    public static class DungeonPartsManager
    {
        // static readonly string Path = "DungeonPartsData/";
        // static readonly string FileName = "DefaultParts";

        /// <Summary>
        /// Set the default dungeon parts.
        /// </Summary>
        [Obsolete("Default dungeon parts will be loaded automatically. You don't have to use this class.")]
        public static DungeonPartsMasterData GetDefaultDungeonParts()
        {
            // DungeonPartsMasterData dungeonParts = Resources.Load<DungeonPartsMasterData>(Path + FileName);

            // if (dungeonParts == null)
            // {
            //     Debug.LogWarning("Dungeon parts prefab 'DefaultParts' is missing...");
            // }

            return null;
        }
    }
}