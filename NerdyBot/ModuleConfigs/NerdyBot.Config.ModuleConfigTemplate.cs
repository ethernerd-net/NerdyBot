using Newtonsoft.Json;
using System.Collections.Generic;

namespace NerdyBot.Config
{
  public class ModuleConfig<T> : BaseModuleConfig
  {
    public ModuleConfig( string key/*, IEnumerable<string> aliases*/ )
      : base( key/*, aliases*/ )
    {
    }

    public List<T> List { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<ModuleConfig<T>>( json );
    }
    protected override void Assign( dynamic conf )
    {
      base.Assign( conf as BaseModuleConfig );
      this.List = conf.List;
    }
  }
}
