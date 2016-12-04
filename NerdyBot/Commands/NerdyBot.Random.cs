using NerdyBot.Commands.Config;
using NerdyBot.Contracts;
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
    private BaseCommandConfig conf;
    private const string DEFAULTKEY = "random";
    private static readonly IEnumerable<string> DEFAULTALIASES = new string[] { "rnd", "rand" };

    private IClient client;

    #region ICommand
    public ICommandConfig Config { get { return this.conf; } }

    public void Init( IClient client )
    {
      this.conf = new BaseCommandConfig( DEFAULTKEY, DEFAULTALIASES );
      this.client = client;
    }

    public Task Execute( ICommandMessage msg )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( msg.Arguments.Count() == 1 )
        {
          switch ( msg.Arguments[0] )
          {
          case "cat":
            string catJson = ( new WebClient() ).DownloadString( "http://random.cat/meow" );
            var cat = JsonConvert.DeserializeObject<RandomCat>( catJson );
            this.client.SendMessage( cat.file.Replace( "\\", "" ),
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "penguin":
            string pengu = ( new WebClient() ).DownloadString( "http://penguin.wtf/" );
            this.client.SendMessage( pengu,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "bunny":
            string bunnyJson = ( new WebClient() ).DownloadString( "https://api.bunnies.io/v2/loop/random/?media=gif" );
            var bunny = JsonConvert.DeserializeObject<RandomBunny>( bunnyJson );
            this.client.SendMessage( bunny.media.gif,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "chuck":
            string chuckJson = ( new WebClient() ).DownloadString( "http://api.icndb.com/jokes/random" );
            var chuck = JsonConvert.DeserializeObject<RandomChuck>( chuckJson );
            this.client.SendMessage( chuck.value.joke,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "joke":
            string jokeJson = ( new WebClient() ).DownloadString( "http://tambal.azurewebsites.net/joke/random" );
            var joke = JsonConvert.DeserializeObject<ChuckJoke>( jokeJson );
            this.client.SendMessage( joke.joke,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "yomomma":          
            string momJson = ( new WebClient() ).DownloadString( "http://api.yomomma.info/" );
            var mom = JsonConvert.DeserializeObject<ChuckJoke>( momJson );
            this.client.SendMessage( mom.joke,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "quote":
            string quoteJson = ( new WebClient() ).DownloadString( "http://quotesondesign.com/wp-json/posts?filter[orderby]=rand" );
            var quote = JsonConvert.DeserializeObject<List<RandomQuote>>( quoteJson ).First();
            string text = StripHTML( EntityToUnicode( quote.content ) );
            this.client.SendMessage( text + Environment.NewLine + Environment.NewLine + "-" + quote.title,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Block } );
            break;

          case "trump":
            string trumpJson = ( new WebClient() ).DownloadString( "https://api.whatdoestrumpthink.com/api/v1/quotes/random" );
            var trump = JsonConvert.DeserializeObject<TrumpQuote>( trumpJson );
            this.client.SendMessage( "Trump : " + trump.message,
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "xkcd":
            string xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/info.0.json" );
            var xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );

            xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/" + ( new Random() ).Next( xkcd.num ) + "/info.0.json" );
            xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );
            this.client.SendMessage( xkcd.img.Replace( "\\", "" ),
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id } );
            break;

          case "help":
            this.client.SendMessage( FullHelp(),
              new SendMessageOptions() { TargetType = TargetType.User, TargetId = msg.User.Id, MessageType = MessageType.Block } );
            break;

          default:
            this.client.SendMessage( "Invalider Parameter boi!",
              new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );
            break;
          }
        }
        else
          this.client.SendMessage( "Invalider Parameter boi!",
            new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = msg.Channel.Id, MessageType = MessageType.Info } );
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
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
      sb.AppendLine();
      sb.AppendLine( this.conf.Key + " [option]" );
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
