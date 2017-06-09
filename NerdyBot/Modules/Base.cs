using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using NerdyBot.Services;
using NerdyBot.Models;

namespace NerdyBot.Modules
{
  public class NerdyBotBase : ModuleBase
  {
    public BotConfig BotConfig { get; set; }
    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }
    public DiscordSocketClient Client { get; set; }

    [Command( "purge" ), Alias( "t" ), RequireUserPermission( Discord.GuildPermission.Administrator )]
    public async Task Purge( int count )
    {
      await Context.Channel.DeleteMessagesAsync( Context.Channel.GetMessagesAsync( count ).First().Result );
    }

    [Command( "stop" )]
    public void StopPlaying()
    {
      AudioService.Playing[Context.Guild.Id] = false;
    }

    [Command( "leave" )]
    public async Task LeaveChannel()
    {
      await AudioService.LeaveChannel( Context.Guild.Id );
    }

    [Command( "join" )]
    public async Task JoinChannel()
    {
      await AudioService.JoinChannel( Context );
    }

    [Command( "exit" )]
    public async Task Exit()
    {
      if ( Context.User.Id == BotConfig.AdminUserId )
      {
        await Context.Message.DeleteAsync();
        await Client.StopAsync();
        //TODO safe consoleoutput to file
        //Environment.Exit( 0 );
      }
    }
    
    [Command( "help" )]
    public async Task ShowHelp()
    {
      StringBuilder sb = new StringBuilder();
      Assembly assembly = Assembly.GetExecutingAssembly();

      foreach ( Type type in assembly.GetTypes().Where( type => type.BaseType == typeof( ModuleBase ) ) )
      {
        MethodInfo help;
        if ( ( help = type.GetMethod( "QuickHelp" ) ) != null )
        {
           sb.AppendLine( (string)help.Invoke( null, null ) );
           sb.AppendLine();
           sb.AppendLine();
         }
       }
       await MessageService.SendMessageToCurrentUser( Context, sb.ToString(), MessageType.Info, true );
     }
  }
}
