using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  interface ICommand
  {
    string Key { get; }
    IEnumerable<string> Aliases { get; }
    bool NeedAdmin { get; }
    void Init();
    Task Execute( MessageEventArgs msg, string[] args, IClient client );
  }
}
