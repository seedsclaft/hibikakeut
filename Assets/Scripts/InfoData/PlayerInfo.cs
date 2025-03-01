using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class PlayerInfo
    {
        public PlayerInfo()
        {
        }

        public ParameterInt UserId = new();
        public void SetUserId()
        {
            if (UserId.Value == 0)
            {
                return;
            }
            int strong = 100;
            int sec = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            UserId.SetValue(sec + (strong*UnityEngine.Random.Range(0,strong)));
        }

        public ParameterString PlayerName = new();

        private int _clearCount = 0;
        public int ClearCount => _clearCount;
        public void GainClearCount()
        {
            //_clearCount++;
        }        
        
        private List<int> _skillIds = new ();
        public List<int> SkillIds => _skillIds;
        public void AddSkillId(int skillId)
        {
            if (!_skillIds.Contains(skillId))
            {
                _skillIds.Add(skillId);
            }
        }

        private List<int> _readTutorials = new ();
        public List<int> ReadTutorials => _readTutorials;
        public void AddReadTutorials(int id) => _readTutorials.Add(id);

        private Dictionary<int,List<KindType>> _enemyWeakPointDict = new ();
        public Dictionary<int,List<KindType>> EnemyWeakPointDict => _enemyWeakPointDict;
        public void AddEnemyWeakPointDict(int enemyId,KindType kindType)
        {
            if (CheckEnemyWeakPointDict(enemyId,kindType))
            {
                return;
            }
            if (!_enemyWeakPointDict.ContainsKey(enemyId) || _enemyWeakPointDict[enemyId] == null)
            {
                _enemyWeakPointDict[enemyId] = new ();
            }
            _enemyWeakPointDict[enemyId].Add(kindType);
        }
        
        public bool CheckEnemyWeakPointDict(int enemyId,KindType kindType)
        {
            if (_enemyWeakPointDict.ContainsKey(enemyId))
            {
                if (_enemyWeakPointDict[enemyId] != null && _enemyWeakPointDict[enemyId].Contains(kindType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}