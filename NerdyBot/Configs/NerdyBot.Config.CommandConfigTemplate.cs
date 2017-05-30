using Newtonsoft.Json;

namespace NerdyBot.Config
{
  public class CommandConfig<T> : BaseCommandConfig where T : new()
  {
    public CommandConfig( string key/*, IEnumerable<string> aliases*/ )
      : base( key/*, aliases*/ )
    {
      this.Ext = new T();
    }

    public T Ext { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<CommandConfig<T>>( json );
    }
    protected override void Assign( dynamic conf )
    {
      base.Assign( conf as BaseCommandConfig );
      this.Ext = conf.Ext;
    }
  }
}
