using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using NerdyBot.Contracts;
using NerdyBot.Config;

namespace NerdyBot
{
  partial class NerdyBot
  {
    private DiscordSocketClient client;

    private ServiceProvider svcProvider = new ServiceProvider();

    private CommandService svcCommand = new CommandService();
    private AudioService svcAudio = new AudioService();
    private MessageService svcMessage;


    private MainConfig conf;
    private const string MAINCFG = "cfg";
    
    public NerdyBot()
    {
      conf = new MainConfig( MAINCFG );
      conf.Read();

      svcMessage = new MessageService( conf.ResponseChannel );
    }

    private IEnumerable<string> preservedKeys = new string[] { "perm", "help", "stop", "purge", "leave", "join", "backup" };

    //private List<ICommand> commands = new List<ICommand>();
    private async Task InstallCommands()
    {
      client.MessageReceived += HandleCommand;

      svcProvider.AddService( svcAudio );
      svcProvider.AddService( svcMessage );
      svcProvider.AddService( client );

      await svcCommand.AddModulesAsync( Assembly.GetEntryAssembly() );
     // await svcCommand.AddModuleAsync( typeof( NerdyBot.
    }
    /*private void InitMainCommands( CommandService svc )
    {
      svc.CreateCommand( "help" )
        .Parameter( "args", ParameterType.Multiple )
        .Do( e =>
        {
          ShowHelp( e, e.Args );
          e.Message.Delete();
        } );
      svc.CreateCommand( "perm" )
        .Parameter( "args", ParameterType.Multiple )
        .AddCheck( ( cmd, u, ch ) => u.ServerPermissions.Administrator )
        .Do( e =>
        {
          RestrictCommandByRole( e, e.Args );
          e.Message.Delete();
        } );
      svc.CreateCommand( "leave" )
        .Do( e =>
        {
          //if ( e.User.VoiceChannel != null )
          //  this.svcAudio.Leave( e.User.VoiceChannel );
          e.Message.Delete();
        } );
    }*/


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

    /*
    private void InitCommands()
    {
      InitMainCommands( svc );
      try
      {
        Assembly assembly = Assembly.GetExecutingAssembly();

        foreach ( Type type in assembly.GetTypes() )
        {
          if ( type.GetInterface( "ICommand" ) != null )
          {
            ICommand command = ( ICommand )Activator.CreateInstance( type, null, null );
            command.Init( this );
            if ( !CanLoadCommand( command ) )
              throw new InvalidOperationException( "Duplicated command key or alias: " + command.Config.Key );
            this.commands.Add( command );

            svc.CreateCommand( command.Config.Key )
              .Alias( command.Config.Aliases.ToArray() )
              .Parameter( "args", ParameterType.Multiple )
              .Do( e =>
              {
                command.Execute( new CommandMessage()
                {
                  Arguments = e.Args,
                  Text = e.Message.Text,
                  Channel = new CommandChannel()
                  {
                    Id = e.Channel.Id,
                    Mention = e.Channel.Mention,
                    Name = e.Channel.Name
                  },
                  User = new CommandUser()
                  {
                    Id = e.User.Id,
                    Name = e.User.Name,
                    FullName = e.User.ToString(),
                    Mention = e.User.Mention,
                    Permissions = e.User.ServerPermissions
                  }
                } );
                e.Message.Delete();
              } );
          }
        }
      }
      catch ( Exception ex )
      {
        throw new InvalidOperationException( "Schade", ex );
      }
    }
    */
    /*
    private bool CanExecute( IUser user, ICommand cmd )
    {
      bool ret = false;
      if ( user.ServerPermissions.Administrator )
        return true;

      switch ( cmd.Config.RestrictionType )
      {
      case Commands.Config.RestrictType.Admin:
        ret = false;
        break;
      case Commands.Config.RestrictType.None:
        ret = true;
        break;
      case Commands.Config.RestrictType.Allow:
        ret = ( cmd.Config.RestrictedRoles.Count() == 0 || user.Roles.Any( r => cmd.Config.RestrictedRoles.Any( id => r.Id == id ) ) );
        break;
      case Commands.Config.RestrictType.Deny:
        ret = ( cmd.Config.RestrictedRoles.Count() == 0 || !user.Roles.Any( r => cmd.Config.RestrictedRoles.Any( id => r.Id == id ) ) );
        break;
      default:
        throw new InvalidOperationException( "WTF?!?!" );
      }
      return ret;
    }*/
    /*private bool CanLoadCommand( ICommand command )
    {
      if ( preservedKeys.Contains( command.Config.Key ) )
        return false;
      if ( this.svcCommand.Any( cmd => cmd.Config.Key == command.Config.Key ||
          cmd.Config.Aliases.Any( ali => ali == command.Config.Key ||
            command.Config.Aliases.Any( ali2 => ali == ali2 ) ) ) )
        return false;
      return true;
    }*/

    #region MainCommands
   /* private void RestrictCommandByRole( CommandEventArgs e, string[] args )
    {
      string info = string.Empty;
      string option = args.First().ToLower();
      switch ( option )
      {
      case "add":
      case "rem":
        if ( args.Count() != 3 )
          info = "Äh? Ich glaube die Parameteranzahl stimmt so nicht!";
        else
        {
          var role = e.Server.Roles.FirstOrDefault( r => r.Name == args[1] );
          var cmd = this.svcCommand.FirstOrDefault( c => c.Config.Key == args[2] );
          if ( cmd == null )
            info = "Command nicht gefunden!";
          else
          {
            if ( cmd.Config.RestrictedRoles.Contains( role.Id ) )
            {
              if ( option == "rem" )
                cmd.Config.RestrictedRoles.Remove( role.Id );
            }
            else
            {
              if ( option == "add" )
                cmd.Config.RestrictedRoles.Add( role.Id );
            }
            info = "Permissions updated for " + cmd.Config.Key;
          }
        }
        break;
      case "type":
        if ( args.Count() != 3 )
          info = "Äh? Ich glaube die Parameteranzahl stimmt so nicht!";
        else
        {
          var cmd = this.svcCommand.FirstOrDefault( c => c.Config.Key == args[2] );
          switch ( args[1].ToLower() )
          {
          case "admin":
            cmd.Config.RestrictionType = Commands.Config.RestrictType.Admin;
            break;
          case "none":
            cmd.Config.RestrictionType = Commands.Config.RestrictType.None;
            break;
          case "deny":
            cmd.Config.RestrictionType = Commands.Config.RestrictType.Deny;
            break;
          case "allow":
            cmd.Config.RestrictionType = Commands.Config.RestrictType.Allow;
            break;
          default:
            SendMessage( args[1] + " ist ein invalider Parameter", new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = e.Channel.Id, MessageType = MessageType.Info } );
            return;
          }
          info = "Permissions updated for " + cmd.Config.Key;
        }
        break;
      case "help":
        //TODO HELP
        break;
      default:
        info = "Invalider Parameter!";
        break;
      }
      SendMessage( info, new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = e.Channel.Id, MessageType = MessageType.Info } );
    }*/
   /* private void ShowHelp( CommandEventArgs e, IEnumerable<string> args )
    {
      StringBuilder sb = new StringBuilder();
      if ( args.Count() == 0 )
      {
        this.svcCommand.ForEach( ( cmd ) =>
        {
          sb.AppendLine( cmd.QuickHelp() );
          sb.AppendLine();
          sb.AppendLine();
        } );
      }
      else
      {
        var command = this.svcCommand.FirstOrDefault( cmd => cmd.Config.Key == args.First() || cmd.Config.Aliases.Any( ali => ali == args.First() ) );
        if ( svcCommand != null )
          sb = new StringBuilder( command.FullHelp() );
        else
          sb = new StringBuilder( "Du kennst anscheinend mehr Commands als ich!" );
      }
      SendMessage( sb.ToString(), new SendMessageOptions() { TargetType = TargetType.User, TargetId = e.User.Id, Split = true, MessageType = MessageType.Block } );
    }*/
    #endregion MainCommands

    private void LogHandler( object sender, LogMessageEventArgs e )
    {
      Console.WriteLine( "{0}: {1}", e.Source, e.Message );
    }

    public async Task Start()
    {
      this.client = new DiscordSocketClient();
      await InstallCommands();
      

      await client.LoginAsync( TokenType.Bot, conf.Token );
      await client.StartAsync();
      //await client.SetGameAsync( "Not nerdy at all" );
      
      await Task.Delay( -1 );
    }

  }
}