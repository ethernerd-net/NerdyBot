using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NerdyBot.Commands.Config;

namespace NerdyBot.Commands
{
  class TranslateCommand : ICommand
  {
    public IEnumerable<string> Aliases
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public string Key
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public List<ulong> RestrictedRoles
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public RestrictType RestrictionType
    {
      get
      {
        throw new NotImplementedException();
      }

      set
      {
        throw new NotImplementedException();
      }
    }

    public Task Execute( MessageEventArgs msg, string[] args, IClient client )
    {
      throw new NotImplementedException();
    }

    public void Init()
    {
      throw new NotImplementedException();
    }
  }
}
