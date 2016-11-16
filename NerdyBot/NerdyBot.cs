using Discord;
using Discord.Audio;
using Discord.Commands;
using NerdyBot.Commands;
using Newtonsoft.Json;
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
    Config cfg;

    private const string MAINCFG = "cfg.json";

    public NerdyBot()
    {
      cfg = JsonConvert.DeserializeObject<Config>( File.ReadAllText( MAINCFG ) );

      discord = new DiscordClient( x =>
       {
         x.LogLevel = LogSeverity.Info;
         x.LogHandler = LogHandler;
       } );

      discord.UsingCommands( x =>
      {
        x.PrefixChar = cfg.Prefix;
        x.AllowMentionPrefix = true;
      } );

      discord.MessageReceived += Discord_MessageReceived;

      discord.UsingAudio( x =>
      {
        x.Mode = AudioMode.Outgoing;
      } );
      InitCommands();
    }

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
            if ( this.commands.Any( cmd => cmd.Key == command.Key || cmd.Aliases.Any( a => a == command.Key || command.Aliases.Any( al => a == al ) ) ) )
              throw new ArgumentException( "Duplikated command key or alias: " + command.Key );
            command.Init();
            this.commands.Add( command );
          }
        }
      }
      catch ( Exception )
      {
        // throw new InvalidOperationException(ex);
      }
    }

    private async void Discord_MessageReceived( object sender, MessageEventArgs e )
    {
      bool isCommand = false;
      if ( output == null )
        output = e.Server.GetChannel( cfg.ResponseChannel );

      if ( e.Message.Text.StartsWith( cfg.Prefix.ToString() ) )
      {
        string[] args = e.Message.Text.Substring(1).Split( ' ' );
        foreach ( var cmd in this.commands )
        {
          if ( args[0] == cmd.Key || cmd.Aliases.Any( a => args[0] == a ) )
          {
            isCommand = true;
            //TODO: Rolllen Checken
            cmd.Execute( e, args.Skip( 1 ).ToArray(), this );
          }
        }
      }

      if ( isCommand )
        e.Message.Delete();
    }

    private void LogHandler( object sender, LogMessageEventArgs e )
    {
      Console.WriteLine( e.Message );
    }


    public void Start()
    {
      discord.ExecuteAndWait( async () =>
       {
         await discord.Connect( cfg.Token, TokenType.Bot );
         discord.SetGame( "Not nerdy at all" );
       } );
    }

    #region IClient
    public Config Config { get { return this.cfg; } }


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
