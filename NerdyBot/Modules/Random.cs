using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Discord.Commands;

using NerdyBot.Services;

namespace NerdyBot.Modules
{
  [Group( "random" ), Alias( "rnd", "rand" )]
  public class RandomTag : ModuleBase
  {
    public MessageService MessageService { get; set; }

    [Command( "cat" )]
    public async Task Cat()
    {
      string catJson = ( new WebClient() ).DownloadString( "http://random.cat/meow" );
      var cat = JsonConvert.DeserializeObject<RandomCat>( catJson );
      await MessageService.SendMessageToCurrentChannel( Context, cat.file.Replace( "\\", "" ) );
    }

    [Command( "penguin" )]
    public async Task Penguin()
    {
      string pengu = ( new WebClient() ).DownloadString( "http://penguin.wtf/" );
      await MessageService.SendMessageToCurrentChannel( Context, pengu );
    }

    [Command( "bunny" )]
    public async Task Bunny()
    {
      string bunnyJson = ( new WebClient() ).DownloadString( "https://api.bunnies.io/v2/loop/random/?media=gif" );
      var bunny = JsonConvert.DeserializeObject<RandomBunny>( bunnyJson );
      await MessageService.SendMessageToCurrentChannel( Context, bunny.media.gif );
    }

    [Command( "chuck" )]
    public async Task Chuck()
    {
      string chuckJson = ( new WebClient() ).DownloadString( "http://api.icndb.com/jokes/random" );
      var chuck = JsonConvert.DeserializeObject<RandomChuck>( chuckJson );
      await MessageService.SendMessageToCurrentChannel( Context, chuck.value.joke );
    }

    [Command( "joke" )]
    public async Task Joke()
    {
      string jokeJson = ( new WebClient() ).DownloadString( "http://tambal.azurewebsites.net/joke/random" );
      var joke = JsonConvert.DeserializeObject<ChuckJoke>( jokeJson );
      await MessageService.SendMessageToCurrentChannel( Context, joke.joke );
    }

    [Command( "yomomma" )]
    public async Task Yomomma()
    {
      string momJson = ( new WebClient() ).DownloadString( "http://api.yomomma.info/" );
      var mom = JsonConvert.DeserializeObject<ChuckJoke>( momJson );
      await MessageService.SendMessageToCurrentChannel( Context, mom.joke );
    }

    [Command( "quote" )]
    public async Task Quote()
    {
      string quoteJson = ( new WebClient() ).DownloadString( "http://quotesondesign.com/wp-json/posts?filter[orderby]=rand" );
      var quote = JsonConvert.DeserializeObject<List<RandomQuote>>( quoteJson ).First();
      string text = StripHTML( EntityToUnicode( quote.content ) );
      await MessageService.SendMessageToCurrentChannel( Context, text + Environment.NewLine + Environment.NewLine + "-" + quote.title, MessageType.Block );
    }

    [Command( "trump" )]
    public async Task Trump()
    {
      string trumpJson = ( new WebClient() ).DownloadString( "https://api.whatdoestrumpthink.com/api/v1/quotes/random" );
      var trump = JsonConvert.DeserializeObject<TrumpQuote>( trumpJson );
      await MessageService.SendMessageToCurrentChannel( Context, "Trump : " + trump.message );
    }

    [Command( "xkcd" )]
    public async Task xkcd()
    {
      string xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/info.0.json" );
      var xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );

      xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/" + ( new Random() ).Next( xkcd.num ) + "/info.0.json" );
      xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );
      await MessageService.SendMessageToCurrentChannel( Context, xkcd.img.Replace( "\\", "" ) );
    }

    [Command( "help" )]
    public async Task help()
    {
      await MessageService.SendMessageToCurrentUser( Context, FullHelp(), MessageType.Block );
    }

    public static string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== RANDOM ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Random Command gibt, je nach Sub-Parameter, einen zufälligen Output zurück." );
      sb.AppendLine( "Key: random" );
      return sb.ToString();
    }
    public static string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: rnd | rand" );
      sb.AppendLine();
      sb.AppendLine( "random [option]" );
      sb.AppendLine( "option: cat | penguin | bunny | chuck | joke | yomomma | quote | trump | xkcd | help" );
      return sb.ToString();
    }

    public string EntityToUnicode( string html )
    {
      var replacements = new Dictionary<string, string>();
      var regex = new Regex( "(&[a-z,0-9,#]{2,6};)" );
      foreach ( Match match in regex.Matches( html ) )
      {
        if ( !replacements.ContainsKey( match.Value ) )
        {
          var unicode = WebUtility.HtmlDecode( match.Value );
          if ( unicode.Length == 1 )
          {
            replacements.Add( match.Value, unicode );
          }
        }
      }
      foreach ( var replacement in replacements )
      {
        html = html.Replace( replacement.Key, replacement.Value );
      }
      return html;
    }
    public string StripHTML( string input )
    {
      return Regex.Replace( input, "<.*?>", String.Empty );
    }

    #region jsonClasses
    private class RandomCat
    {
      public string file { get; set; }
    }
    private class RandomBunny
    {
      public int id { get; set; }
      public BunnyMedia media { get; set; }
    }
    private class BunnyMedia
    {
      public string gif { get; set; }
      public string poster { get; set; }
    }
    private class RandomChuck
    {
      public ChuckJoke value { get; set; }
    }
    private class ChuckJoke
    {
      public int id { get; set; }
      public string joke { get; set; }
    }
    private class RandomQuote
    {
      public int ID { get; set; }
      public string title { get; set; }
      public string content { get; set; }
      public string link { get; set; }
    }
    private class TrumpQuote
    {
      public string message { get; set; }
    }
    private class RandomXKCD
    {
      public int num { get; set; }
      public string img { get; set; }
    }
    #endregion jsonClasses
  }
}
