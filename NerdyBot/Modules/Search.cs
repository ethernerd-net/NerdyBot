using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;
using Discord.Commands;

using NerdyBot.Services;
using NerdyBot.Models;

namespace NerdyBot.Modules
{
  [Group( "search" ), Alias( "s" )]
  public class Search : ModuleBase
  {
    public BotConfig BotConfig { get; set; }
    public MessageService MessageService { get; set; }
    public YoutubeService YoutubeService { get; set; }

    [Command("youtube"), Alias("yt")]
    public async Task Youtube( string query )
    {
      var responseItem = ( await YoutubeService.SearchVideos( query, 1 ) ).FirstOrDefault();
      if ( responseItem != null )
        await MessageService.SendMessageToCurrentChannel( Context, $"https://www.youtube.com/watch?v={responseItem.Id.VideoId}" );
      else
        await MessageService.SendMessageToCurrentChannel( Context, "Und ich dachte es gibt alles auf youtube", MessageType.Info );
    }

    [Command( "imgur" ), Alias( "i" )]
    public async Task Imgur( string query )
    {
      var request = WebRequest.CreateHttp( $"https://api.imgur.com/3/gallery/search/viral?q={query}" );
      request.Headers.Add( HttpRequestHeader.Authorization, $"Client-ID {BotConfig.ImgurClientId}" );
      var response = ( HttpWebResponse )await request.GetResponseAsync();
      using ( StreamReader reader = new StreamReader( response.GetResponseStream() ) )
      {
        string responseJson = reader.ReadToEnd();
        var imgurJson = JsonConvert.DeserializeObject<ImgurJson>( responseJson );
        ImgurData data;
        if ( imgurJson.success && ( data = imgurJson.data.FirstOrDefault() ) != null )
          await MessageService.SendMessageToCurrentChannel( Context, data.link.Replace( "\\", "" ) );
        else
          await MessageService.SendMessageToCurrentChannel( Context, "No memes today.", MessageType.Info );
      }
    }

    [Command( "urban" ), Alias( "u" )]
    public async Task Urban( string query )
    {
      string urbanJson = ( new WebClient() ).DownloadString( $"http://api.urbandictionary.com/v0/define?term={query}" );
      var urban = JsonConvert.DeserializeObject<UrbanJson>( urbanJson );
      if ( urban.list != null && urban.list.Count() > 0 )
        await MessageService.SendMessageToCurrentChannel( Context, urban.list.First().permalink.Replace( "\\", "" ) );
      else
        await MessageService.SendMessageToCurrentChannel( Context, "404, Urban not found!", MessageType.Info );
    }

    [Command( "lyrics" ), Alias( "l" )]
    public async Task Lyrics( string query )
    {
      var request = WebRequest.CreateHttp( $"http://api.genius.com/search?q={query}&access_token={BotConfig.GeniusAccessToken}" );
      //request.Headers.Add( HttpRequestHeader.Authorization, $"Bearer {BotConfig.GeniusAccessToken}" );
      var response = ( HttpWebResponse )await request.GetResponseAsync();
      using ( StreamReader reader = new StreamReader( response.GetResponseStream() ) )
      {
        string responseJson = reader.ReadToEnd();
        var geniusJson = JsonConvert.DeserializeObject<GeniusJson>( responseJson ).response;
        Hit data;
        if ( ( data = geniusJson.hits.FirstOrDefault() ) != null )
          await MessageService.SendMessageToCurrentChannel( Context, data.result.url.Replace( "\\", "" ) );
        else
          await MessageService.SendMessageToCurrentChannel( Context, "No memes today.", MessageType.Info );
      }
    }

    [Command( "help" )]
    public async Task Help()
    {
      await MessageService.SendMessageToCurrentUser( Context, FullHelp(), MessageType.Block );
    }

    public static string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== SEARCH ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Search Befehlt ermöglicht eine 'Auf gut Glück'-Suche auf verschiedenen Plattformen." );
      sb.AppendLine( "Key: search" );
      return sb.ToString();
    }
    public static string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: s" );
      sb.AppendLine(  );
      sb.AppendLine( "Plattformen: youtube/yt | imgur/i | urban/u | lyrics/l" );
      sb.AppendLine();
      sb.AppendLine( "Beispiel: !search [PLATTFORM] query" );
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
    private class UrbanJson
    {
      public List<UrbanEntry> list { get; set; }
      public List<string> sounds { get; set; }
    }
    private class UrbanEntry
    {
      public string permalink { get; set; }
      public string definition { get; set; }
    }
    public class Meta
    {
      public int status { get; set; }
    }

    public class Stats
    {
      public bool hot { get; set; }
      public int unreviewed_annotations { get; set; }
      public int pageviews { get; set; }
    }

    public class PrimaryArtist
    {
      public string api_path { get; set; }
      public string header_image_url { get; set; }
      public int id { get; set; }
      public string image_url { get; set; }
      public bool is_meme_verified { get; set; }
      public bool is_verified { get; set; }
      public string name { get; set; }
      public string url { get; set; }
      public int iq { get; set; }
    }

    public class Result
    {
      public int annotation_count { get; set; }
      public string api_path { get; set; }
      public string full_title { get; set; }
      public string header_image_thumbnail_url { get; set; }
      public string header_image_url { get; set; }
      public int id { get; set; }
      public int lyrics_owner_id { get; set; }
      public string path { get; set; }
      public int? pyongs_count { get; set; }
      public string song_art_image_thumbnail_url { get; set; }
      public Stats stats { get; set; }
      public string title { get; set; }
      public string title_with_featured { get; set; }
      public string url { get; set; }
      public PrimaryArtist primary_artist { get; set; }
    }

    public class Hit
    {
      public List<object> highlights { get; set; }
      public string index { get; set; }
      public string type { get; set; }
      public Result result { get; set; }
    }

    public class GeniusData
    {
      public List<Hit> hits { get; set; }
    }

    public class GeniusJson
    {
      public Meta meta { get; set; }
      public GeniusData response { get; set; }
    }
    #endregion jsonClasses
  }
}
