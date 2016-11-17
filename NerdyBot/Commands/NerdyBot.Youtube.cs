﻿using Discord;
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
    private const string CFGPATH = "youtube.json";
    private const string DEFAULTKEY = "youtube";
    private static readonly string[] DEFAULTALIASES = new string[] { "yt" };

    private BaseCommandConfig conf;

    #region ICommand
    public string Key { get { return this.conf.Key; } }
    public IEnumerable<string> Aliases { get { return this.conf.Aliases; } }
    public List<ulong> RestrictedRoles { get { return this.conf.RestrictedRoles; } }
    public RestrictType RestrictionType { get { return this.conf.RestrictionType; } set { this.conf.RestrictionType = value; } }

    public void Init()
    {
      this.conf = new TagCommandConfig<string>( CFGPATH, DEFAULTKEY, DEFAULTALIASES );
      this.conf.Read();
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( () =>
      {
        var youtubeService = new YouTubeService( new BaseClientService.Initializer()
        {
          ApiKey = "AIzaSyAmrg8abuMO0esvieSZCdduxqog815QRnY",
          ApplicationName = this.GetType().ToString()
        } );

        if ( args[0] == "help" )
        {
          StringBuilder sb = new StringBuilder();
          sb.Append( QuickHelp() );
          sb.AppendLine( "Aliase: " + string.Join( " | ", this.conf.Aliases ) );
          sb.AppendLine();
          sb.AppendLine( "Beispiel: " + client.Config.Prefix + this.conf.Key + " [KEYWORDS]" );
          msg.User.SendMessage( "```" + sb.ToString() + "```" );
        }
      } );
    }

    public string QuickHelp()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine( "======== Youtube ========" );
      sb.AppendLine();
      sb.AppendLine( "Der Youtube Befehlt ermöglicht eine 'Auf gut Glück'-Suche." );
      return sb.ToString();
    }
    #endregion ICommand
  }
}