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
  class NerdyBot
  {
    private const char PREFIX = '!';
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
      this.svcAudio = new AudioService( svcMessage );
      this.svcDatabase = new DatabaseService();

      this.client.Ready += ClientReady;
    }

    private async Task ClientReady()
    {
      //await client.SetGameAsync( "Not nerdy at all" );
      foreach ( var guild in this.client.Guilds )
        this.svcAudio.AddGuild( guild.Id );
    }

    private IEnumerable<string> preservedKeys = new string[] { "perm", "help", "stop", "purge", "leave", "join", "backup" };
    
    private async Task InstallCommands()
    {
      client.MessageReceived += HandleCommand;

      svcProvider.AddService( svcAudio );
      svcProvider.AddService( svcMessage );
      svcProvider.AddService( svcDatabase );
      svcProvider.AddService( svcYoutube );
      svcProvider.AddService( client );
      svcProvider.AddService( cfg );

      await svcCommand.AddModulesAsync( Assembly.GetEntryAssembly() );
    }

    public async Task HandleCommand( SocketMessage messageParam )
    {
      // Don't process the command if it was a System Message
      var message = messageParam as SocketUserMessage;
      if ( message == null )
        return;
      // Create a number to track where the prefix ends and the command begins
      int argPos = 0;
      // Determine if the message is a command, based on if it starts with '!' or a mention prefix
      if ( !( message.HasCharPrefix( PREFIX, ref argPos ) || message.HasMentionPrefix( client.CurrentUser, ref argPos ) ) )
        return;
      // Create a Command Context
      var context = new CommandContext( client, message );

      await svcMessage.Log( context.Message.Content, context.User.ToString() );

      // Execute the command. (result does not indicate a return value, 
      // rather an object stating if the command executed succesfully)
      var result = await svcCommand.ExecuteAsync( context, argPos, svcProvider );
      if ( !result.IsSuccess )
        await context.Channel.SendMessageAsync( result.ErrorReason );

      await messageParam.DeleteAsync();
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