using Discord;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  interface ICommand
  {
    Task Command( MessageEventArgs msg, IClient client );
  }
}
