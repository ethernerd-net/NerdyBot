using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;

using NerdyBot.Config;
using NerdyBot.Contracts;
using Discord.Commands;

namespace NerdyBot.Commands
{
  [Group( "youtube" ), Alias( "yt" )]
  public class YoutubeCommand : ModuleBase
  {
    private BaseCommandConfig conf;

    public MessageService MessageService { get; set; }
    
    public YoutubeCommand()
    {
      this.conf = new BaseCommandConfig( "youtube" );
      this.conf.Read();
    }

    [Command()]
    public async Task Execute( params string[] args )
    {
      var youtubeService = new YouTubeService( new BaseClientService.Initializer()
      {
        ApiKey = this.conf.ApiKey,
        ApplicationName = "NerdyBot"
      } );

      var searchListRequest = youtubeService.Search.List( "snippet" );
      searchListRequest.Q = string.Join( " ", args ); // Replace with your search term.
      searchListRequest.MaxResults = 1;
      searchListRequest.Type = "video";
      searchListRequest.Order = SearchResource.ListRequest.OrderEnum.ViewCount;

      var searchListResponse = await searchListRequest.ExecuteAsync();
      var responseItem = searchListResponse.Items.FirstOrDefault( item => item.Id.Kind == "youtube#video" );
      if ( responseItem != null )
        MessageService.SendMessage( Context, "https://www.youtube.com/watch?v=" + responseItem.Id.VideoId,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      else
        MessageService.SendMessage( Context, "Und ich dachte es gibt alles auf youtube",
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
    }

    [Command( "help" )]
    public async Task Help()
    {
      MessageService.SendMessage( Context, FullHelp(),
        new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== Youtube ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Youtube Befehlt ermöglicht eine 'Auf gut Glück'-Suche." );
      sb.AppendLine( "Key: yotuube" );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: yt" );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: youtube [KEYWORDS]" );
      return sb.ToString();
    }
  }
}
