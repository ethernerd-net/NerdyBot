using Discord.Commands;
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
  [Group( "random" ), Alias( "rnd", "rand" )]
  public class RandomTag : ModuleBase
  {
    private MessageService svcMessage;

    #region ICommand
    public RandomTag( MessageService svcMessage )
    {
      this.svcMessage = svcMessage;
    }

    [Command( "cat" )]
    public async Task Cat()
    {
        string catJson = ( new WebClient() ).DownloadString( "http://random.cat/meow" );
        var cat = JsonConvert.DeserializeObject<RandomCat>( catJson );
        this.svcMessage.SendMessage( Context, cat.file.Replace( "\\", "" ),
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
    }
    [Command( "penguin" )]
    public Task Penguin()
    {
      return Task.Factory.StartNew( () =>
      {
        string pengu = ( new WebClient() ).DownloadString( "http://penguin.wtf/" );
        this.svcMessage.SendMessage( Context, pengu,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      } );
    }
    [Command( "bunny" )]
    public Task Bunny()
    {
      return Task.Factory.StartNew( () =>
      {
        string bunnyJson = ( new WebClient() ).DownloadString( "https://api.bunnies.io/v2/loop/random/?media=gif" );
        var bunny = JsonConvert.DeserializeObject<RandomBunny>( bunnyJson );
        this.svcMessage.SendMessage( Context, bunny.media.gif,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      } );
    }
    [Command( "chuck" )]
    public Task Chuck()
    {
      return Task.Factory.StartNew( () =>
      {
        string chuckJson = ( new WebClient() ).DownloadString( "http://api.icndb.com/jokes/random" );
        var chuck = JsonConvert.DeserializeObject<RandomChuck>( chuckJson );
        this.svcMessage.SendMessage( Context, chuck.value.joke,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      } );
    }
    [Command( "joke" )]
    public Task Joke()
    {
      return Task.Factory.StartNew( () =>
      {
        string jokeJson = ( new WebClient() ).DownloadString( "http://tambal.azurewebsites.net/joke/random" );
        var joke = JsonConvert.DeserializeObject<ChuckJoke>( jokeJson );
        this.svcMessage.SendMessage( Context, joke.joke,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      } );
    }
    [Command( "yomomma" )]
    public Task Yomomma()
    {
      return Task.Factory.StartNew( () =>
      {
        string momJson = ( new WebClient() ).DownloadString( "http://api.yomomma.info/" );
        var mom = JsonConvert.DeserializeObject<ChuckJoke>( momJson );
        this.svcMessage.SendMessage( Context, mom.joke,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      } );
    }

    [Command( "quote" )]
    public Task Quote()
    {
      return Task.Factory.StartNew( () =>
      {
        string quoteJson = ( new WebClient() ).DownloadString( "http://quotesondesign.com/wp-json/posts?filter[orderby]=rand" );
        var quote = JsonConvert.DeserializeObject<List<RandomQuote>>( quoteJson ).First();
        string text = StripHTML( EntityToUnicode( quote.content ) );
        this.svcMessage.SendMessage( Context, text + Environment.NewLine + Environment.NewLine + "-" + quote.title,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id, MessageType = MessageType.Block } );
      } );
    }

    [Command( "trump" )]
    public Task Trump()
    {
      return Task.Factory.StartNew( () =>
      {
        string trumpJson = ( new WebClient() ).DownloadString( "https://api.whatdoestrumpthink.com/api/v1/quotes/random" );
        var trump = JsonConvert.DeserializeObject<TrumpQuote>( trumpJson );
        this.svcMessage.SendMessage( Context, "Trump : " + trump.message,
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      } );
    }

    [Command( "xkcd" )]
    public Task xkcd()
    {
      return Task.Factory.StartNew( () =>
      {
        string xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/info.0.json" );
        var xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );

        xkcdJson = ( new WebClient() ).DownloadString( "https://xkcd.com/" + ( new Random() ).Next( xkcd.num ) + "/info.0.json" );
        xkcd = JsonConvert.DeserializeObject<RandomXKCD>( xkcdJson );
        this.svcMessage.SendMessage( Context, xkcd.img.Replace( "\\", "" ),
          new SendMessageOptions() { TargetType = TargetType.Channel, TargetId = Context.Channel.Id } );
      } );
    }

    [Command( "help" )]
    public Task help()
    {
      return Task.Factory.StartNew( () =>
      {
        this.svcMessage.SendMessage( Context, FullHelp(),
        new SendMessageOptions() { TargetType = TargetType.User, TargetId = Context.User.Id, MessageType = MessageType.Block } );
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== RANDOM ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Random Command gibt, je nach Sub-Parameter, einen zufälligen Output zurück." );
      sb.AppendLine( "Key: random" );
      return sb.ToString();
    }
    public string FullHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append( QuickHelp() );
      sb.AppendLine( "Aliase: rnd | rand" );
      sb.AppendLine();
      sb.AppendLine( "random [option]" );
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
