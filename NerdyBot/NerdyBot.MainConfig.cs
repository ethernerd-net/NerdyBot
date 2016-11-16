using Newtonsoft.Json;

namespace NerdyBot
{
  class MainConfig : BaseConfig
  {
    public MainConfig( string filePath ) 
      : base( filePath )
    {
    }

    public string Token { get; set; }
    public char Prefix { get; set; }
    public ulong ResponseChannel { get; set; }

    protected override dynamic Parse( string json )
    {
      return JsonConvert.DeserializeObject<MainConfig>( json );
    }

    protected override void Assign( dynamic conf )
    {
      this.Token = conf.Token;
      this.Prefix = conf.Prefix;
      this.ResponseChannel = conf.ResponseChannel;
    }
  }
}
