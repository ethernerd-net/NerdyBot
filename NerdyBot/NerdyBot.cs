﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Net;
using System.Diagnostics;
using System.Text;

using Discord;
using Discord.Audio;
using Discord.Commands;

using NerdyBot.Contracts;
using System.Threading.Tasks;
using NAudio.Wave;

namespace NerdyBot
{
  partial class NerdyBot : IClient
  {
    private DiscordClient client;
    private AudioService audio;

    private object playing = new object();

    private MainConfig conf;
    private const string MAINCFG = "cfg";

    public NerdyBot()
    {
      conf = new MainConfig( MAINCFG );
      conf.Read();

      client = new DiscordClient( x =>
       {
         x.LogLevel = LogSeverity.Info;
         x.LogHandler = LogHandler;
       } );

      client.UsingCommands( x =>
      {
        x.PrefixChar = conf.Prefix;
        x.AllowMentionPrefix = true;
      } );

      client.UsingAudio( x =>
      {
        x.Mode = AudioMode.Outgoing;
      } );
      this.audio = client.GetService<AudioService>();
    }

    private IEnumerable<string> preservedKeys = new string[] { "perm", "help", "stop", "purge", "leave", "join", "backup" };

    private List<ICommand> commands = new List<ICommand>();
    private void InitCommands()
    {
      var svc = client.GetService<CommandService>();
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
    private void InitMainCommands( CommandService svc )
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
      svc.CreateCommand( "purge" )
        .Parameter( "count", ParameterType.Required )
        .AddCheck( ( cmd, u, ch ) => u.ServerPermissions.Administrator )
        .Do( async e =>
        {
          int count;
          if ( int.TryParse( e.GetArg( "count" ), out count ) )
          {
            var msgs = await e.Channel.DownloadMessages( count );
            e.Channel.DeleteMessages( msgs );
          }
        } );
      svc.CreateCommand( "stop" )
        .Do( e =>
        {
          this.StopPlaying = true;
          e.Message.Delete();
        } );
      svc.CreateCommand( "leave" )
        .Do( e =>
        {
          if ( e.User.VoiceChannel != null )
            this.audio.Leave( e.User.VoiceChannel );
          e.Message.Delete();
        } );
      svc.CreateCommand( "join" )
        .Do( e =>
        {
          if ( e.User.VoiceChannel != null )
            this.audio.Join( e.User.VoiceChannel );
          e.Message.Delete();
        } );
      svc.CreateCommand( "backup" )
        .AddCheck( ( cmd, u, ch ) => u.ServerPermissions.Administrator )
        .Do( e =>
        {
          Backup( e );
          e.Message.Delete();
        } );
    }

    private bool CanExecute( User user, ICommand cmd )
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
    }
    private bool CanLoadCommand( ICommand command )
    {
      if ( preservedKeys.Contains( command.Config.Key ) )
        return false;
      if ( this.commands.Any( cmd => cmd.Config.Key == command.Config.Key ||
          cmd.Config.Aliases.Any( ali => ali == command.Config.Key ||
            command.Config.Aliases.Any( ali2 => ali == ali2 ) ) ) )
        return false;
      return true;
    }

    #region MainCommands
    private void RestrictCommandByRole( CommandEventArgs e, string[] args )
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
          var cmd = this.commands.FirstOrDefault( c => c.Config.Key == args[2] );
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
          var cmd = this.commands.FirstOrDefault( c => c.Config.Key == args[2] );
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
    }
    private void ShowHelp( CommandEventArgs e, IEnumerable<string> args )
    {
      StringBuilder sb = new StringBuilder();
      if ( args.Count() == 0 )
      {
        this.commands.ForEach( ( cmd ) =>
        {
          sb.AppendLine( cmd.QuickHelp() );
          sb.AppendLine();
          sb.AppendLine();
        } );
      }
      else
      {
        var command = this.commands.FirstOrDefault( cmd => cmd.Config.Key == args.First() || cmd.Config.Aliases.Any( ali => ali == args.First() ) );
        if ( commands != null )
          sb = new StringBuilder( command.FullHelp() );
        else
          sb = new StringBuilder( "Du kennst anscheinend mehr Commands als ich!" );
      }
      SendMessage( sb.ToString(), new SendMessageOptions() { TargetType = TargetType.User, TargetId = e.User.Id, Split = true, MessageType = MessageType.Block } );
    }
    private void Backup( CommandEventArgs e )
    {
      Task.Factory.StartNew( () =>
      {
        e.User.SendMessage( "not implemented" );
      } );
    }
    #endregion MainCommands

    private void LogHandler( object sender, LogMessageEventArgs e )
    {
      Console.WriteLine( "{0}: {1}", e.Source, e.Message );
    }

    public void Start()
    {
      InitCommands();
      client.ExecuteAndWait( async () =>
       {
         await client.Connect( conf.Token, TokenType.Bot );
         client.SetGame( "Not nerdy at all" );
       } );
    }

    #region IClient
    public bool StopPlaying { get; set; }
    public void DownloadAudio( string url, string outp )
    {
      try
      {
        bool transform = false;
        string ext = Path.GetExtension( url );
        Log( "downloading " + url );
        if ( ext != string.Empty )
        {
          string tempOut = Path.Combine( Path.GetDirectoryName( outp ), "temp" + ext );
          if ( ext == ".mp3" )
            tempOut = outp;

          if ( !Directory.Exists( Path.GetDirectoryName( tempOut ) ) )
            Directory.CreateDirectory( Path.GetDirectoryName( tempOut ) );

          ( new WebClient() ).DownloadFile( url, tempOut );

          transform = ( ext != ".mp3" );
        }
        else
        {
          //Externe Prozesse sind böse, aber der kann so viel :S
          //Ich könne allerdings auf die ganzen features verzichten und nen reinen yt dl anbieten
          //https://github.com/flagbug/YoutubeExtractor
          string tempOut = Path.Combine( Path.GetDirectoryName( outp ), "temp.%(ext)s" );
          ProcessStartInfo ytdl = new ProcessStartInfo();
          ytdl.WindowStyle = ProcessWindowStyle.Hidden;
          ytdl.FileName = "ext\\youtube-dl.exe";

          ytdl.Arguments = "--extract-audio --audio-quality 0 --no-playlist --output \"" + tempOut + "\" \"" + url + "\"";
          Process.Start( ytdl ).WaitForExit();
          transform = true;
        }
        Log( "download complete" );
        if ( transform )
        {
          string tempFIle = Directory.GetFiles( Path.GetDirectoryName( outp ), "temp.*", SearchOption.TopDirectoryOnly ).First();
          Log( "converting: " + Path.GetFileName( tempFIle ) );

          ProcessStartInfo ffmpeg = new ProcessStartInfo();
          ffmpeg.WindowStyle = ProcessWindowStyle.Hidden;
          ffmpeg.FileName = "ext\\ffmpeg.exe";

          ffmpeg.Arguments = "-i " + tempFIle + " -f mp3 " + outp;
          Process.Start( ffmpeg ).WaitForExit();

          File.Delete( tempFIle );
          Log( "conversion complete" );
        }
      }
      catch ( Exception ex )
      {
        throw new ArgumentException( ex.Message );
      }
    }
    public void SendAudio( ICommandUser user, string localPath, float volume = 1f, bool delAfterPlay = false )
    {
      var vUser = this.client.Servers.First().Users.FirstOrDefault( u => u.Id == user.Id );
      if ( vUser != null && vUser.VoiceChannel.Id != 0 )
      {
        var vChannel = this.client.Servers.First().VoiceChannels.FirstOrDefault( vc => vc.Id == vUser.VoiceChannel.Id );
        lock ( playing )
        {
          IAudioClient vClient = this.audio.Join( vChannel ).Result;
          Log( "playing " + Path.GetDirectoryName( localPath ), vUser.ToString() );
          try
          {
            var channelCount = this.audio.Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
            var OutFormat = new NAudio.Wave.WaveFormat( 48000, 16, channelCount ); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using ( var MP3Reader = new Mp3FileReader( localPath ) ) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using ( var resampler = new MediaFoundationResampler( MP3Reader, OutFormat ) ) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
              resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
              int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
              byte[] buffer = new byte[blockSize];
              int byteCount;

              while ( ( byteCount = resampler.Read( buffer, 0, blockSize ) ) > 0 && !StopPlaying ) // Read audio into our buffer, and keep a loop open while data is present
              {
                if ( byteCount < blockSize )
                {
                  // Incomplete Frame
                  for ( int i = byteCount; i < blockSize; i++ )
                    buffer[i] = 0;
                }
                vClient.Send( ScaleVolume.ScaleVolumeSafeNoAlloc( buffer, volume ), 0, blockSize ); // Send the buffer to Discord
              }
            }
            vClient.Wait();
          }
          catch ( Exception )
          {
            Console.WriteLine( "Format not supported." );
          }
          StopPlaying = false;
          if ( delAfterPlay )
            File.Delete( localPath );
        }
      }
    }

    public async void SendMessage( string message, SendMessageOptions options )
    {
      switch ( options.TargetType )
      {
      case TargetType.User:
        User usr = this.client.Servers.First().GetUser( options.TargetId );
        if ( !options.Split && message.Length > 1990 )
        {
          File.WriteAllText( "raw.txt", message );
          await usr.SendFile( "raw.txt" );
          File.Delete( "raw.txt" );
        }
        else
        {
          foreach ( string msg in ChunkMessage( message ) )
            usr.SendMessage( FormatMessage( msg, options.MessageType, options.Hightlight ) );
        }
        break;
      case TargetType.Channel:
        {
          Channel ch = this.client.Servers.First().GetChannel( options.TargetId );
          if ( !options.Split && message.Length > 1990 )
          {
            File.WriteAllText( "raw.txt", message );
            await ch.SendFile( "raw.txt" );
            File.Delete( "raw.txt" );
          }
          else
          {
            foreach ( string msg in ChunkMessage( message ) )
              ch.SendMessage( FormatMessage( msg, options.MessageType, options.Hightlight ) );
          }
        }
        break;
      case TargetType.Default:
      default:
        {
          Channel ch = this.client.GetChannel( conf.ResponseChannel );
          if ( !options.Split && message.Length > 1990 )
          {
            File.WriteAllText( "raw.txt", message );
            await ch.SendFile( "raw.txt" );
            File.Delete( "raw.txt" );
          }
          else
          {
            foreach ( string msg in ChunkMessage( message ) )
              ch.SendMessage( FormatMessage( msg, options.MessageType, options.Hightlight ) );
          }
        }
        break;
      }
    }

    public void Log( string text, string source = "", LogSeverity logLevel = LogSeverity.Info )
    {
      this.client.Log.Log( logLevel, source, text );
    }
    #endregion

    private readonly int chunkSize = 1990;
    private IEnumerable<string> ChunkMessage( string str )
    {
      if ( str.Length > chunkSize )
        return Enumerable.Range( 0, str.Length / chunkSize )
          .Select( i => str.Substring( i * chunkSize, chunkSize ) );
      return new string[] { str };
    }
    private string FormatMessage( string message, MessageType format, string highlight )
    {
      string formatedMessage = string.Empty;
      switch ( format )
      {
      case MessageType.Block:
        formatedMessage = "```" + highlight + Environment.NewLine + message + "```";
        break;
      case MessageType.Info:
        formatedMessage = "`" + message + "`";
        break;
      case MessageType.Normal:
      default:
        formatedMessage = message;
        break;
      }
      return formatedMessage;
    }
  }
}