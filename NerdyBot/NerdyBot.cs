using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using NerdyBot.Services;
using NerdyBot.Config;

namespace NerdyBot
{
  class NerdyBot
  {
    private DiscordSocketClient client = new DiscordSocketClient();

    private ServiceProvider svcProvider = new ServiceProvider();

    private CommandService svcCommand = new CommandService();
    private AudioService svcAudio;
    private MessageService svcMessage;

    private MainConfig conf;
    private const string MAINCFG = "cfg";
    
    internal NerdyBot()
    {
      conf = new MainConfig( MAINCFG );
      conf.Read();
      
      this.svcMessage = new MessageService( this.client, conf.ResponseChannel );
      this.svcAudio = new AudioService( svcMessage );
    }

    private async Task ClientReady()
    {
      await client.SetGameAsync( "Not nerdy at all" );
    }

    private IEnumerable<string> preservedKeys = new string[] { "perm", "help", "stop", "purge", "leave", "join", "backup" };
    
    private async Task InstallCommands()
    {
      client.MessageReceived += HandleCommand;

      svcProvider.AddService( svcAudio );
      svcProvider.AddService( svcMessage );
      svcProvider.AddService( client );

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
      if ( !( message.HasCharPrefix( conf.Prefix, ref argPos ) || message.HasMentionPrefix( client.CurrentUser, ref argPos ) ) )
        return;
      // Create a Command Context
      var context = new CommandContext( client, message );
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

      await client.LoginAsync( TokenType.Bot, conf.Token );
      await client.StartAsync();
      //await client.SetGameAsync( "Not nerdy at all" );

      await Task.Delay( -1 );
    }

  }
}