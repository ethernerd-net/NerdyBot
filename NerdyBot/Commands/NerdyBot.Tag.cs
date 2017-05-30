using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using NerdyBot.Commands.Config;
using NerdyBot.Contracts;


namespace NerdyBot.Commands
{
  public class TagCommand : ModuleBase
  {
    private AudioService svcAudio;
    private MessageService svcMessage;
    private CommandConfig<TagConfig> conf;
    
    private IEnumerable<string> KeyWords
    {
      get
      {
        return new string[] { "create", "delete", "edit", "list", "raw", "info", "help" };
      }
    }

    #region ICommand
    public ICommandConfig Config { get { return this.conf; } }

    public TagCommand( AudioService svcAudio, MessageService svcMessage )
    {
      this.conf = new CommandConfig<TagConfig>( "tag" );
      this.conf.Read();
      this.svcAudio = svcAudio;
      this.svcMessage = svcMessage;
    }

    [Command( "tag" ), Alias( "t" ), Summary( "Echos a message." )]
    public async Task Execute( params string[] args )
    {
      string response = "Invalid parameter count, check help for... guess what?";
      var subArgs = args.Skip( 1 ).ToArray();
      string option = args.First().ToLower();

      switch ( option )
      {
      case "create":
        if ( args.Count() >= 4 )
        {
          string tn = subArgs.First().ToLower();
          if ( IsValidName( tn ) )
          {
            if ( Create( subArgs, Context.User.Username ) )
              response = "Tag '" + tn + "' erstellt!";
            else
              response = "Fehler beim erstellen des tags (parameter oder code)";
          }
          else
            response = "Tag '" + tn + "' existiert bereits oder ist reserviert!!";
        }
        break;

      case "edit":
        if ( args.Count() >= 4 )
          Edit( subArgs, Context.User.Username, out response );
        break;

      case "list":
        List( Context.Channel );
        response = string.Empty;
        break;

      case "help":
        this.svcMessage.SendMessage( Context, FullHelp(),
          new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = Contracts.MessageType.Block } );
        break;

      case "delete":
      case "info":
      case "raw":
      default:
        string tagName = args.First().ToLower();
        if ( args.Count() > 1 )
          tagName = subArgs.First().ToLower();
        var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == tagName );
        if ( tag != null )
        {
          switch ( option )
          {
          case "delete":
            if ( args.Count() == 2 )
            {
              Delete( tag );
              response = "Tag '" + tag.Name + "' delete!";
            }
            break;
          case "info":
            if ( args.Count() >= 2 )
            {
              Info( tag, Context.Channel );
              response = string.Empty;
            }
            break;
          case "raw":
            if ( args.Count() >= 2 )
            {
              Raw( tag, Context.Channel );
              response = string.Empty;
            }
            break;
          default:
            if ( args.Count() == 1 )
            {
              Send( tag );
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
        this.svcMessage.SendMessage( Context, response,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = Contracts.MessageType.Info } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== TAG ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Tag Command erlaubt es Tags ( Sound | Text | URL ) zu erstellen, diese verhalten sich in etwa wie Textbausteine." );
      sb.AppendLine( "Ein Textbaustein kann mehrere Elemente des selben Typs enthalten, beim Aufruf des Tags wird dann zufällig ein Eintrag gewählt." );
      sb.AppendLine( "Key: tag" );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: t" );
      sb.AppendLine();
      sb.AppendLine();
      sb.AppendLine( "===> create" );
      sb.AppendLine( "tag create [tagname] [typ] {liste}" );
      sb.AppendLine( "tagname: Einzigartiger Name zum identifizieren des Bausteins" );
      sb.AppendLine( "typ: sound | text | url" );
      sb.AppendLine( "liste: leerzeichen getrennte liste an urls / texte sind getrennt durch ';;' (ohne '')" );
      sb.AppendLine();
      sb.AppendLine( "===> delete" );
      sb.AppendLine( "tag delete [tagname]" );
      sb.AppendLine( "Löscht einen Tag und dazugehörige Elemente" );
      sb.AppendLine();
      sb.AppendLine( "===> edit" );
      sb.AppendLine( "tag edit [tagname] [option] {}" );
      sb.AppendLine( "option: add | remove | rename" );
      sb.AppendLine( " -> add: Wie beim create kann hier eine Liste an URLs/Text angehängt werden um den Baustein zu erweitern" );
      sb.AppendLine( " -> remove: Entfernt den entsprechenden Text/Url aus der Inventar des Tags" );
      sb.AppendLine( " -> rename: Erlaubt das umbenennen des kompletten Tags" );
      sb.AppendLine();
      sb.AppendLine( "===> list" );
      sb.AppendLine( "tag list" );
      sb.AppendLine( "Listet alle vorhandenen Tags auf" );
      sb.AppendLine();
      sb.AppendLine( "===> stop" );
      sb.AppendLine( "tag stop" );
      sb.AppendLine( "Stopt das abspielen eines Sound Tags" );
      sb.AppendLine();
      sb.AppendLine( "===> help" );
      sb.AppendLine( "tag help" );
      sb.AppendLine( ">_>" );
      sb.AppendLine();
      sb.AppendLine();
      return sb.ToString();
    }
    #endregion ICommand

    private bool Create( string[] args, string author  )
    {
      bool response = true;
      try
      {
        string tagName = args.First().ToLower();
        this.svcMessage.Log( "creating tag '" + tagName + "'", author );
        Tag tag = new Tag();
        tag.Name = tagName;
        tag.Author = author;
        tag.CreateDate = DateTime.Now;
        tag.Count = 0;
        tag.Volume = 100;
        tag.Entries = new List<string>();

        switch ( args[1].ToLower() )
        {
        case "text":
          tag.Type = Commands.Config.TagType.Text;
          AddTextToTag( tag, args.Skip( 2 ).ToArray() );
          break;

        case "sound":
          tag.Type = Commands.Config.TagType.Sound;
          AddSoundToTag( tag, args.Skip( 2 ).ToArray() );
          break;

        case "url":
          tag.Type = Commands.Config.TagType.Url;
          AddUrlToTag( tag, args.Skip( 2 ).ToArray() );
          break;
        default:
          response = false;
          break;
        }
        this.conf.Ext.Tags.Add( tag );
        this.conf.Write();
        this.svcMessage.Log( "finished creation", author );
      }
      catch ( Exception ex )
      {
        this.svcMessage.Log( ex.Message, "Exception" );
        response = false;
      }
      return response;
    }
    private void Edit( string[] args, string author, out string response )
    {
      response = string.Empty;
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == args[0].ToLower() );
      if ( tag == null )
        response = "Tag '" + args[0] + "' existiert nicht!";
      else
      {
        if ( tag.Author == author )
        {
          string[] entries = args.Skip( 2 ).ToArray();

          switch ( args[1] )
          {
          case "add":
            switch ( tag.Type )
            {
            case Commands.Config.TagType.Text:
              AddTextToTag( tag, entries );
              break;
            case Commands.Config.TagType.Sound:
              AddSoundToTag( tag, entries );
              break;
            case Commands.Config.TagType.Url:
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
              if ( tag.Type != Commands.Config.TagType.Text )
              {
                string dirName = tag.Type == Commands.Config.TagType.Sound ? "sounds" : "pics";
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
            response = "Die Option Name '" + args[2] + "' ist nicht valide!";
            break;
          }
          this.conf.Write();
        }
        else
          response = "Du bist zu unwichtig dafür!";
      }
    }
    private void List( IMessageChannel channel )
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
          sb.Append( "(" + Enum.GetName( typeof( Commands.Config.TagType ), t.Type )[0] + "|" + t.Entries.Count() + ")" );
          sb.Append( ", " );
        }
        sb.Remove( sb.Length - 2, 2 );
      }
      this.svcMessage.SendMessage( Context, sb.ToString(),
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = channel.Id, MessageType = Contracts.MessageType.Block, Hightlight = "md" } );
    }
    private void Delete( Tag tag )
    {
      if ( tag.Type == Commands.Config.TagType.Sound )
        Directory.Delete( Path.Combine( "tag", tag.Name ), true );

      this.conf.Ext.Tags.Remove( tag );
      this.conf.Write();
    }
    private void Info( Tag tag, IMessageChannel channel )
    {
      StringBuilder sb = new StringBuilder( "==== " + tag.Name + " =====" );
      sb.AppendLine();
      sb.AppendLine();

      sb.Append( "Author: " );
      sb.AppendLine( tag.Author );

      sb.Append( "Typ: " );
      sb.AppendLine( Enum.GetName( typeof( Commands.Config.TagType ), tag.Type ) );

      sb.Append( "Erstellungs Datum: " );
      sb.AppendLine( tag.CreateDate.ToLongDateString() );

      sb.Append( "Hits: " );
      sb.AppendLine( tag.Count.ToString() );

      sb.Append( "Anzahl Einträge: " );
      sb.AppendLine( tag.Entries.Count.ToString() );

      this.svcMessage.SendMessage( Context, sb.ToString(),
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = channel.Id, MessageType = Contracts.MessageType.Block } );
    }
    private void Raw( Tag tag, IMessageChannel channel )
    {
      StringBuilder sb = new StringBuilder( "==== " + tag.Name + " ====" );
      sb.AppendLine();
      sb.AppendLine();

      foreach ( string entry in tag.Entries )
        sb.AppendLine( entry );

      this.svcMessage.SendMessage( Context, sb.ToString(),
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = channel.Id, MessageType = Contracts.MessageType.Block } );
    }
    private void Send( Tag tag )
    {
      int idx = ( new Random() ).Next( 0, tag.Entries.Count() );
      switch ( tag.Type )
      {
      case Commands.Config.TagType.Text:
        this.svcMessage.SendMessage( Context, tag.Entries[idx],
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = Contracts.MessageType.Block } );
        break;
      case Commands.Config.TagType.Sound:
        this.svcAudio.StopPlaying = false;
        string path = Path.Combine( "tag", tag.Name, idx + ".mp3" );
        if ( !File.Exists( path ) )
          this.svcAudio.DownloadAudio( tag.Entries[idx], path );
        this.svcAudio.SendAudio( Context, path, tag.Volume / 100f );
        break;
      case Commands.Config.TagType.Url:
        this.svcMessage.SendMessage( Context, tag.Entries[idx],
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
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
      string path = Path.Combine( "tag", tag.Name );
      Directory.CreateDirectory( path );
      int listCount = tag.Entries.Count;

      for ( int i = 0; i < args.Count(); i++ )
      {
        this.svcAudio.DownloadAudio( args[i], Path.Combine( path, ( listCount + i ) + ".mp3" ) );
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
      case Commands.Config.TagType.Text:
        string text = string.Empty;
        for ( int i = 0; i < args.Count(); i++ )
          text += " " + args[i];

        foreach ( string entry in text.Split( new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries ) )
          if ( tag.Entries.Remove( entry ) )
            remCount++;

        break;
      case Commands.Config.TagType.Sound:
      case Commands.Config.TagType.Url:
        for ( int i = 0; i < args.Count(); i++ )
        {
          int idx = tag.Entries.FindIndex( s => s == args[i] );
          if ( idx >= 0 )
          {
            tag.Entries.RemoveAt( idx );
            remCount++;
            if ( tag.Type == Commands.Config.TagType.Sound )
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
