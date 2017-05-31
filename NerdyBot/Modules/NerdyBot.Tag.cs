using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Discord.Commands;

using NerdyBot.Config;
using NerdyBot.Services;

namespace NerdyBot.Commands
{
  [Group( "tag" ), Alias( "t" )]
  public class TagCommand : ModuleBase
  {
    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }
    private ModuleConfig<TagConfig> conf;

    private IEnumerable<string> KeyWords
    {
      get
      {
        return new string[] { "create", "delete", "edit", "list", "raw", "info", "help" };
      }
    }

    public TagCommand()
    {
      this.conf = new ModuleConfig<TagConfig>( "tag" );
      this.conf.Read();
    }

    [Command( "create" )]
    public async Task Create( string tagName, TagType tagType, params string[] content )
    {
      string author = Context.User.ToString();
      try
      {
        MessageService.Log( $"creating tag '{tagName.ToLower()}'", author );
        Tag tag = new Tag();
        tag.Name = tagName;
        tag.Author = author;
        tag.Type = tagType;
        tag.CreateDate = DateTime.Now;
        tag.Count = 0;
        tag.Volume = 100;
        tag.Entries = new List<string>();

        switch ( tagType )
        {
        case TagType.Text:
          AddTextToTag( tag, content );
          break;

        case TagType.Sound:
          AddSoundToTag( tag, content );
          break;

        case TagType.Url:
          AddUrlToTag( tag, content );
          break;
        default:
          throw new ArgumentException( "WTF?!?!" );
        }
        this.conf.Ext.Tags.Add( tag );
        this.conf.Write();
        MessageService.Log( "finished creation", author );
        MessageService.SendMessage( Context, $"Tag '{tagName}' erfolgreich erstellt!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      }
      catch ( Exception ex )
      {
        MessageService.Log( ex.Message, "Exception" );
      }
    }

    [Command( "edit" )]
    public async Task Edit( string tagName, string editType, params string[] content )
    {
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        if ( tag.Author == Context.User.ToString() )
        {
          switch ( editType )
          {
          case "add":
            switch ( tag.Type )
            {
            case TagType.Text:
              AddTextToTag( tag, content );
              break;
            case TagType.Sound:
              AddSoundToTag( tag, content );
              break;
            case TagType.Url:
              AddUrlToTag( tag, content );
              break;
            default:
              throw new ArgumentException( "WTF?!?!" );
            }
            MessageService.SendMessage( Context, $"{content.Count()} Einträge zu '{tag.Name} hinzugefügt'!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
            break;
          case "remove":
            int remCount = RemoveTagEntry( tag, content );
            MessageService.SendMessage( Context, $"{remCount} / {content.Count()} removed",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
            break;
          case "rename":
            string newTagName = content[0].ToLower();
            if ( IsValidName( newTagName ) )
            {
              if ( tag.Type != Config.TagType.Text )
              {
                string dirName = tag.Type == Config.TagType.Sound ? "sounds" : "pics";
                Directory.Move( Path.Combine( dirName, tag.Name ), Path.Combine( dirName, newTagName ) );
              }
              tag.Name = newTagName;
              MessageService.SendMessage( Context, $"Tag umbenannt in '{tag.Name}'!",
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
            }
            else
              MessageService.SendMessage( Context, $"Tag '{newTagName}' existiert bereits oder ist reserviert!!",
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
            break;
          case "volume":
            short vol;
            if ( short.TryParse( content[0], out vol ) && vol > 0 && vol <= 100 )
              tag.Volume = vol;
            else
              MessageService.SendMessage( Context, $"Die Lautstärke muss eine Zahl zwischen 0 und 101 sein!",
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
            break;
          default:
            MessageService.SendMessage( Context, $"Die Option Name '{editType}' ist nicht valide!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
            break;
          }
          this.conf.Write();
        }
        else
          MessageService.SendMessage( Context, $"Du bist zu unwichtig für diese Aktion",
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      }
    }

    [Command( "list" ), Priority(10)]
    public async Task List()
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
      MessageService.SendMessage( Context, sb.ToString(),
        new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Block, Hightlight = "md", Split = true } );
    }

    [Command( "delete" )]
    public async Task Delete( string tagName )
    {
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        if ( tag.Type == Config.TagType.Sound )
          Directory.Delete( Path.Combine( "tag", tag.Name ), true );

        this.conf.Ext.Tags.Remove( tag );
        this.conf.Write();
        MessageService.SendMessage( Context, $"Tag '{tagName}' erfolgreich gelöscht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      }
    }

    [Command( "info" )]
    public async Task Info( string tagName )
    {
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        StringBuilder sb = new StringBuilder( "==== " + tag.Name + " =====" );
        sb.AppendLine();
        sb.AppendLine();

        sb.Append( "Author: " );
        sb.AppendLine( tag.Author );

        sb.Append( "Typ: " );
        sb.AppendLine( Enum.GetName( typeof( Config.TagType ), tag.Type ) );

        sb.Append( "Erstellungs Datum: " );
        sb.AppendLine( tag.CreateDate.ToLongDateString() );

        sb.Append( "Hits: " );
        sb.AppendLine( tag.Count.ToString() );

        sb.Append( "Anzahl Einträge: " );
        sb.AppendLine( tag.Entries.Count.ToString() );

        MessageService.SendMessage( Context, sb.ToString(),
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Block } );
      }
    }

    [Command( "raw" )]
    public async Task Raw( string tagName )
    {
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        StringBuilder sb = new StringBuilder( $"==== {tag.Name} ====" );
        sb.AppendLine();
        sb.AppendLine();

        foreach ( string entry in tag.Entries )
          sb.AppendLine( entry );

        MessageService.SendMessage( Context, sb.ToString(),
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Block } );
      }
    }

    [Command( "help" )]
    public async Task Help()
    {
      MessageService.SendMessage( Context, FullHelp(),
        new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
    }

    [Command()]
    public async Task Send( string tagName )
    {
      var tag = this.conf.Ext.Tags.FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        int idx = ( new Random() ).Next( 0, tag.Entries.Count() );
        switch ( tag.Type )
        {
        case TagType.Text:
          MessageService.SendMessage( Context, tag.Entries[idx],
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Block } );
          break;
        case TagType.Sound:
          AudioService.StopPlaying = false;
          string path = Path.Combine( "tag", tag.Name, idx + ".mp3" );
          if ( !File.Exists( path ) )
            await AudioService.DownloadAudio( tag.Entries[idx], path );
          AudioService.SendAudio( Context, path, tag.Volume / 100f );
          break;
        case TagType.Url:
          MessageService.SendMessage( Context, tag.Entries[idx],
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
          break;
        default:
          throw new ArgumentException( "WTF?!" );
        }
        tag.Count++;
        this.conf.Write();
      }
    }

    public static string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== TAG ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Tag Command erlaubt es Tags ( Sound | Text | URL ) zu erstellen, diese verhalten sich in etwa wie Textbausteine." );
      sb.AppendLine( "Ein Textbaustein kann mehrere Elemente des selben Typs enthalten, beim Aufruf des Tags wird dann zufällig ein Eintrag gewählt." );
      sb.AppendLine( "Key: tag" );
      return sb.ToString();
    }
    public static string FullHelp()
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
        AudioService.DownloadAudio( args[i], Path.Combine( path, ( listCount + i ) + ".mp3" ) );
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
