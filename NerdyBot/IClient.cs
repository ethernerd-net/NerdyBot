using Discord;

namespace NerdyBot
{
  interface IClient
  {
    Config Config { get; }
    void Log( string text );
    void Write( string info, Channel ch = null );
    void WriteInfo( string info, Channel ch = null );
    void WriteBlock( string info, string highlight = "", Channel ch = null );

    T GetService<T>() where T : class, IService;
  }
}
