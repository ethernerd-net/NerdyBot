using Newtonsoft.Json;
using System.Collections.Generic;

namespace NerdyBot.Config
{
  public class BaseCommandConfig : BaseConfig
  {
    public BaseCommandConfig( string defaultKey/*, IEnumerable<string> aliases = null*/ )
      :base( defaultKey )
    {      
      this.Key = defaultKey;
      //this.Aliases = aliases ?? new List<string>();
      this.RestrictedRoles = new List<ulong>();
      this.RestrictionType = RestrictType.None;
    }

    public string Key { get; set; }
    //public IEnumerable<string> Aliases { get; set; }
    public List<ulong> RestrictedRoles { get; set; }
    public RestrictType RestrictionType { get; set; }
    public string ApiKey { get; set; }
    public long Hits { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<BaseCommandConfig>( json );
    }
    protected override void Assign( dynamic conf )
    {
      this.Key = conf.Key;
      //this.Aliases = conf.Aliases;
      this.RestrictedRoles = conf.RestrictedRoles;
      this.RestrictionType = conf.RestrictionType;
      this.ApiKey = conf.ApiKey;
      this.Hits = conf.Hits;
    }
  }
  public enum RestrictType { None = 0, Allow = 1, Deny = 2, Admin = 3 }
}
