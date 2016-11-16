using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NerdyBot.Commands.Config;

namespace NerdyBot.Commands
{
  class Ball8 : ICommand
  {
    private const string CFGPATH = "8ball.json";
    private const string DEFAULTKEY = "8ball";
    private static readonly string[] DEFAULTALIASES = new string[] { "8b" };

    private TagCommandConfig<string> conf;

    #region ICommand
    public string Key { get { return this.conf.Key; } }
    public IEnumerable<string> Aliases { get { return this.conf.Aliases; } }
    public List<ulong> RestrictedRoles { get { return this.conf.RestrictedRoles; } }
    public RestrictType RestrictionType { get { return this.conf.RestrictionType; } set { this.conf.RestrictionType = value; } }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      return Task.Factory.StartNew( () =>
      {
        if ( args[0] == "add" && msg.User.ServerPermissions.Administrator )
        {
          string answer = string.Join( " ", args.Skip( 1 ) );
          this.conf.Items.Add( answer );
          this.conf.Write();
        }
        else
        {
          int idx = ( new Random() ).Next( 0, this.conf.Items.Count() );
          client.Write( msg.User.Mention + " asked: '" + string.Join( " ", args ) +
            Environment.NewLine + Environment.NewLine +
            "My answer is: " + this.conf.Items[idx], msg.Channel );
        }
      } );
    }

    public void Init()
    {
      this.conf = new TagCommandConfig<string>( CFGPATH, DEFAULTKEY, DEFAULTALIASES );
      this.conf.Read();
    }
    #endregion ICommand
  }
}