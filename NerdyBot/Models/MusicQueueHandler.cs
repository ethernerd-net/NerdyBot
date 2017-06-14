using System.Collections.Concurrent;

namespace NerdyBot.Models
{
  public class MusicModuleHandler
  {
    private static MusicModuleHandler queueHandler;

    
    private ConcurrentDictionary<ulong, ConcurrentQueue<MusicPlaylistEntry>> queues = new ConcurrentDictionary<ulong, ConcurrentQueue<MusicPlaylistEntry>>();
    public ConcurrentDictionary<ulong, ConcurrentQueue<MusicPlaylistEntry>> Queues => queues;
    public int Volume { get; set; }

    private MusicModuleHandler()
    {
    }

    public static MusicModuleHandler Handler
    {
      get
      {
        if ( queueHandler == null )
          queueHandler = new MusicModuleHandler();
        return queueHandler;
      }
    }
  }
}
