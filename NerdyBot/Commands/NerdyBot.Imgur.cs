using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using NerdyBot.Commands.Config;
using NerdyBot.Contracts;

namespace NerdyBot.Commands
{
  class ImgurCommand : ICommand
  {
    private BaseCommandConfig conf;
    private const string DEFAULTKEY = "imgur";
    private static readonly IEnumerable<string> DEFAULTALIASES = new string[] { "i" };

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
      return Task.Factory.StartNew( async () =>
      {
        if ( msg.Arguments.Length == 1 && msg.Arguments[0] == "help" )
          this.client.SendMessage( FullHelp(),
            new SendMessageOptions() { TargetType = TargetType.User, TargetId = msg.User.Id, MessageType = MessageType.Block } );
        else
        {
          var request = WebRequest.CreateHttp( "https://api.imgur.com/3/gallery/search/viral?q=" + string.Join( " ", msg.Arguments ) );
          request.Headers.Add( HttpRequestHeader.Authorization, "Client-ID 9101404d39fcd20" ); 
          var response = ( HttpWebResponse )await request.GetResponseAsync();
          using ( StreamReader reader = new StreamReader( response.GetResponseStream() ) )
          {
            string responseJson = reader.ReadToEnd();
            var imgurJson = JsonConvert.DeserializeObject<ImgurJson>( responseJson );
            ImgurData data;
            if ( imgurJson.success && ( data = imgurJson.data.FirstOrDefault() ) != null )
              this.client.SendMessage( data.link.Replace( "\\", "" ),
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            else
              this.client.SendMessage( "No memes today.",
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );
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
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: " + this.conf.Key + " [KEYWORDS]" );
      return sb.ToString();
    }
    #endregion ICommand

    #region jsonClasses
    private class ImgurJson
    {
      public bool success { get; set; }
      public List<ImgurData> data { get; set; }
    }
    private class ImgurData
    {
      public string id { get; set; }
      public string title { get; set; }
      public string link { get; set; }
      public int views { get; set; }
    }
    #endregion jsonClasses
  }
}
