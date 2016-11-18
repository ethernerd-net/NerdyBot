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
  class ImgurCommand : ICommand
  {
    private const string DEFAULTKEY = "imgur";
    private static readonly string[] DEFAULTALIASES = new string[] { "i" };

    private BaseCommandConfig conf;

    #region ICommand
    public BaseCommandConfig Config { get { return this.conf; } }

    public void Init()
    {
      this.conf = new BaseCommandConfig( DEFAULTKEY, DEFAULTALIASES );
      //this.conf.Read();
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( async () =>
      {
        if ( args.Length == 1 && args[0] == "help" )
          msg.User.SendMessage( "```" + FullHelp( client.Config.Prefix ) + "```" );
        else
        {
          var request = WebRequest.CreateHttp( "https://api.imgur.com/3/gallery/search/viral?q=" + string.Join( " ", args ) );
          request.Headers.Add( HttpRequestHeader.Authorization, "Client-ID 9101404d39fcd20" ); 
          var response = ( HttpWebResponse )await request.GetResponseAsync();
          using ( StreamReader reader = new StreamReader( response.GetResponseStream() ) )
          {
            string responseJson = reader.ReadToEnd();
            var imgurJson = JsonConvert.DeserializeObject<ImgurJson>( responseJson );
            ImgurData data;
            if ( imgurJson.success && ( data = imgurJson.data.FirstOrDefault() ) != null )
              client.Write( data.link.Replace( "\\", "" ), msg.Channel );
            else
              client.WriteInfo( "No memes today.", msg.Channel );
          }
        }
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== IMGUR ========" );
      sb.AppendLine();
      sb.AppendLine( "Posted das erstbeste auf Imgur gefundene Bild anhand der Keywords." );
      sb.AppendLine( "Key: " + this.conf.Key );
      return sb.ToString();
    }
    public string FullHelp( char prefix )
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: " + prefix + this.conf.Key + " [KEYWORDS]" );
      return sb.ToString();
    }
    #endregion ICommand

    class ImgurJson
    {
      public bool success { get; set; }
      public List<ImgurData> data { get; set; }
    }
    class ImgurData
    {
      public string id { get; set; }
      public string title { get; set; }
      public string link { get; set; }
      public int views { get; set; }
    }
  }
}
