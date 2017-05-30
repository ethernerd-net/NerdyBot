using Newtonsoft.Json;
using System.IO;

namespace NerdyBot.Config
{
  public abstract class BaseConfig
  {
    private string filePath;
    public BaseConfig( string fileName )
    {
      if ( fileName != null )
        this.filePath = Path.Combine( "conf", fileName + ".json" );
    }
    public string FilePath { get { return this.filePath; } }
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
      else
        Write();
    }

    protected abstract dynamic Parse( string json );
    protected abstract void Assign( dynamic conf );
  }
}