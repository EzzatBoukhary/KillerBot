using System.Collections.Generic;

namespace Bot.Features.RoleAssignment
{
    public class RoleByPhraseSettings
    {
        public List<ulong> RolesIds { get; set; } = new List<ulong>();
        public List<string> Phrases { get; set; } = new List<string>();
        public List<RoleByPhraseRelation> Relations { get; set; } = new List<RoleByPhraseRelation>();
    }
}