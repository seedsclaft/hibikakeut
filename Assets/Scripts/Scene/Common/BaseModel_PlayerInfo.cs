using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BaseModel
    {
        public string PlayerName()
        {
            if (CurrentData.PlayerInfo == null)
            {
                return "";
            }
            return CurrentData.PlayerInfo.PlayerName.Value;
        }

        public string PlayerId()
        {
            if (CurrentData.PlayerInfo == null)
            {
                return "";
            }
            return CurrentData.PlayerInfo.UserId.ToString();
        }

        public void ClearGame()
        {
            CurrentData.PlayerInfo.GainClearCount();
        }

        public void AddPlayerInfoActorSkillId(int actorId)
        {
            foreach (var skillInfo in PartyMembers().Find(a => a.ActorId.Value == actorId).ChangeAbleSkills())
            {
                AddPlayerInfoSkillId(skillInfo.Id.Value);
            }
        }

        public void AddPlayerInfoSkillId(int skillId)
        {
            CurrentData.PlayerInfo.AddSkillId(skillId);
        }
    }
}
