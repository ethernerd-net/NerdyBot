using Discord;
using NerdyBot.Commands.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  class RandomTag : ICommand
  {
    private const string DEFAULTKEY = "random";
    private static readonly string[] DEFAULTALIASES = new string[] { "rnd", "rand" };

    private BaseCommandConfig conf;

    #region ICommand
    public BaseCommandConfig Config { get { return this.conf; } }

    public void Init()
    {
      this.conf = new BaseCommandConfig( DEFAULTKEY, DEFAULTALIASES );
      //this.conf.Read();
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( args.Count() == 1 )
        {
          switch ( args[0] )
          {
          case "cat":
            string catJson = ( new WebClient() ).DownloadString( "http://random.cat/meow" );
            var cat = JsonConvert.DeserializeObject<RandomCat>( catJson );
            client.Write( cat.file.Replace( "\\", "" ), msg.Channel );
            break;

          case "penguin":
            string pengu = ( new WebClient() ).DownloadString( "http://penguin.wtf/" );
            client.Write( pengu, msg.Channel );
            break;

          case "bunny":
            string bunnyJson = ( new WebClient() ).DownloadString( "https://api.bunnies.io/v2/loop/random/?media=gif" );
            var bunny = JsonConvert.DeserializeObject<RandomBunny>( bunnyJson );
            client.Write( bunny.media.gif, msg.Channel );          
            break;

          case "chuck":
            string chuckJson = ( new WebClient() ).DownloadString( "http://api.icndb.com/jokes/random" );
            var chuck = JsonConvert.DeserializeObject<RandomChuck>( chuckJson );
            client.Write( chuck.value.joke, msg.Channel );
            break;

          case "joke":
            string jokeJson = ( new WebClient() ).DownloadString( "http://tambal.azurewebsites.net/joke/random" );
            var joke = JsonConvert.DeserializeObject<ChuckJoke>( jokeJson );
            client.Write( joke.joke, msg.Channel );
            break;

          case "yomomma":          
            string momJson = ( new WebClient() ).DownloadString( "http://api.yomomma.info/" );
            var mom = JsonConvert.DeserializeObject<ChuckJoke>( momJson );
            client.Write( mom.joke, msg.Channel );
            break;

          case "quote":
            string quoteJson = ( new WebClient() ).DownloadString( "http://quotesondesign.com/wp-json/posts?filter[orderby]=rand" );
            var quote = JsonConvert.DeserializeObject<List<RandomQuote>>( quoteJson ).First();
            string text = StripHTML( EntityToUnicode( quote.content ) );
            client.WriteBlock( text + Environment.NewLine + Environment.NewLine + "-" + quote.title, "", msg.Channel );
            break;

          case "trump":
            string trumpJson = ( new WebClient() ).DownloadString( "https://api.whatdoestrumpthink.com/api/v1/quotes/random" );
            var trump = JsonConvert.DeserializeObject<TrumpQuote>( trumpJson );
            client.Write( "Trump : " + trump.message, msg.Channel );
            break;

          case "xkcd":
            string xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/info.0.json" );
            var xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );

            xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/" + ( new Random() ).Next( xkcd.num ) + "/info.0.json" );
            xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );
            client.Write( xkcd.img.Replace( "\\", "" ), msg.Channel );
            break;

          case "help":
            msg.User.SendMessage( "```" + FullHelp( client.Config.Prefix ) + "```" );
            break;

          default:
            client.WriteInfo( "Invalider Parameter boi!", msg.Channel );
            break;
          }
        }
        else
          client.WriteInfo( "Invalider Parameter boi!", msg.Channel );
      } );
    }
    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== RANDOM ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Random Command gibt, je nach Sub-Parameter, einen zufälligen Output zurück." );
      sb.AppendLine( "Key: " + this.conf.Key );
      return sb.ToString();
    }
    public string FullHelp( char prefix )
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine( prefix + this.conf.Key + " [option]" );
      sb.AppendLine( "option: cat | penguin | bunny | chuck | joke | yomomma | quote | trump | xkcd | help" );
      return sb.ToString();
    }
    #endregion ICommand

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
    class RandomCat
    {
      public string file { get; set; }
    }
    class RandomBunny
    {
      public int id { get; set; }
      public BunnyMedia media { get; set; }
    }
    class BunnyMedia
    {
      public string gif { get; set; }
      public string poster { get; set; }
    }
    class RandomChuck
    {
      public ChuckJoke value { get; set; }
    }
    class ChuckJoke
    {
      public int id { get; set; }
      public string joke { get; set; }
    }
    class RandomQuote
    {
      public int ID { get; set; }
      public string title { get; set; }
      public string content { get; set; }
      public string link { get; set; }
    }
    class TrumpQuote
    {
      public string message { get; set; }
    }
    class RandomXKCD
    {
      public int num { get; set; }
      public string img { get; set; }
    }

    #endregion
  }
}
