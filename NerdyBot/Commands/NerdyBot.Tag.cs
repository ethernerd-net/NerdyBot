using Discord;
using Discord.Audio;
using NAudio.Wave;
using NerdyBot.Commands.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  class TagCommand : ICommand
  {
    private CommandConfig<Tag> cfg;
    private const string CFGPATH = "tag.json";
    private const string DEFAULTKEY = "tag";
    private static readonly string[] DEFAULTALIASES = new string[] { "t" };


    private bool stop = false;
    private object playing = new object();

    private IEnumerable<string> KeyWords
    {
      get
      {
        return new string[] { "create", "delete", "edit", "list", "raw", "info", "help", "stop" };
      }
    }

    #region ICommand
    public string Key { get { return this.cfg.Key; } }
    public IEnumerable<string> Aliases { get { return this.cfg.Aliases; } }
    public bool NeedAdmin { get { return false; } }

    public void Init()
    {
      this.cfg = new CommandConfig<Tag>( CFGPATH, DEFAULTKEY, DEFAULTALIASES );
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( () =>
      {
        switch ( args[0].ToLower() )
        {
        case "create":
          if ( args.Count() >= 4 )
            Create( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "delete":
          if ( args.Count() == 2 )
            Delete( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "edit":
          if ( args.Count() >= 4 )
            Edit( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "info":
          if ( args.Count() >= 2 )
            Info( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "raw":
          if ( args.Count() >= 2 )
            Raw( msg, args, client );
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;

        case "list":
          List( msg, client );
          break;

        case "stop":
          stop = true;
          break;

        default:
          if ( args.Count() == 1 )
          {
            stop = false;
            Send( msg, args, client );
          }
          else
            client.WriteInfo( "Invalid parameter count, check help for... guess what?", msg.Channel );
          break;
        }
      }, TaskCreationOptions.None );
    }
    #endregion ICommand

    private string GetTypeString( TagType type )
    {
      switch ( type )
      {
      case TagType.Text:
        return "T";
      case TagType.Sound:
        return "S";
      case TagType.Url:
        return "U";
      default:
        throw new ArgumentException( "WTF??!" );
      }
    }

    private async void Create( MessageEventArgs msg, string[] args, IClient client )
    {
      if ( this.cfg.Items.Exists( t => t.Name == args[1].ToLower() ) )
        client.WriteInfo( "Tag '" + args[1] + "' existiert bereits!!", msg.Channel );
      else if ( KeyWords.Contains( args[1].ToLower() ) )
        client.WriteInfo( args[1] + "' ist ein reservierter Tag!!", msg.Channel );
      else
      {
        Tag tag = new Tag();
        tag.Name = args[1].ToLower();
        tag.Author = msg.User.ToString();
        tag.CreateDate = DateTime.Now;
        tag.Count = 0;
        tag.Volume = 100;
        tag.Entries = new List<string>();

        switch ( args[2].ToLower() )
        {
        case "text":
          tag.Type = TagType.Text;
          AddTextToTag( tag, args.Skip( 3 ).ToArray() );
          break;

        case "sound":
          tag.Type = TagType.Sound;
          AddSoundToTag( tag, args.Skip( 3 ).ToArray(), client );
          break;

        case "url":
          tag.Type = TagType.Url;
          AddUrlToTag( tag, args.Skip( 3 ).ToArray() );
          break;
        default:
          client.WriteInfo( args[2] + " ist ein invalider Parameter", msg.Channel );
          return;
        }
        this.cfg.Items.Add( tag );
        this.cfg.Write();
        client.WriteInfo( "Tag '" + tag.Name + "' erstellt!", msg.Channel );
      }
    }

    private async void Delete( MessageEventArgs msg, string[] args, IClient client )
    {
      var tag = this.cfg.Items.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        client.WriteInfo( "Tag '" + args[1] + "' existiert nicht!", msg.Channel );
      else
      {
        if ( tag.Type == TagType.Sound )
          Directory.Delete( Path.Combine( "sounds", tag.Name ), true );

        this.cfg.Items.Remove( tag );
        this.cfg.Write();
        client.WriteInfo( "Tag '" + tag.Name + "' delete!", msg.Channel );
      }
    }

    private async void Edit( MessageEventArgs msg, string[] args, IClient client )
    {
      var tag = this.cfg.Items.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        client.WriteInfo( "Tag '" + args[1] + "' existiert nicht!", msg.Channel );
      else
      {
        string[] entries = args.Skip( 3 ).ToArray();

        switch ( args[2] )
        {
        case "add":
          switch ( tag.Type )
          {
          case TagType.Text:
            AddTextToTag( tag, entries );
            break;
          case TagType.Sound:
            AddSoundToTag( tag, entries, client );
            break;
          case TagType.Url:
            AddUrlToTag( tag, entries );
            break;
          default:
            throw new ArgumentException( "WTF?!?!" );
          }
          break;
        case "remove":
          int remCount = RemoveTagEntry( tag, entries );
          client.WriteInfo( remCount + " / " + entries.Count() + " removed", msg.Channel );
          break;
        case "rename":
          if ( this.cfg.Items.FirstOrDefault( t => t.Name == entries[0] ) == null )
          {
            if ( tag.Type != TagType.Text )
            {
              string dirName = tag.Type == TagType.Sound ? "sounds" : "pics";
              Directory.Move( Path.Combine( dirName, tag.Name ), Path.Combine( dirName, entries[0] ) );
            }
            tag.Name = entries[0];
          }
          else
            client.WriteInfo( "Tag Name '" + entries[0] + "' wird bereits verwendet!", msg.Channel );
          break;
        case "volume":
          break;
        default:
          client.WriteInfo( "Die Option Name '" + args[2] + "' ist nicht valide!", msg.Channel );
          return;
        }
        this.cfg.Write();
      }
    }

    private async void List( MessageEventArgs msg, IClient client )
    {
      var tagsInOrder = this.cfg.Items.OrderBy( x => x.Name );
      StringBuilder sb = new StringBuilder( "" );
      if ( tagsInOrder.Count() > 0 )
      {
        char lastHeader = '<';
        foreach ( Tag t in tagsInOrder )
        {
          if ( t.Name[0] != lastHeader )
          {
            if ( lastHeader != '<' )
              sb.Remove( sb.Length - 2, 2 );
            lastHeader = t.Name[0];
            sb.AppendLine();
            sb.AppendLine( "# " + lastHeader + " #" );
          }
          sb.Append( "[" + t.Name + "]" );
          sb.Append( "(" + GetTypeString( t.Type ) + "|" + t.Entries.Count() + ")" );
          sb.Append( ", " );
        }
        sb.Remove( sb.Length - 2, 2 );
      }
      client.WriteBlock( sb.ToString(), "md", msg.Channel );
    }

    private async void Info( MessageEventArgs msg, string[] args, IClient client )
    {
      var tag = this.cfg.Items.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        client.WriteInfo( args[1] + " existiert nicht!", msg.Channel );
      else
      {
        StringBuilder sb = new StringBuilder( "==== " + tag.Name + " =====" );
        sb.AppendLine();
        sb.AppendLine();

        sb.Append( "Author: " );
        sb.AppendLine( tag.Author );

        sb.Append( "Typ: " );
        sb.AppendLine( Enum.GetName( typeof( TagType ), tag.Type ) );

        sb.Append( "Erstellungs Datum: " );
        sb.AppendLine( tag.CreateDate.ToLongDateString() );

        sb.Append( "Hits: " );
        sb.AppendLine( tag.Count.ToString() );

        sb.Append( "Anzahl Einträge: " );
        sb.AppendLine( tag.Entries.Count.ToString() );

        client.WriteBlock( sb.ToString(), "", msg.Channel );
      }
    }

    private async void Raw( MessageEventArgs msg, string[] args, IClient client )
    {
      var tag = this.cfg.Items.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        client.WriteInfo( args[1] + " existiert nicht!", msg.Channel );
      else
      {
        StringBuilder sb = new StringBuilder( "==== " + tag.Name + " ====" );
        sb.AppendLine();
        sb.AppendLine();

        foreach ( string entry in tag.Entries )
          sb.AppendLine( entry );

        client.WriteBlock( sb.ToString(), "", msg.Channel );
      }
    }

    private async void Send( MessageEventArgs msg, string[] args, IClient client )
    {
      var tag = this.cfg.Items.FirstOrDefault( t => t.Name == args[0].ToLower() );
      if ( tag == null )
        client.WriteInfo( args[0] + " existiert nicht!", msg.Channel );
      else
      {
        int idx = ( new Random() ).Next( 0, tag.Entries.Count() );
        switch ( tag.Type )
        {
        case TagType.Text:
          client.WriteBlock( tag.Entries[idx], "", msg.Channel );
          break;
        case TagType.Sound:
          if ( msg.User.VoiceChannel != null )
            SendAudio( client.GetService<AudioService>(), msg.User.VoiceChannel, tag, idx, client );
          else
            client.WriteInfo( "In einen Voicechannel du musst!", msg.Channel );
          break;
        case TagType.Url:
          client.Write( tag.Entries[idx], msg.Channel );
          break;
        default:
          throw new ArgumentException( "WTF?!" );
        }
        tag.Count++;
        this.cfg.Write();
      }
    }

    private void AddTextToTag( Tag tag, string[] args )
    {
      string text = string.Empty;
      for ( int i = 0; i < args.Count(); i++ )
        text += " " + args[i];

      tag.Entries = text.Split( new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries ).ToList();
    }
    private void AddSoundToTag( Tag tag, string[] args, IClient client )
    {
      string path = Path.Combine( "sounds", tag.Name );
      Directory.CreateDirectory( path );
      int listCount = tag.Entries.Count;

      for ( int i = 0; i < args.Count(); i++ )
      {
        DownloadAudio( args[i], Path.Combine( path, ( listCount + i ) + ".mp3" ), client );
        tag.Entries.Add( args[i] );
      }
    }
    private void AddUrlToTag( Tag tag, string[] args )
    {
      tag.Entries.AddRange( args );
    }
    private int RemoveTagEntry( Tag tag, string[] args )
    {
      int remCount = 0;
      switch ( tag.Type )
      {
      case TagType.Text:
        string text = string.Empty;
        for ( int i = 0; i < args.Count(); i++ )
          text += " " + args[i];

        foreach ( string entry in text.Split( new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries ) )
          if ( tag.Entries.Remove( entry ) )
            remCount++;

        break;
      case TagType.Sound:
      case TagType.Url:
        for ( int i = 0; i < args.Count(); i++ )
        {
          int idx = tag.Entries.FindIndex( s => s == args[i] );
          if ( idx >= 0 )
          {
            tag.Entries.RemoveAt( idx );
            remCount++;
            if ( tag.Type == TagType.Sound )
              File.Delete( Path.Combine( "sounds", tag.Name, idx + ".mp3" ) );
          }
        }

        break;
      default:
        throw new ArgumentException( "WTF?!?!" );
      }
      return remCount;
    }

    private void DownloadAudio( string url, string outp, IClient client )
    {
      try
      {
        bool transform = false;
        string ext = Path.GetExtension( url );
        client.Log( "downloading " + url );
        if ( ext != string.Empty )
        {
          string tempOut = Path.Combine( Path.GetDirectoryName( outp ), "temp" + ext );
          if ( ext == ".mp3" )
            tempOut = outp;

          ( new WebClient() ).DownloadFile( url, tempOut );

          transform = ( ext != ".mp3" );
        }
        else
        {
          string tempOut = Path.Combine( Path.GetDirectoryName( outp ), "temp.%(ext)s" );
          ProcessStartInfo ytdl = new System.Diagnostics.ProcessStartInfo();
          ytdl.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
          ytdl.FileName = "ext\\youtube-dl.exe";

          ytdl.Arguments = "--extract-audio --audio-quality 0 --no-playlist --output \"" + tempOut + "\" \"" + url + "\"";
          Process.Start( ytdl ).WaitForExit();
          transform = true;
        }
        if ( transform )
        {
          string tempFIle = Directory.GetFiles( Path.GetDirectoryName( outp ), "temp.*", SearchOption.TopDirectoryOnly ).First();

          ProcessStartInfo ffmpeg = new System.Diagnostics.ProcessStartInfo();
          ffmpeg.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
          ffmpeg.FileName = "ext\\ffmpeg.exe";

          ffmpeg.Arguments = "-i " + tempFIle + " -f mp3 " + outp;
          Process.Start( ffmpeg ).WaitForExit();

          File.Delete( tempFIle );
        }
      }
      catch ( Exception ex )
      {
        throw new ArgumentException( ex.Message );
      }
    }

    private async void SendAudio( AudioService audio, Channel vChannel, Tag tag, int idx, IClient client )
    {
      string path = Path.Combine( "sounds", tag.Name, idx + ".mp3" );
      if ( !File.Exists( path ) )
        DownloadAudio( tag.Entries[idx], path, client );

      lock ( playing )
      {
        IAudioClient vClient = audio.Join( vChannel ).Result;
        client.Log( "reading " + tag.Name );
        var channelCount = client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
        var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
        using ( var MP3Reader = new Mp3FileReader( path ) ) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
        using ( var resampler = new MediaFoundationResampler( MP3Reader, OutFormat ) ) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
        {
          resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality

          int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
          byte[] buffer = new byte[blockSize];
          int byteCount;

          client.Log( "start sending audio: " + tag.Name );
          while ( ( byteCount = resampler.Read( buffer, 0, blockSize ) ) > 0 && !stop ) // Read audio into our buffer, and keep a loop open while data is present
          {
            if ( byteCount < blockSize )
            {
              // Incomplete Frame
              for ( int i = byteCount; i < blockSize; i++ )
                buffer[i] = 0;
            }
            vClient.Send( buffer, 0, blockSize ); // Send the buffer to Discord
          }
          client.Log( "finished sending" );
        }
        vClient.Wait();
        stop = false;
      }
    }
  }
}
