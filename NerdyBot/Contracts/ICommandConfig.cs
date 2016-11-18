using NerdyBot.Commands.Config;
using System.Collections.Generic;

namespace NerdyBot.Contracts
{
  interface ICommandConfig
  {
    string Key { get; }
    IEnumerable<string> Aliases { get; }
    List<ulong> RestrictedRoles { get; }
    RestrictType RestrictionType { get; set; }
  }
}
