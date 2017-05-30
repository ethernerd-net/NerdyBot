using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;

using NerdyBot.Commands.Config;
using NerdyBot.Contracts;
using Discord.Commands;

namespace NerdyBot.Commands
{
  public class YoutubeCommand : ModuleBase
  {
    private BaseCommandConfig conf;

    private MessageService svcMessage;

    #region ICommand
    public ICommandConfig Config { get { return this.conf; } }

    public YoutubeCommand( MessageService svcMessage )
    {
      this.conf = new BaseCommandConfig( "youtube" );
      this.conf.Read();
      this.svcMessage = svcMessage;
    }

    [Command( "youtube" ), Alias( "yt" )]
    public Task Execute( params string[] args )
    {
      return Task.Factory.StartNew( async () =>
      {
        if ( args.Length == 1 && args[0] == "help" )
          this.svcMessage.SendMessage( Context, FullHelp(),
            new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
        else
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
            this.svcMessage.SendMessage( Context, "https://www.youtube.com/watch?v=" + responseItem.Id.VideoId,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
          else
            this.svcMessage.SendMessage( Context, "Und ich dachte es gibt alles auf youtube",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Info } );
        }
      } );
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
    #endregion ICommand
  }
}
