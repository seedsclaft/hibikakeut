using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class ArtifactListModel : BaseModel
    {
        public ArtifactListModel()
        {
        }

        public List<SkillInfo> ArtifactSkills()
        {
            return PartyInfo?.AritifactSkills();
        }
    }
}