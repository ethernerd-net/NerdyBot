using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;

using NerdyBot.Commands.Config;
using NerdyBot.Contracts;

namespace NerdyBot.Commands
{
  class TagCommand : ICommand
  {
    private CommandConfig<TagConfig> conf;
    private const string DEFAULTKEY = "tag";
    private static readonly IEnumerable<string> DEFAULTALIASES = new string[] { "t" };

    private IClient client;

    private IEnumerable<string> KeyWords
    {
      get
      {
        return new string[] { "create", "delete", "edit", "list", "raw", "info", "help" };
      }
    }

    #region ICommand
    public ICommandConfig Config { get { return this.conf; } }

    public void Init( IClient client)
    {
      this.conf = new CommandConfig<TagConfig>( DEFAULTKEY, DEFAULTALIASES );
      this.conf.Read();
      this.client = client;
    }

    public Task Execute( ICommandMessage msg )
    {
      return Task.Factory.StartNew( () =>
      {
        string response = "Invalid parameter count, check help for... guess what?";
        var subArgs = msg.Arguments.Skip( 1 ).ToArray();
        string option = msg.Arguments.First().ToLower();

        switch ( option )
        {
        case "create":
          if ( msg.Arguments.Count() >= 4 )
            response = Create( subArgs, msg.User.FullName );
          break;

        case "edit":
          if ( msg.Arguments.Count() >= 4 )
            response = Edit( subArgs, msg.User.FullName, msg.User.Permissions );
          break;

        case "list":
          List( msg.Channel );
          response = string.Empty;
          break;

        case "help":
          this.client.SendMessage( FullHelp(),
            new SendMessageOptions() { TargetType = TargetType.User, TargetId = msg.User.Id, MessageType = MessageType.Block } );
          break;

        case "delete":
        case "info":
        case "raw":
        default:
          string tagName = msg.Arguments.First().ToLower();
          if ( msg.Arguments.Count() > 1 )
            tagName = subArgs.First().ToLower();
          var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == tagName );
          if ( tag != null )
          {
            switch ( option )
            {
            case "delete":
              if ( msg.Arguments.Count() == 2 )
              {
                Delete( tag );
                response = "Tag '" + tag.Name + "' delete!";
              }
              break;
            case "info":
              if ( msg.Arguments.Count() >= 2 )
              {
                Info( tag, msg.Channel );
                response = string.Empty;
              }
              break;
            case "raw":
              if ( msg.Arguments.Count() >= 2 )
              {
                Raw( tag, msg.Channel );
                response = string.Empty;
              }
              break;
            default:
              if ( msg.Arguments.Count() == 1 )
              {
                Send( tag, msg );
                response = string.Empty;
              }
              break;
            }
          }
          else
            response = "Tag '" + tagName + "' existiert nicht!";
          break;
        }
        if ( response != string.Empty )
          this.client.SendMessage( response,
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );

      }, TaskCreationOptions.None );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== TAG ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Tag Command erlaubt es Tags ( Sound | Text | URL ) zu erstellen, diese verhalten sich in etwa wie Textbausteine." );
      sb.AppendLine( "Ein Textbaustein kann mehrere Elemente des selben Typs enthalten, beim Aufruf des Tags wird dann zufällig ein Eintrag gewählt." );
      sb.AppendLine( "Key: " + this.conf.Key );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine( "===> create" );
      sb.AppendLine( this.conf.Key + " create [tagname] [typ] {liste}" );
      sb.AppendLine( "tagname: Einzigartiger Name zum identifizieren des Bausteins" );
      sb.AppendLine( "typ: sound | text | url" );
      sb.AppendLine( "liste: leerzeichen getrennte liste an urls / texte sind getrennt durch ';;' (ohne '')" );
      sb.AppendLine();
      sb.AppendLine( "===> delete" );
      sb.AppendLine( this.conf.Key + " delete [tagname]" );
      sb.AppendLine( "Löscht einen Tag und dazugehörige Elemente" );
      sb.AppendLine();
      sb.AppendLine( "===> edit" );
      sb.AppendLine( this.conf.Key + " edit [tagname] [option] {}" );
      sb.AppendLine( "option: add | remove | rename" );
      sb.AppendLine( " -> add: Wie beim create kann hier eine Liste an URLs/Text angehängt werden um den Baustein zu erweitern" );
      sb.AppendLine( " -> remove: Entfernt den entsprechenden Text/Url aus der Inventar des Tags" );
      sb.AppendLine( " -> rename: Erlaubt das umbenennen des kompletten Tags" );
      sb.AppendLine();
      sb.AppendLine( "===> list" );
      sb.AppendLine( this.conf.Key + " list" );
      sb.AppendLine( "Listet alle vorhandenen Tags auf" );
      sb.AppendLine();
      sb.AppendLine( "===> stop" );
      sb.AppendLine( this.conf.Key + " stop" );
      sb.AppendLine( "Stopt das abspielen eines Sound Tags" );
      sb.AppendLine();
      sb.AppendLine( "===> help" );
      sb.AppendLine( this.conf.Key + " help" );
      sb.AppendLine( ">_>" );
      sb.AppendLine();
      sb.AppendLine();
      return sb.ToString();
    }
    #endregion ICommand

    private string Create( string[] args, string author  )
    {
      string response = string.Empty;
      if ( !IsValidName( args[0].ToLower() ) )
        response = "Tag '" + args[0] + "' existiert bereits oder ist reserviert!!";
      else
      {
        Tag tag = new Tag();
        tag.Name = args[0].ToLower();
        tag.Author = author;
        tag.CreateDate = DateTime.Now;
        tag.Count = 0;
        tag.Volume = 100;
        tag.Entries = new List<string>();

        switch ( args[1].ToLower() )
        {
        case "text":
          tag.Type = TagType.Text;
          AddTextToTag( tag, args.Skip( 2 ).ToArray() );
          break;

        case "sound":
          tag.Type = TagType.Sound;
          AddSoundToTag( tag, args.Skip( 2 ).ToArray() );
          break;

        case "url":
          tag.Type = TagType.Url;
          AddUrlToTag( tag, args.Skip( 2 ).ToArray() );
          break;
        default:
          return args[1] + " ist ein invalider Parameter";
        }
        this.conf.Ext.Tags.Add( tag );
        this.conf.Write();
        response = "Tag '" + tag.Name + "' erstellt!";
      }
      return response;
    }
    private string Edit( string[] args, string author, ServerPermissions permissions )
    {
      string response = string.Empty;
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == args[0].ToLower() );
      if ( tag == null )
        response = "Tag '" + args[0] + "' existiert nicht!";
      else
      {
        if ( tag.Author == author || permissions.Administrator )
        {
          string[] entries = args.Skip( 2 ).ToArray();

          switch ( args[1] )
          {
          case "add":
            switch ( tag.Type )
            {
            case TagType.Text:
              AddTextToTag( tag, entries );
              break;
            case TagType.Sound:
              AddSoundToTag( tag, entries );
              break;
            case TagType.Url:
              AddUrlToTag( tag, entries );
              break;
            default:
              throw new ArgumentException( "WTF?!?!" );
            }
            response = entries.Count() + " Einträge zu '" + tag.Name + " hinzugefügt'!";
            break;
          case "remove":
            int remCount = RemoveTagEntry( tag, entries );
            response = remCount + " / " + entries.Count() + " removed";
            break;
          case "rename":
            if ( IsValidName( entries[0].ToLower() ) )
            {
              if ( tag.Type != TagType.Text )
              {
                string dirName = tag.Type == TagType.Sound ? "sounds" : "pics";
                Directory.Move( Path.Combine( dirName, tag.Name ), Path.Combine( dirName, entries[0] ) );
              }
              tag.Name = entries[0];
              response = "Tag umbenannt in '" + tag.Name + "'!";
            }
            else
              response = "Tag '" + entries[0] + "' existiert bereits oder ist reserviert!!";
            break;
          case "volume":
            short vol;
            if ( short.TryParse( entries[0], out vol ) && vol > 0 && vol <= 100 )
              tag.Volume = vol;
            else
              response = "Die Lautstärke muss eine Zahl zwischen 0 und 101 sein!";
            break;
          default:
            return "Die Option Name '" + args[2] + "' ist nicht valide!";
          }
          this.conf.Write();
        }
        else
          response = "Du bist zu unwichtig dafür!";
      }
      return response;
    }
    private void List( ICommandChannel channel )
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
          sb.Append( "(" + Enum.GetName( typeof( TagType ), t.Type )[0] + "|" + t.Entries.Count() + ")" );
          sb.Append( ", " );
        }
        sb.Remove( sb.Length - 2, 2 );
      }
      this.client.SendMessage( sb.ToString(),
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = channel.Id, MessageType = MessageType.Block, Hightlight = "md" } );
    }
    private void Delete( Tag tag )
    {
      if ( tag.Type == TagType.Sound )
        Directory.Delete( Path.Combine( "tag", tag.Name ), true );

      this.conf.Ext.Tags.Remove( tag );
      this.conf.Write();
    }
    private void Info( Tag tag, ICommandChannel channel )
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

      this.client.SendMessage( sb.ToString(),
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = channel.Id, MessageType = MessageType.Block } );
    }
    private void Raw( Tag tag, ICommandChannel channel )
    {
      StringBuilder sb = new StringBuilder( "==== " + tag.Name + " ====" );
      sb.AppendLine();
      sb.AppendLine();

      foreach ( string entry in tag.Entries )
        sb.AppendLine( entry );

      this.client.SendMessage( sb.ToString(),
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = channel.Id, MessageType = MessageType.Block } );
    }
    private void Send( Tag tag, ICommandMessage msg )
    {
      int idx = ( new Random() ).Next( 0, tag.Entries.Count() );
      switch ( tag.Type )
      {
      case TagType.Text:
        this.client.SendMessage( tag.Entries[idx],
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Block } );
        break;
      case TagType.Sound:
        if ( msg.User.VoiceChannelId != 0 )
        {
          this.client.StopPlaying = false;
          string path = Path.Combine( this.conf.Key, tag.Name, idx + ".mp3" );
          if ( !File.Exists( path ) )
            this.client.DownloadAudio( tag.Entries[idx], path );
          this.client.SendAudio( msg.User.VoiceChannelId, path, tag.Volume / 100f );
        }
        break;
      case TagType.Url:
        this.client.SendMessage( tag.Entries[idx],
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
        break;
      default:
        throw new ArgumentException( "WTF?!" );
      }
      tag.Count++;
      this.conf.Write();
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
    private void AddSoundToTag( Tag tag, string[] args )
    {
      string path = Path.Combine( "sounds", tag.Name );
      Directory.CreateDirectory( path );
      int listCount = tag.Entries.Count;

      for ( int i = 0; i < args.Count(); i++ )
      {
        this.client.DownloadAudio( args[i], Path.Combine( path, ( listCount + i ) + ".mp3" ) );
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
  }
}
