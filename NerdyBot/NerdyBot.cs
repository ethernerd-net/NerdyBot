using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using NerdyBot.Services;
using NerdyBot.Models;

namespace NerdyBot
{
  public class NerdyBot
  {
    private DiscordSocketClient client = new DiscordSocketClient();

    private ServiceProvider svcProvider = new ServiceProvider();

    private CommandService svcCommand = new CommandService();
    private DatabaseService svcDatabase;
    private MessageService svcMessage;
    private YoutubeService svcYoutube;
    private AudioService svcAudio;

    private BotConfig cfg;

    internal NerdyBot()
    {
      this.cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<BotConfig>( System.IO.File.ReadAllText( "conf.json" ) );

      this.svcYoutube = new YoutubeService( cfg.YoutubeAPIKey, cfg.YoutubeAppName );
      this.svcMessage = new MessageService( this.client );
      this.svcAudio = new AudioService( svcMessage, this.client );
      this.svcDatabase = new DatabaseService();

      this.svcDatabase.Database.CreateTable<Guild>();

      this.client.Ready += ClientReady;
      this.client.JoinedGuild += JoinedGuild;
      this.client.UserJoined += UserJoined;
    }


    private async Task ClientReady()
    {
      await Task.Delay( 2000 );
      await client.SetGameAsync( "Not nerdy at all" );
      foreach ( var guild in this.client.Guilds )
      {
        //await guild.DefaultChannel.SendMessageAsync( "`Sorry for the wait, my Master killed my process. Don't tell him i'm back! ;)`" ); //SUPRISE, i'm back! :3
        this.svcAudio.AddGuild( guild.Id );
      }
    }

    private async Task JoinedGuild( SocketGuild guild )
    {
      long guildId = ( long )guild.Id;
      var lightGuild = this.svcDatabase.Database.Table<Guild>().Where( g => g.GuildId == guildId ).FirstOrDefault();
      if ( lightGuild == null )
      {
        //await guild.DefaultChannel.SendMessageAsync( "`Hi, my Name is NerdyBot and i'm addicted! (To be buggy and not doing what i was designed to do)`" );
        this.svcDatabase.Database.Insert( new Guild() { GuildId = guildId, WelcomeMessage = "Welcome $mention$, make yourself at home :)" } );
      }
      this.svcAudio.AddGuild( guild.Id );
    }

    private async Task UserJoined( SocketGuildUser user )
    {
      long guildId = ( long )user.Guild.Id;
      var lightGuild = this.svcDatabase.Database.Table<Guild>().Where( g => g.GuildId == guildId ).FirstOrDefault();
      if ( !string.IsNullOrEmpty( lightGuild.WelcomeMessage ) )
        await user.Guild.DefaultChannel.SendMessageAsync( lightGuild.WelcomeMessage.Replace( "$mention$", user.Mention ) );
    }

    private IEnumerable<string> preservedKeys = new string[] { "perm", "help", "stop", "purge", "leave", "join", "backup" };
    
    private async Task InstallCommands()
    {
      this.client.MessageReceived += HandleCommand;

      svcProvider.AddService( svcAudio );
      svcProvider.AddService( svcMessage );
      svcProvider.AddService( svcDatabase );
      svcProvider.AddService( svcYoutube );
      svcProvider.AddService( client );
      svcProvider.AddService( cfg );

      await svcCommand.AddModulesAsync( Assembly.GetEntryAssembly() );
    }

    private async Task HandleCommand( SocketMessage messageParam )
    {
      // Don't process the command if it was a System Message
      var message = messageParam as SocketUserMessage;
      if ( message == null )
        return;
      // Create a number to track where the prefix ends and the command begins
      int argPos = 0;
      // Determine if the message is a command, based on if it starts with '!' or a mention prefix
      if ( !( message.HasCharPrefix( this.cfg.PrefixChar, ref argPos ) || message.HasMentionPrefix( client.CurrentUser, ref argPos ) ) )
        return;
      // Create a Command Context
      var context = new CommandContext( client, message );

      await svcMessage.Log( context.Message.Content, context.User.ToString() );

      // Execute the command. (result does not indicate a return value, 
      // rather an object stating if the command executed succesfully)
      IResult result;
      if ( message.HasMentionPrefix( client.CurrentUser, ref argPos ) )
      {
        var cmd = svcCommand.Commands.First( c => c.Name == "chat" );
        var lst = new string[] { message.Content.Substring( argPos ) };
        result = await cmd.ExecuteAsync( context, lst, lst, svcProvider );
      }
      else
      {
        result = await svcCommand.ExecuteAsync( context, argPos, svcProvider );

        await messageParam.DeleteAsync();
      }
      if ( !result.IsSuccess )
        await context.Channel.SendMessageAsync( $"`{result.ErrorReason}`" );
    }

    public async Task Start()
    {
      await InstallCommands();

      await client.LoginAsync( TokenType.Bot, this.cfg.DiscordToken );
      await client.StartAsync();

      await Task.Delay( -1 );
    }
  }
}