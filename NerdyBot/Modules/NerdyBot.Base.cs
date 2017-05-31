using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;

using NerdyBot.Services;

namespace NerdyBot.Commands
{
  public class NerdyBotBase : ModuleBase
  {
    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }
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
       MessageService.SendMessage( Context, sb.ToString(), new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, Split = true, MessageType = MessageType.Block } );
     }
  }
}
