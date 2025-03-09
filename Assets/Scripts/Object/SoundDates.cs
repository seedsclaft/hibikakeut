using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class SoundDates : ScriptableObject
    {
        [SerializeField] public List<SoundData> Data = new();
    }

    [Serializable]
    public class SoundData
    {
        public int Id;
        public string Key;
        public string FileName;
        public float Volume;
        public bool Loop;
        public string CrossFade;
        public float Pitch = 1;
    }
}