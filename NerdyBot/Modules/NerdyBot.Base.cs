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

    [Command( "purge" ), Alias( "t" )]
    public async Task Purge( int count )
    {
      if ( ( await Context.Guild.GetUserAsync( Context.User.Id ) ).GuildPermissions.Administrator )
        await Context.Channel.DeleteMessagesAsync( await Context.Channel.GetMessagesAsync( count ).First() );
      else
        await MessageService.Log( "Accesspermission Violation attempt", Context.User.ToString() );
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
       await MessageService.SendMessage( Context, sb.ToString(), new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, Split = true, MessageType = MessageType.Info } );
     }
  }
}
