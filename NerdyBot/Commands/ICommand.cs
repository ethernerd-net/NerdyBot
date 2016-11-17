using Discord;
using NerdyBot.Commands.Config;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  interface ICommand
  {
    BaseCommandConfig Config { get; }
    void Init();
    Task Execute( MessageEventArgs msg, string[] args, IClient client );
    string QuickHelp();
    string FullHelp( char prefix );
  }
}
