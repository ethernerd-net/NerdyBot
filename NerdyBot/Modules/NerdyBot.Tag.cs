using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord.Commands;

using NerdyBot.Services;
using NerdyBot.Models;

namespace NerdyBot.Modules
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
    public void Create( string tagName, TagType tagType, params string[] content )
    {
      Task.Run( async () =>
      {
        if ( !IsValidName( tagName, Context.Guild.Id ) )
          await MessageService.SendMessageToCurrentChannel( Context, $"Tag '{tagName}' bereits in Verwendung!", MessageType.Info );
        else
        {
          Tag tag = new Tag()
          {
            Name = tagName,
            Author = Context.User.ToString(),
            Type = tagType,
            CreateDate = DateTime.Now,
            Count = 0,
            Volume = 100,
            GuildId = ( long )Context.Guild.Id
          };

          DatabaseService.Database.Insert( tag );

          switch ( tagType )
          {
          case TagType.Url:
          case TagType.Text:
            AddTextToTag( tag, content );
            break;

          case TagType.Sound:
            int amountCreated = await AddSoundToTag( tag, content );
            if ( amountCreated != content.Count() )
            {
              await MessageService.SendMessageToCurrentChannel( Context, $"Tag '{tagName}' konnte nur {amountCreated} von {content.Count()} Einträgen anlegen!", MessageType.Info );
              if ( amountCreated == 0 )
              {
                DatabaseService.Database.Delete( tag );
                throw new InvalidDataException( "Erstellung abgebrochen!" );
              }
            }
            break;

          default:
            throw new ArgumentException( "WTF?!?!" );
          }
          await MessageService.SendMessageToCurrentChannel( Context, $"Tag '{tagName}' erfolgreich erstellt!", MessageType.Info );
        }
      } );
    }

    [Command( "edit" )]
    public void Edit( string tagName, EditType editType, params string[] content )
    {
      Task.Run( async () =>
      {
        var tag = GetTag( tagName, Context.Guild.Id );
        if ( tag.Author != Context.User.ToString() && !( await Context.Guild.GetUserAsync( Context.User.Id ) ).GuildPermissions.Administrator )
          await MessageService.SendMessageToCurrentChannel( Context, $"Du bist zu unwichtig für diese Aktion", MessageType.Info );
        else
        {
          switch ( editType )
          {
          case EditType.Add:
            int count = 0;
            switch ( tag.Type )
            {
            case TagType.Url:
            case TagType.Text:
              AddTextToTag( tag, content );
              count = content.Count();
              break;
            case TagType.Sound:
              count = await AddSoundToTag( tag, content );
              break;
            default:
              throw new ArgumentException( "WTF?!?!" );
            }
            await MessageService.SendMessageToCurrentChannel( Context, $"{count} Einträge zu '{tag.Name} hinzugefügt'!", MessageType.Info );
            break;

          case EditType.Remove:
            int remCount = RemoveTagEntry( tag, content );
            await MessageService.SendMessageToCurrentChannel( Context, $"{remCount} / {content.Count()} removed", MessageType.Info );
            break;

          case EditType.Rename:
            string newTagName = content[0].ToLower();
            if ( IsValidName( newTagName, Context.Guild.Id ) )
            {
              tag.Name = newTagName;
              await MessageService.SendMessageToCurrentChannel( Context, $"Tag umbenannt in '{tag.Name}'!", MessageType.Info );
            }
            else
              await MessageService.SendMessageToCurrentChannel( Context, $"Tag '{newTagName}' existiert bereits oder ist reserviert!!", MessageType.Info );
            break;

          case EditType.Volume:
            short vol;
            if ( short.TryParse( content[0], out vol ) && vol > 0 && vol <= 100 )
              tag.Volume = vol;
            else
              await MessageService.SendMessageToCurrentChannel( Context, $"Die Lautstärke muss eine Zahl zwischen 0 und 101 sein!", MessageType.Info );
            break;
          default:
            await MessageService.SendMessageToCurrentChannel( Context, $"Die Option Name '{editType}' ist nicht valide!", MessageType.Info );
            break;
          }
          DatabaseService.Database.Update( tag );
        }
      } );
    }

    [Command( "list" ), Priority( 10 )]
    public async Task List()
    {
      long gid = ( long )Context.Guild.Id;
      var tagsInOrder = DatabaseService.Database.Table<Tag>().Where( t => t.GuildId == gid ).OrderBy( x => x.Name );
      StringBuilder sb = new StringBuilder( "" );

      if ( tagsInOrder.Any() )
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
      await MessageService.SendMessageToCurrentChannel( Context, sb.ToString(), MessageType.Block, true, "md" );
    }

    [Command( "delete" )]
    public async Task Delete( string tagName )
    {
      var tag = GetTag( tagName, Context.Guild.Id );
      if ( DatabaseService.Database.Delete<Tag>( tag.Id ) > 0 )
        await MessageService.SendMessageToCurrentChannel( Context, $"Tag '{tagName}' erfolgreich gelöscht!", MessageType.Info );
      else
        await MessageService.SendMessageToCurrentChannel( Context, $"Fehler beim löschen (schade)!", MessageType.Info );
    }

    [Command( "info" )]
    public async Task Info( string tagName )
    {
      var tag = GetTag( tagName, Context.Guild.Id );
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

      await MessageService.SendMessageToCurrentChannel( Context, sb.ToString(), MessageType.Block );
    }

    [Command( "raw" )]
    public async Task Raw( string tagName )
    {
      var tag = GetTag( tagName, Context.Guild.Id );
      StringBuilder sb = new StringBuilder( $"==== {tag.Name} ====" );
      sb.AppendLine();
      sb.AppendLine();

      foreach ( var entry in DatabaseService.Database.Table<TagEntry>() )
        sb.AppendLine( entry.TextContent );

      await MessageService.SendMessageToCurrentChannel( Context, sb.ToString(), MessageType.Block );
    }

    [Command( "help" )]
    public async Task Help()
    {
      await MessageService.SendMessageToCurrentUser( Context, FullHelp(), MessageType.Block );
    }

    [Command()]
    public void Send( string tagName )
    {
      Task.Run( async () =>
      {
        var tag = GetTag( tagName, Context.Guild.Id );
        var tagEntries = DatabaseService.Database.Table<TagEntry>().Where( te => te.TagId == tag.Id );

        int idx = ( new Random() ).Next( 0, tagEntries.Count() );
        switch ( tag.Type )
        {
        case TagType.Text:
          await MessageService.SendMessageToCurrentChannel( Context, tagEntries.ElementAt( idx ).TextContent, MessageType.Info );
          break;
        case TagType.Sound:
          await AudioService.SendAudio( Context, tagEntries.ElementAt( idx ).ByteContent, tag.Volume / 100f );
          break;
        case TagType.Url:
          await MessageService.SendMessageToCurrentChannel( Context, tagEntries.ElementAt( idx ).TextContent );
          break;
        default:
          throw new ArgumentException( "WTF?!" );
        }
        tag.Count++;
        DatabaseService.Database.Update( tag );
      } );
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

    private bool IsValidName( string name, ulong guildId )
    {
      return !( DatabaseService.Database.Table<Tag>().Any( t => t.Name == name && t.GuildId == ( long )guildId ) || KeyWords.Contains( name ) );
    }
    private Tag GetTag( string name, ulong guildId )
    {
      var tag = DatabaseService.Database.Table<Tag>().FirstOrDefault( t => t.Name == name.ToLower() && t.GuildId == ( long )guildId );
      if ( tag == null )
        throw new ApplicationException( $"Tag '{name}' existiert nicht!" );
      return tag;
    }
    private void AddTextToTag( Tag tag, string[] entries )
    {
      foreach ( string entry in entries )
        DatabaseService.Database.Insert( new TagEntry() { TagId = tag.Id, TextContent = entry } );
    }
    private async Task<int> AddSoundToTag( Tag tag, string[] entries )
    {
      int count = 0;
      string path = Path.Combine( "tag", tag.Name );
      Directory.CreateDirectory( path );

      foreach ( string entry in entries )
      {
        try
        {
          DatabaseService.Database.Insert( new TagEntry()
          {
            TagId = tag.Id,
            TextContent = entry,
            ByteContent = await AudioService.DownloadAudio( entry )
          } );
          count++;
        }
        catch ( Exception ex )
        {
          await MessageService.Log( ex.Message, "Exception" );
        }
      }
      return count;
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

    public enum EditType { Add, Remove, Rename, Volume }
  }
}
