using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Discord.Commands;

using NerdyBot.Contracts;

namespace NerdyBot.Commands
{

  [Group( "imgur" ), Alias( "i" )]
  public class ImgurCommand : ModuleBase
  {
    public MessageService MessageService { get; set; }

    [Command( "help" )]
    public async Task Help()
    {
      MessageService.SendMessage( Context, FullHelp(),
        new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
    }


    [Command()]
    public async Task Execute( params string[] args )
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
          MessageService.SendMessage( Context, data.link.Replace( "\\", "" ),
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
        else
          MessageService.SendMessage( Context, "No memes today.",
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
      }
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
