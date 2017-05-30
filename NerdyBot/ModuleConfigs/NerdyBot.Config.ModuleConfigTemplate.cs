using Newtonsoft.Json;

namespace NerdyBot.Config
{
  public class ModuleConfig<T> : BaseModuleConfig where T : new()
  {
    public ModuleConfig( string key/*, IEnumerable<string> aliases*/ )
      : base( key/*, aliases*/ )
    {
      this.Ext = new T();
    }

    public T Ext { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<ModuleConfig<T>>( json );
    }
    protected override void Assign( dynamic conf )
    {
      base.Assign( conf as BaseModuleConfig );
      this.Ext = conf.Ext;
    }
  }
}
