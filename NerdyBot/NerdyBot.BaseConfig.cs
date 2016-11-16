using Newtonsoft.Json;
using System.IO;

namespace NerdyBot
{
  public abstract class BaseConfig
  {
    private string filePath;
    public BaseConfig( string fileName )
    {
      if ( fileName != null )
        this.filePath = Path.Combine( "conf", fileName );
    }
    private object lck = new object();
    public void Write()
    {
      lock ( lck )
      {
        File.WriteAllText( this.filePath, JsonConvert.SerializeObject( this ) );
      }
    }
    public void Read()
    {
      if ( File.Exists( filePath ) )
        Assign( Parse( File.ReadAllText( this.filePath ) ) );
    }

    protected abstract dynamic Parse( string json );
    protected abstract void Assign( dynamic conf );
  }
}