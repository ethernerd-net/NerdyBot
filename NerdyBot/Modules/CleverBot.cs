using System.Threading.Tasks;

using Discord.Commands;
using Cleverbot.Net;

using NerdyBot.Models;
using NerdyBot.Services;

namespace NerdyBot.Modules
{
  public class CleverBotModule : ModuleBase
  {
    private CleverbotSession session;

    public BotConfig BotConfig { get; set; }
    public MessageService MessageService { get; set; }

    public CleverBotModule( BotConfig bc )
    {
      this.session = new CleverbotSession( bc.CleverBotApiKey );
    }

    [Command("chat")]
    public async Task Execute( string message )
    {
      using ( Context.Channel.EnterTypingState() )
      {
        var answer = await this.session.GetResponseAsync( message );
        MessageService.SendMessageToCurrentChannel( Context, $"{Context.User.Mention}, {answer.Response}" );
      }
    }
  }
}
