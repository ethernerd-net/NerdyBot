using Newtonsoft.Json;
using System.Collections.Generic;

namespace NerdyBot.Commands.Config
{
  public class BaseCommandConfig : BaseConfig
  {
    public BaseCommandConfig( string filePath, string defaultKey, params string[] aliases )
      :base( filePath )
    {      
      this.Key = defaultKey;
      this.Aliases = aliases;
      this.RestrictedRoles = new List<ulong>();
      this.RestrictionType = RestrictType.None;
    }

    public string Key { get; private set; }
    public IEnumerable<string> Aliases { get; private set; }
    public List<ulong> RestrictedRoles { get; private set; }
    public RestrictType RestrictionType { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<BaseCommandConfig>( json );
    }
    protected override void Assign( dynamic conf )
    {
      this.Key = conf.Key;
      this.Aliases = conf.Aliases;
      this.RestrictedRoles = conf.RestrictedRoles;
      this.RestrictionType = conf.RestrictionType;
    }
  }
}
