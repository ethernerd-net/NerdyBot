using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using NerdyBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdyBot.Services
{
  public class YoutubeService
  {
    private YouTubeService yt;


    public YoutubeService( string apiKey, string appName )
    {
      this.yt = new YouTubeService( new BaseClientService.Initializer()
      {
        ApiKey = apiKey,
        ApplicationName = appName// "NerdyBot" //TODO
      } );
    }

    public async Task<IEnumerable<SearchResult>> SearchVideos( string query, int maxResults = 10, SearchResource.ListRequest.OrderEnum order = SearchResource.ListRequest.OrderEnum.ViewCount )
    {
      var searchListRequest = this.yt.Search.List( "snippet" );
      searchListRequest.Q = query;
      searchListRequest.MaxResults = maxResults;
      searchListRequest.Type = "video";
      searchListRequest.Order = order;

      return ( await searchListRequest.ExecuteAsync() ).Items.Where( item => item.Id.Kind == "youtube#video" );
    }
  }
}
