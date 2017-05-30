using Newtonsoft.Json;

namespace NerdyBot.Config
{
  class MainConfig : BaseConfig
  {
    public MainConfig( string fileName ) 
      : base( fileName )
    {
    }

    public string Token { get; set; }
    public char Prefix { get; set; }
    public string BackUpApiKey { get; set; }
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
