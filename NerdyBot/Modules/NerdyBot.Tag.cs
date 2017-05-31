using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord.Commands;

using NerdyBot.Services;
using NerdyBot.Models;

namespace NerdyBot.Commands
{
  [Group( "tag" ), Alias( "t" )]
  public class TagCommand : ModuleBase
  {
    public AudioService AudioService { get; set; }
    public MessageService MessageService { get; set; }
    public DatabaseService DatabaseService { get; set; }

    private IEnumerable<string> KeyWords
    {
      get
      {
        return new string[] { "create", "delete", "edit", "list", "raw", "info", "help" };
      }
    }

    public TagCommand( DatabaseService databaseService )
    {
      databaseService.Database.CreateTable<Tag>();
      databaseService.Database.CreateTable<TagEntry>();
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

        DatabaseService.Database.Insert( tag );

        switch ( tagType )
        {
        case TagType.Url:
        case TagType.Text:
          AddTextToTag( tag, content );
          break;

        case TagType.Sound:
          try
          {
            AddSoundToTag( tag, content );
          }
          catch ( Exception ex )
          {
            MessageService.Log( ex.Message, "Exception" );
            MessageService.SendMessage( Context, $"Tag '{tagName}' konnte nicht erstellt werden!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
            DatabaseService.Database.Delete( tag );
          }
          break;
        default:
          throw new ArgumentException( "WTF?!?!" );
        }
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
      var tag = DatabaseService.Database.Table<Tag>().FirstOrDefault( t => t.Name == tagName.ToLower() );
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
            bool success = true;
            switch ( tag.Type )
            {
            case TagType.Url:
            case TagType.Text:
              AddTextToTag( tag, content );
              break;
            case TagType.Sound:
              try
              {
                AddSoundToTag( tag, content );
              }
              catch ( Exception ex )
              {
                success = false;
                MessageService.Log( ex.Message, "Exception" );
                MessageService.SendMessage( Context, $"Einige Einträge konnten nicht hinzugefügt werden!",
                  new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
              }
              break;
            default:
              throw new ArgumentException( "WTF?!?!" );
            }
            if ( success )
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
          DatabaseService.Database.Update( tag );
        }
        else
          MessageService.SendMessage( Context, $"Du bist zu unwichtig für diese Aktion",
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      }
    }

    [Command( "list" ), Priority(10)]
    public async Task List()
    {
      var tagsInOrder = DatabaseService.Database.Table<Tag>().OrderBy( x => x.Name );
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
          sb.Append( $"[{t.Name}]" );
          sb.Append( $"({Enum.GetName( typeof( TagType ), t.Type )[0]}|{DatabaseService.Database.Table<TagEntry>().Count( te => te.TagId == t.Id)})" );
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
      var tag = DatabaseService.Database.Table<Tag>().FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        if ( DatabaseService.Database.Delete<Tag>( tag.Id ) > 0 )
          MessageService.SendMessage( Context, $"Tag '{tagName}' erfolgreich gelöscht!",
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
        else
          MessageService.SendMessage( Context, $"Fehler beim löschen (schade)!",
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      }
    }

    [Command( "info" )]
    public async Task Info( string tagName )
    {
      var tag = DatabaseService.Database.Table<Tag>().FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        StringBuilder sb = new StringBuilder( $"==== {tag.Name} =====" );
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
        sb.AppendLine( DatabaseService.Database.Table<TagEntry>().Count( te => te.TagId == tag.Id ).ToString() );

        MessageService.SendMessage( Context, sb.ToString(),
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Block } );
      }
    }

    [Command( "raw" )]
    public async Task Raw( string tagName )
    {
      var tag = DatabaseService.Database.Table<Tag>().FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        StringBuilder sb = new StringBuilder( $"==== {tag.Name} ====" );
        sb.AppendLine();
        sb.AppendLine();

        foreach ( var entry in DatabaseService.Database.Table<TagEntry>() )
          sb.AppendLine( entry.TextContent );

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
      var tag = DatabaseService.Database.Table<Tag>().FirstOrDefault( t => t.Name == tagName.ToLower() );
      if ( tag == null )
        MessageService.SendMessage( Context, $"Tag '{tagName}' existiert nicht!",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      else
      {
        var tagEntries = DatabaseService.Database.Table<TagEntry>().Where( te => te.TagId == tag.Id );
        int idx = ( new Random() ).Next( 0, tagEntries.Count() );
        switch ( tag.Type )
        {
        case TagType.Text:
          MessageService.SendMessage( Context, tagEntries.ElementAt( idx ).TextContent,
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Block } );
          break;
        case TagType.Sound:
          AudioService.StopPlaying = false;
          AudioService.SendAudio( Context, tagEntries.ElementAt( idx ).ByteContent, tag.Volume / 100f );
          break;
        case TagType.Url:
          MessageService.SendMessage( Context, tagEntries.ElementAt( idx ).TextContent,
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
          break;
        default:
          throw new ArgumentException( "WTF?!" );
        }
        tag.Count++;
        DatabaseService.Database.Update( tag );
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
      return !( DatabaseService.Database.Table<Tag>().Any( t => t.Name == name ) || KeyWords.Contains( name ) );
    }
    private void AddTextToTag( Tag tag, string[] entries )
    {
      foreach ( string entry in entries )
        DatabaseService.Database.Insert( new TagEntry() { TagId = tag.Id, TextContent = entry } );
    }
    private async void AddSoundToTag( Tag tag, string[] entries )
    {
      string path = Path.Combine( "tag", tag.Name );
      Directory.CreateDirectory( path );

      foreach ( string entry in entries )
      {
        DatabaseService.Database.Insert( new TagEntry()
        {
          TagId = tag.Id,
          TextContent = entry,
          ByteContent = await AudioService.DownloadAudio( entry )
        } );
      }
    }
    private int RemoveTagEntry( Tag tag, string[] entries )
    {
      int remCount = 0;

      foreach ( string entry in entries )
      {
        var primkey = DatabaseService.Database.Table<TagEntry>().Where( te => te.TagId == tag.Id && te.TextContent == entry ).First().Id;
        remCount += DatabaseService.Database.Delete<TagEntry>( primkey );
      }

      return remCount;
    }
  }
}
