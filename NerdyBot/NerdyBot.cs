using Discord;
using Discord.Audio;
using Discord.Commands;
using NerdyBot.Commands;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NerdyBot
{
  partial class NerdyBot : IClient
  {
    DiscordClient discord;
    Channel output;
    MainConfig conf;

    private const string MAINCFG = "cfg.json";

    public NerdyBot()
    {
      conf = new MainConfig( MAINCFG );
      conf.Read();

      discord = new DiscordClient( x =>
       {
         x.LogLevel = LogSeverity.Info;
         x.LogHandler = LogHandler;
       } );

      discord.UsingCommands( x =>
      {
        x.PrefixChar = conf.Prefix;
        x.AllowMentionPrefix = true;
      } );

      discord.MessageReceived += Discord_MessageReceived;

      discord.UsingAudio( x =>
      {
        x.Mode = AudioMode.Outgoing;
      } );
      InitCommands();
    }

    private string[] preservedKeys = new string[] { "perm" };

    private List<ICommand> commands = new List<ICommand>();
    private void InitCommands()
    {
      try
      {
        Assembly assembly = Assembly.GetExecutingAssembly();

        foreach ( Type type in assembly.GetTypes() )
        {
          if ( type.GetInterface( "ICommand" ) != null )
          {
            ICommand command = ( ICommand )Activator.CreateInstance( type, null, null );
            command.Init();
            if ( preservedKeys.Contains( command.Key ) || this.commands.Any( cmd => cmd.Key == command.Key || cmd.Aliases.Any( a => a == command.Key || command.Aliases.Any( al => a == al ) ) ) )
              throw new InvalidOperationException( "Duplicated command key or alias: " + command.Key );
            this.commands.Add( command );
          }
        }
      }
      catch ( Exception ex )
      {
        throw new InvalidOperationException( "Schade",  ex);
      }
    }

    private async void Discord_MessageReceived( object sender, MessageEventArgs e )
    {
      bool isCommand = false;
      if ( output == null )
        output = e.Server.GetChannel( conf.ResponseChannel );

      if ( e.Message.Text.StartsWith( conf.Prefix.ToString() ) )
      {
        string[] args = e.Message.Text.Substring(1).Split( ' ' );
        if ( args[0] == "perm" )
          RestrictCommandByRole( e, args.Skip( 1 ).ToArray() );
        else
        {
          foreach ( var cmd in this.commands )
          {
            if ( args[0] == cmd.Key || cmd.Aliases.Any( a => args[0] == a ) )
            {
              isCommand = true;
              if ( CanExecute( e.User, cmd ) )
                cmd.Execute( e, args.Skip( 1 ).ToArray(), this );
            }
          }
        }
      }

      if ( isCommand )
        e.Message.Delete();
    }

    private bool CanExecute( User user, ICommand cmd )
    {
      bool ret = false;
      if ( user.ServerPermissions.Administrator )
        return true;

      switch ( cmd.RestrictionType )
      {
      case Commands.Config.RestrictType.Admin:
        ret = false;
        break;
      case Commands.Config.RestrictType.None:
        ret = true;
        break;
      case Commands.Config.RestrictType.Allow:
        ret = ( cmd.RestrictedRoles.Count() == 0 || user.Roles.Any( r => cmd.RestrictedRoles.Any( id => r.Id == id ) ) );
        break;
      case Commands.Config.RestrictType.Deny:
        ret = ( cmd.RestrictedRoles.Count() == 0 || !user.Roles.Any( r => cmd.RestrictedRoles.Any( id => r.Id == id ) ) );
        break;
      default:
        throw new InvalidOperationException( "WTF?!?!" );
      }
      return ret;
    }

    private void RestrictCommandByRole( MessageEventArgs e, string[] args )
    {
      if ( e.User.ServerPermissions.Administrator )
      {
        switch ( args[0].ToLower() )
        {
        case "add":
        case "rem":
          if ( args.Count() != 3 )
            WriteInfo( "Äh? Ich glaube die Parameteranzahl stimmt so nicht!" );
          else
          {
            var role = e.Server.Roles.FirstOrDefault( r => r.Name == args[1] );
            var cmd = this.commands.FirstOrDefault( c => c.Key == args[2] );
            if ( cmd == null )
              WriteInfo( "Command nicht gefunden!" );
            else
            {
              if ( cmd.RestrictedRoles.Contains( role.Id ) )
              {
                if ( args[0] == "rem" )
                  cmd.RestrictedRoles.Remove( role.Id );
              }
              else
              {
                if ( args[0] == "add" )
                  cmd.RestrictedRoles.Add( role.Id );
              }
              WriteInfo( "Permissions updated for " + cmd.Key, e.Channel );
            }
          }
          break;
        case "type":
          if ( args.Count() != 3 )
            WriteInfo( "Äh? Ich glaube die Parameteranzahl stimmt so nicht!" );
          else
          {
            var cmd = this.commands.FirstOrDefault( c => c.Key == args[2] );
            switch ( args[1].ToLower() )
            {
            case "admin":
              cmd.RestrictionType = Commands.Config.RestrictType.Admin;
              break;
            case "none":
              cmd.RestrictionType = Commands.Config.RestrictType.None;
              break;
            case "deny":
              cmd.RestrictionType = Commands.Config.RestrictType.Deny;
              break;
            case "allow":
              cmd.RestrictionType = Commands.Config.RestrictType.Allow;
              break;
            default:
              WriteInfo( args[1] + " ist ein invalider Parameter", e.Channel );
              return;
            }
            WriteInfo( "Permissions updated for " + cmd.Key, e.Channel );
          }
          break;
        case "help":
          //TODO HELP
          break;
        default:
          WriteInfo( "Invalider Parameter!" );
          break;
        }

      }
      else
      {
        WriteInfo( "Du bist zu unwichtig dafür!" );
      }
    }

    private void LogHandler( object sender, LogMessageEventArgs e )
    {
      Console.WriteLine( e.Message );
    }

    public void Start()
    {
      discord.ExecuteAndWait( async () =>
       {
         await discord.Connect( conf.Token, TokenType.Bot );
         discord.SetGame( "Not nerdy at all" );
       } );
    }

    #region IClient
    public MainConfig Config { get { return this.conf; } }


    public T GetService<T>() where T : class, IService
    {
      return discord.GetService<T>();
    }

    public async void Write( string info, Channel ch = null )
    {
      ch = ch ?? output;
      ch.SendMessage( info );
    }
    public async void WriteInfo( string info, Channel ch = null )
    {
      ch = ch ?? output;
      ch.SendMessage( "`" + info + "`" );
    }
    public async void WriteBlock( string info, string highlight = "", Channel ch = null )
    {
      ch = ch ?? output;
      if ( info.Length + highlight.Length + 6 > 2000 )
      {
        File.WriteAllText( "raw.txt", info );
        await ch.SendFile( "raw.txt" );
        File.Delete( "raw.txt" );
      }
      else
        ch.SendMessage( "```" + highlight + Environment.NewLine + info + "```" );
    }
    public void Log( string text )
    {
      this.discord.Log.Log( LogSeverity.Info, "", text );
    }
    #endregion
  }
}
