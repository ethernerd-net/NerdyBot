using NerdyBot.Commands.Config;
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
  class UrbanCommand : ICommand
  {
    private BaseCommandConfig conf;
    private const string DEFAULTKEY = "urban";
    private static readonly string[] DEFAULTALIASES = new string[] { "u" };

    private IClient client;

    #region ICommand
    public ICommandConfig Config { get { return this.conf; } }

    public void Init( IClient client )
    {
      this.conf = new BaseCommandConfig( DEFAULTKEY, DEFAULTALIASES );
      this.conf.Read();
      this.client = client;
    }

    public Task Execute( ICommandMessage msg )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( msg.Arguments.Length == 1 && msg.Arguments[0] == "help" )
          this.client.SendMessage( FullHelp(),
            new SendMessageOptions() { TargetType = TargetType.User, TargetId = msg.User.Id, MessageType = MessageType.Block } );
        else if ( msg.Arguments.Length > 1 && msg.Arguments[0] == "sound" )
        {
          string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", msg.Arguments.Skip( 1 ) ) );
          var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
          if ( urban.sounds != null && urban.sounds.Count() > 0 )
          {
            string soundUrl = urban.sounds.First();
            if ( soundUrl.EndsWith( ".mp3" ) )
            {
              client.StopPlaying = false;
              string path = Path.Combine( "urban", string.Join( " ", msg.Arguments.Skip( 1 ) ) + ".mp3" );
              if ( !File.Exists( path ) )
                client.DownloadAudio( urban.sounds.First(), path );
              client.SendAudio( msg.User, path );
            }
            else
              this.client.SendMessage( "404, Urban not found!",
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );
          }
          else
            this.client.SendMessage( "404, Urban not found!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );
        }
        else
        {
          string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", msg.Arguments ) );
          var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
          if ( urban.list != null && urban.list.Count() > 0 )
            this.client.SendMessage( urban.list.First().permalink.Replace( "\\", "" ),
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
          else
            this.client.SendMessage( "404, Urban not found!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );
        }
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== Urban ========" );
      sb.AppendLine();
      sb.AppendLine( "Sucht auf Urban Dictonary nach dem eingegebenen Keyword." );
      sb.AppendLine( "Key: " + this.conf.Key );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: " + this.conf.Key + " <sound> [KEYWORD]" );
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
