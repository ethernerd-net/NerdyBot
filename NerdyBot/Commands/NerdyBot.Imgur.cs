using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using NerdyBot.Commands.Config;
using NerdyBot.Contracts;
using Discord.Commands;

namespace NerdyBot.Commands
{
  public class ImgurCommand : ModuleBase
  {
    private MessageService svcMessage;

    #region ICommand

    public ImgurCommand( MessageService svcMessage )
    {
      this.svcMessage = svcMessage;
    }

    [Command( "imgur" ), Alias( "i" )]
    public Task Execute( params string[] args )
    {
      return Task.Factory.StartNew( async () =>
      {
        if ( args.Length == 1 && args[0] == "help" )
          this.svcMessage.SendMessage( Context, FullHelp(),
            new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
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
              this.svcMessage.SendMessage( Context, data.link.Replace( "\\", "" ),
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
            else
              this.svcMessage.SendMessage( Context, "No memes today.",
                new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
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
      sb.AppendLine( "Key: imgur" );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: i" );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: imgur [KEYWORDS]" );
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
