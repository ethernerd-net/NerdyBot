using Discord;
using Discord.Audio;
using Discord.Commands;
using NerdyBot.Commands;
using Newtonsoft.Json;
using System;
using System.IO;

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
    }

    private async void Discord_MessageReceived( object sender, MessageEventArgs e )
    {
      bool isCommand = true;
      if ( output == null )
        output = e.Server.GetChannel( cfg.ResponseChannel );

      if ( e.Message.Text.StartsWith( cfg.Prefix + "n" ) )
        CommandFactory.GetCommand<TagCommand>().Command( e, this );
      else if ( e.Message.Text.StartsWith( cfg.Prefix + "unload" ) )
      {
        if ( e.User.ServerPermissions.Administrator )
        {
        }
        else
          WriteInfo( "Du bist zu unwichtig für diesen Command!", e.Channel );
      }
      else
        isCommand = false;

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
