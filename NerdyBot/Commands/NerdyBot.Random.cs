﻿using Discord;
using NerdyBot.Commands.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NerdyBot.Commands
{
  class RandomTag : ICommand
  {
    private const string CFGPATH = "";
    private const string DEFAULTKEY = "random";
    private static readonly string[] DEFAULTALIASES = new string[] { "rnd", "rand" };

    private BaseCommandConfig conf;

    #region ICommand
    public string Key { get { return this.conf.Key; } }
    public IEnumerable<string> Aliases { get { return this.conf.Aliases; } }
    public List<ulong> RestrictedRoles { get { return this.conf.RestrictedRoles; } }
    public RestrictType RestrictionType { get { return this.conf.RestrictionType; } set { this.conf.RestrictionType = value; } }

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
            string text = EntityToUnicode( quote.content ).Replace( "<p>", " " ).Replace( "</p>", " " );
            client.Write( text + Environment.NewLine + Environment.NewLine + "-" + quote.title, msg.Channel );
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

    public void Init()
    {
      this.conf = new BaseCommandConfig( CFGPATH, DEFAULTKEY, DEFAULTALIASES );
      //this.conf.Read();
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
    #endregion
  }
}
