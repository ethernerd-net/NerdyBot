using Discord;

namespace NerdyBot
{
  interface IClient
  {
    MainConfig Config { get; }
    void Log( string text );
    void Write( string info, Channel ch = null );
    void WriteInfo( string info, Channel ch = null );
    void WriteBlock( string info, string highlight = "", Channel ch = null );

    void DownloadAudio( string url, string outp );
    void SendAudio( Channel vChannel, string localPath );
    bool StopPlaying { get; set; }

    T GetService<T>() where T : class, IService;
  }
}
