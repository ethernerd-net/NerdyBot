using Discord;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using NerdyBot.Commands.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  class YoutubeCommand : ICommand
  {
    private const string DEFAULTKEY = "youtube";
    private static readonly string[] DEFAULTALIASES = new string[] { "yt" };

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
      return Task.Factory.StartNew( async () =>
      {
        if ( args.Length == 1 && args[0] == "help" )
          msg.User.SendMessage( "```" + FullHelp( client.Config.Prefix ) + "```" );
        else
        {
          var youtubeService = new YouTubeService( new BaseClientService.Initializer()
          {
            ApiKey = "AIzaSyAmrg8abuMO0esvieSZCdduxqog815QRnY",
            ApplicationName = this.GetType().ToString()
          } );

          var searchListRequest = youtubeService.Search.List( "snippet" );
          searchListRequest.Q = string.Join( " ", args ); // Replace with your search term.
          searchListRequest.MaxResults = 1;
          searchListRequest.Type = "video";
          searchListRequest.Order = SearchResource.ListRequest.OrderEnum.ViewCount;

          var searchListResponse = await searchListRequest.ExecuteAsync();
          var responseItem = searchListResponse.Items.FirstOrDefault( item => item.Id.Kind == "youtube#video" );
          if ( responseItem != null )
            client.Write( "https://www.youtube.com/watch?v=" + responseItem.Id.VideoId, msg.Channel );
          else
            client.WriteInfo( "Und ich dachte es gibt alles auf youtube", msg.Channel );        
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
  }
}
