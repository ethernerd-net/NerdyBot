using Discord.Commands;
using NerdyBot.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  public class NerdyBotBase : ModuleBase
  {
    private AudioService svcAudio;
    public NerdyBotBase( AudioService audio )
    {
      this.svcAudio = audio;
    }

    [Command( "purge" ), Alias( "t" )]
    public async Task Purge( int count )
    {
      Context.Message.DeleteAsync();
      Context.Channel.DeleteMessagesAsync( await Context.Channel.GetMessagesAsync( count ).First() );
    }

    [Command( "stop" )]
    public async Task StopPlaying()
    {
      this.svcAudio.StopPlaying = true;
      Context.Message.DeleteAsync();
    }
  }
}
