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
    private CommandConfig<TagConfig> conf;
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
    public BaseCommandConfig Config { get { return this.conf; } }

    public void Init()
    {
      this.conf = new CommandConfig<TagConfig>( DEFAULTKEY, DEFAULTALIASES );
      this.conf.Read();
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

        case "help":
          WriteHelp( msg.User, client.Config.Prefix );
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

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== TAG ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Tag Command erlaubt es Tags ( Sound | Text | URL ) zu erstellen, diese verhalten sich in etwa wie Textbausteine." );
      sb.AppendLine( "Ein Textbaustein kann mehrere Elemente des selben Typs enthalten, beim Aufruf des Tags wird dann zufällig ein Eintrag gewählt." );
      return sb.ToString();
    }
    #endregion ICommand

    private void WriteHelp( User usr, char prefix )
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine( "===> create" );
      sb.AppendLine( prefix + this.conf.Key + " create [tagname] [typ] {liste}" );
      sb.AppendLine( "tagname: Einzigartiger Name zum identifizieren des Bausteins" );
      sb.AppendLine( "typ: sound | text | url" );
      sb.AppendLine( "liste: leerzeichen getrennte liste an urls / texte sind getrennt durch ';;' (ohne '')" );
      sb.AppendLine();
      sb.AppendLine( "===> delete" );
      sb.AppendLine( prefix + this.conf.Key + " delete [tagname]" );
      sb.AppendLine( "Löscht einen Tag und dazugehörige Elemente" );
      sb.AppendLine();
      sb.AppendLine( "===> edit" );
      sb.AppendLine( prefix + this.conf.Key + " edit [tagname] [option] {}" );
      sb.AppendLine( "option: add | remove | rename" );
      sb.AppendLine( " -> add: Wie beim create kann hier eine Liste an URLs/Text angehängt werden um den Baustein zu erweitern" );
      sb.AppendLine( " -> remove: Entfernt den entsprechenden Text/Url aus der Inventar des Tags" );
      sb.AppendLine( " -> rename: Erlaubt das umbenennen des kompletten Tags" );
      sb.AppendLine();
      sb.AppendLine( "===> list" );
      sb.AppendLine( prefix + this.conf.Key + " list" );
      sb.AppendLine( "Listet alle vorhandenen Tags auf" );
      sb.AppendLine();
      sb.AppendLine( "===> stop" );
      sb.AppendLine( prefix + this.conf.Key + " stop" );
      sb.AppendLine( "Stopt das abspielen eines Sound Tags" );
      sb.AppendLine();
      sb.AppendLine( "===> help" );
      sb.AppendLine( prefix + this.conf.Key + " help" );
      sb.AppendLine( ">_>" );
      sb.AppendLine();
      sb.AppendLine();
      usr.SendMessage( "```" + sb.ToString() + "```" );
    }

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
      if ( !IsValidName( args[1].ToLower() ) )
        client.WriteInfo( "Tag '" + args[1] + "' existiert bereits oder ist reserviert!!", msg.Channel );
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
        this.conf.Ext.Tags.Add( tag );
        this.conf.Write();
        client.WriteInfo( "Tag '" + tag.Name + "' erstellt!", msg.Channel );
      }
    }

    private async void Delete( MessageEventArgs msg, string[] args, IClient client )
    {
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        client.WriteInfo( "Tag '" + args[1] + "' existiert nicht!", msg.Channel );
      else
      {
        if ( tag.Type == TagType.Sound )
          Directory.Delete( Path.Combine( "sounds", tag.Name ), true );

        this.conf.Ext.Tags.Remove( tag );
        this.conf.Write();
        client.WriteInfo( "Tag '" + tag.Name + "' delete!", msg.Channel );
      }
    }

    private async void Edit( MessageEventArgs msg, string[] args, IClient client )
    {
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        client.WriteInfo( "Tag '" + args[1] + "' existiert nicht!", msg.Channel );
      else
      {
        if ( tag.Author == msg.User.ToString() || msg.User.ServerPermissions.Administrator )
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
            client.WriteInfo( entries.Count() + " Einträge zu '" + tag.Name + " hinzugefügt'!", msg.Channel );
            break;
          case "remove":
            int remCount = RemoveTagEntry( tag, entries );
            client.WriteInfo( remCount + " / " + entries.Count() + " removed", msg.Channel );
            break;
          case "rename":
            if ( !IsValidName( entries[0].ToLower() ) )
            {
              if ( tag.Type != TagType.Text )
              {
                string dirName = tag.Type == TagType.Sound ? "sounds" : "pics";
                Directory.Move( Path.Combine( dirName, tag.Name ), Path.Combine( dirName, entries[0] ) );
              }
              tag.Name = entries[0];
              client.WriteInfo( "Tag umbenannt in '" + tag.Name + "'!", msg.Channel );
            }
            else
              client.WriteInfo( "Tag '" + args[1] + "' existiert bereits oder ist reserviert!!", msg.Channel );
            break;
          case "volume":
            break;
          default:
            client.WriteInfo( "Die Option Name '" + args[2] + "' ist nicht valide!", msg.Channel );
            return;
          }
          this.conf.Write();
        }
        else
          client.WriteInfo( "Du bist zu unwichtig dafür!", msg.Channel );
      }
    }

    private async void List( MessageEventArgs msg, IClient client )
    {
      var tagsInOrder = this.conf.Ext.Tags.OrderBy( x => x.Name );
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
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == args[1].ToLower() );
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
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == args[1].ToLower() );
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
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == args[0].ToLower() );
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
        this.conf.Write();
      }
    }

    private bool IsValidName( string name )
    {
      return !( this.conf.Ext.Tags.Exists( t => t.Name == name ) || KeyWords.Contains( name ) );
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
