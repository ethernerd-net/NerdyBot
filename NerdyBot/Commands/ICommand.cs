using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  interface ICommand
  {
    string Key { get; }
    IEnumerable<string> Aliases { get; }
    List<ulong> RestrictedRoles { get; }
    Config.RestrictType RestrictionType { get; set; }
    void Init();
    Task Execute( MessageEventArgs msg, string[] args, IClient client );
    string QuickHelp();
  }
}
