using System;
using System.Web;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

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
        ApplicationName = appName
      } );
    }

    public async Task<IEnumerable<SearchResult>> SearchVideos( string query, int maxResults = 10, SearchResource.ListRequest.OrderEnum order = SearchResource.ListRequest.OrderEnum.ViewCount )
    {
      var searchSearchRequest = this.yt.Search.List( "snippet" );
      searchSearchRequest.Q = query;
      searchSearchRequest.MaxResults = maxResults;
      searchSearchRequest.Type = "video";
      searchSearchRequest.Order = order;

      return ( await searchSearchRequest.ExecuteAsync() ).Items.Where( item => item.Id.Kind == "youtube#video" );
    }

    public async Task<Video> GetVideoInfo( string vId )
    {
      var searchVideoRequest = this.yt.Videos.List( "snippet,contentDetails,statistics" );
      searchVideoRequest.Id = vId;
      searchVideoRequest.MaxResults = 1;

      return ( await searchVideoRequest.ExecuteAsync() ).Items.FirstOrDefault();
    }
    public async Task<Playlist> GetPlaylistInfo( string plId )
    {
      var searchPlaylistRequest = this.yt.Playlists.List( "snippet,contentDetails" );
      searchPlaylistRequest.Id = plId;
      searchPlaylistRequest.MaxResults = 1;

      return ( await searchPlaylistRequest.ExecuteAsync() ).Items.FirstOrDefault();
    }

    public YoutubeIdResultSet GetIDfromUrl( string url )
    {
      var uri = new Uri( url );
      var query = HttpUtility.ParseQueryString( uri.Query );

      YoutubeIdResultSet resultSet;

      if ( query.AllKeys.Contains( "list" ) )
        resultSet = new YoutubeIdResultSet() { Id = query["list"], IdType = YoutubeIdType.Playlist };
      else if ( query.AllKeys.Contains( "v" ) )
        resultSet = new YoutubeIdResultSet() { Id = query["v"], IdType = YoutubeIdType.Video };
      else
        resultSet = new YoutubeIdResultSet() { Id = uri.Segments.Last(), IdType = YoutubeIdType.Video };
      return resultSet;
    }

    public bool IsYoutubeUrl( string url )
    {
      return Regex.IsMatch( url, @"^(https?\:\/\/)?(www\.)?(youtube\.com|youtu\.?be)\/.+$" );
    }
  }
  public class YoutubeIdResultSet
  {
    public string Id { get; set; }
    public YoutubeIdType IdType { get; set; }
  }
  public enum YoutubeIdType { Video, Playlist }
}
