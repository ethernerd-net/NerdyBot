using Discord.Commands;
using NerdyBot.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  public class UrbanCommand : ModuleBase
  {
    private AudioService svcAudio;
    private MessageService svcMessage;

    #region ICommand

    public void Init( AudioService svcAudio, MessageService svcMessage )
    {
      this.svcAudio = svcAudio;
      this.svcMessage = svcMessage;
    }

    [Command( "urban" ), Alias( "u" )]
    public Task Execute( params string[] args )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( args.Length == 1 && args[0] == "help" )
          this.svcMessage.SendMessage( Context, FullHelp(),
            new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
        else if ( args.Length > 1 && args[0] == "sound" )
        {
          string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", args.Skip( 1 ) ) );
          var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
          if ( urban.sounds != null && urban.sounds.Count() > 0 )
          {
            string soundUrl = urban.sounds.First();
            if ( soundUrl.EndsWith( ".mp3" ) )
            {
              this.svcAudio.StopPlaying = false;
              string path = Path.Combine( "urban", string.Join( " ", args.Skip( 1 ) ) + ".mp3" );
              if ( !File.Exists( path ) )
                this.svcAudio.DownloadAudio( urban.sounds.First(), path );
              this.svcAudio.SendAudio( Context, path );
            }
            else
              this.svcMessage.SendMessage( Context, "404, Urban not found!",
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
          }
          else
            this.svcMessage.SendMessage( Context, "404, Urban not found!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
        }
        else
        {
          string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", args ) );
          var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
          if ( urban.list != null && urban.list.Count() > 0 )
            this.svcMessage.SendMessage( Context, urban.list.First().permalink.Replace( "\\", "" ),
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
          else
            this.svcMessage.SendMessage( Context, "404, Urban not found!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
        }
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== Urban ========" );
      sb.AppendLine();
      sb.AppendLine( "Sucht auf Urban Dictonary nach dem eingegebenen Keyword." );
      sb.AppendLine( "Key: urban" );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: u" );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: urban <sound> [KEYWORD]" );
      sb.AppendLine( "Bei mitangabe des optionalen Parameters 'sound' wird auf urban dictionary nach einem entsprechenden soundfile gesucht!" );
      return sb.ToString();
    }
    #endregion ICommand

    #region jsonClasses
    private class UrbanJson
    {
      public List<UrbanEntry> list { get; set; }
      public List<string> sounds { get; set; }
    }
    private class UrbanEntry
    {
      public string permalink { get; set; }
      public string definition { get; set; }
    }
    #endregion jsonClasses
  }
}
