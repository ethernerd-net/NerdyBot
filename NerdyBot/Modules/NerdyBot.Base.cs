using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using NerdyBot.Contracts;

namespace NerdyBot.Commands
{
  public class NerdyBotBase : ModuleBase
  {
    public AudioService AudioService { get; set; }
    public DiscordSocketClient Client { get; set; }

    [Command( "purge" ), Alias( "t" )]
    public async Task Purge( int count )
    {
      Context.Channel.DeleteMessagesAsync( await Context.Channel.GetMessagesAsync( count ).First() );
    }

    [Command( "stop" )]
    public async Task StopPlaying()
    {
      this.AudioService.StopPlaying = true;
    }

    [Command( "leave" )]
    public async Task LeaveChannel()
    {
      this.AudioService.LeaveChannel();
    }

    [Command( "exit" )]
    public async Task Exit()
    {
      if ( Context.Guild.GetUserAsync( Context.User.Id ).Result.GuildPermissions.Administrator )
        Client.StopAsync();
    }
  }
}
