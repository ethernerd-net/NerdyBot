using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using NerdyBot.Commands.Config;
using NerdyBot.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  class YoutubeCommand : ICommand
  {
    private BaseCommandConfig conf;
    private const string DEFAULTKEY = "youtube";
    private static readonly IEnumerable<string> DEFAULTALIASES = new string[] { "yt" };

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
           var youtubeService = new YouTubeService( new BaseClientService.Initializer()
          {
            ApiKey = "AIzaSyAmrg8abuMO0esvieSZCdduxqog815QRnY",
            ApplicationName = this.GetType().ToString()
          } );

          var searchListRequest = youtubeService.Search.List( "snippet" );
          searchListRequest.Q = string.Join( " ", msg.Arguments ); // Replace with your search term.
          searchListRequest.MaxResults = 1;
          searchListRequest.Type = "video";
          searchListRequest.Order = SearchResource.ListRequest.OrderEnum.ViewCount;

          var searchListResponse = await searchListRequest.ExecuteAsync();
          var responseItem = searchListResponse.Items.FirstOrDefault( item => item.Id.Kind == "youtube#video" );
          if ( responseItem != null )
            this.client.SendMessage( "https://www.youtube.com/watch?v=" + responseItem.Id.VideoId,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
          else
            this.client.SendMessage( "Und ich dachte es gibt alles auf youtube",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );
        }
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== Youtube ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Youtube Befehlt ermöglicht eine 'Auf gut Glück'-Suche." );
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
  }
}
