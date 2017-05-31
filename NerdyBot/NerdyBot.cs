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
    private AudioService svcAudio;
    private MessageService svcMessage;
    
    internal NerdyBot()
    {      
      this.svcMessage = new MessageService( this.client );
      this.svcAudio = new AudioService( svcMessage );
      this.svcDatabase = new DatabaseService();

      this.svcDatabase.Database.CreateTable<ModuleConfig>();
      if ( this.svcDatabase.Database.Table<ModuleConfig>().Any( mc => mc.Name == "base" ) )
        this.svcDatabase.Database.Insert( new ModuleConfig() { Name = "base", ApiKey = "INSERT DISCORD TOKEN HERE" } ); //TODO
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
      svcProvider.AddService( svcDatabase );
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
      if ( !( message.HasCharPrefix( PREFIX, ref argPos ) || message.HasMentionPrefix( client.CurrentUser, ref argPos ) ) )
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

      await client.LoginAsync( TokenType.Bot, this.svcDatabase.Database.Table<Models.ModuleConfig>().First( mc => mc.Name == "base" ).ApiKey );
      await client.StartAsync();
      //await client.SetGameAsync( "Not nerdy at all" );

      await Task.Delay( -1 );
    }
  }
}