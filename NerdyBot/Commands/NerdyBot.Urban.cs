using Discord;
using NerdyBot.Commands.Config;
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
    private const string DEFAULTKEY = "urban";
    private static readonly string[] DEFAULTALIASES = new string[] {  };

    private BaseCommandConfig conf;

    #region ICommand
    public BaseCommandConfig Config { get { return this.conf; } }

    public void Init()
    {
      this.conf = new BaseCommandConfig( DEFAULTKEY, DEFAULTALIASES );
      this.conf.Read();
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( args.Length == 1 && args[0] == "help" )
          msg.User.SendMessage( "```" + FullHelp( client.Config.Prefix ) + "```" );
        else if ( args.Length > 1 && args[0] == "sound" )
        {
          string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", args.Skip( 1 ) ) );
          var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
          if ( urban.sounds != null && urban.sounds.Count() > 0 )
          {
            string soundUrl = urban.sounds.First();
            if ( soundUrl.EndsWith( ".mp3" ) )
            {
              client.StopPlaying = false;
              string path = Path.Combine( msg.Server.Name, string.Join( " ", args.Skip( 1 ) ) + ".mp3" );
              if ( !File.Exists( path ) )
                client.DownloadAudio( urban.sounds.First(), path );
              client.SendAudio( msg.User.VoiceChannel, path );
            }
            else
              client.WriteInfo( "404, Urban not found!", msg.Channel );
          }
          else
            client.WriteInfo( "404, Urban not found!", msg.Channel );
        }
        else
        {
          string urbanJson = ( new WebClient() ).DownloadString( "http://api.urbandictionary.com/v0/define?term=" + string.Join( " ", args ) );
          var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
          if ( urban.list != null && urban.list.Count() > 0 )
            client.Write( urban.list.First().permalink.Replace( "\\", "" ), msg.Channel );
          else
            client.WriteInfo( "404, Urban not found!", msg.Channel );
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
    public string FullHelp( char prefix )
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: " + prefix + this.conf.Key + " <sound> [KEYWORD]" );
      sb.AppendLine( "Bei mitangabe des optionalen Parameters 'sound' wird auf urban dictionary nach einem entsprechenden soundfile gesucht!" );
      return sb.ToString();
    }
    #endregion ICommand
  }
  public class UrbanJson
  {
    public List<UrbanEntry> list { get; set; }
    public List<string> sounds { get; set; }
  }
  public class UrbanEntry
  {
    public string permalink { get; set; }
    public string definition { get; set; }
  }
}
