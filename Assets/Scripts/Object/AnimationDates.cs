using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class AnimationDates : ScriptableObject
    {
        public List<AnimationData> Data = new();
    }

    [Serializable]
    public class AnimationData 
    {   
        public int Id;
        public string AnimationPath;
        public bool MakerEffect;
        public AnimationPosition Position;
        public float Scale;
        public float Speed;
        public int DamageTiming;
    }

    public enum AnimationPosition
    {
        Center = 0,
        Down = 1
    }
}