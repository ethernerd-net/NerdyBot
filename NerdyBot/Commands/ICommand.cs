using Discord;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  interface ICommand
  {
    string Key { get; }
    bool NeedAdmin { get; }
    void Init();
    Task Execute( MessageEventArgs msg, string[] args, IClient client );
  }
}
