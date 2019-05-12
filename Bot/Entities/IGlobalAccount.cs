using System;
using System.Collections.Generic;

namespace Bot.Entities
{
    public interface IGlobalAccount : IEquatable<IGlobalAccount>
    {
        ulong Id { get; }
        Dictionary<string, string> Tags { get; }
    }
}
