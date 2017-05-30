using Discord;

namespace NerdyBot.Contracts
{
  interface IClient
  {
    void Log( string text, string source = "", LogSeverity logLevel = LogSeverity.Info );
    void SendMessage( string message, SendMessageOptions options );

    void DownloadAudio( string url, string outp );
    void SendAudio( ICommandUser user, string localPath, float volume = 1f, bool delAfterPlay = false );
    bool StopPlaying { get; set; }
  }

  public class SendMessageOptions
  {
    public SendMessageOptions()
    {
      this.TargetId = 0;
      this.TargetType = TargetType.Default;
      this.Split = false;
      this.MessageType = MessageType.Normal;
      this.Hightlight = string.Empty;
    }
    public static SendMessageOptions Default { get { return new SendMessageOptions(); } }

    public ulong TargetId { get; set; }
    public TargetType TargetType { get; set; }
    public bool Split { get; set; }
    public MessageType MessageType { get; set; }
    public string Hightlight { get; set; }
  }
  public enum TargetType { User = 0, Channel = 1, Default = 2 }
  public enum MessageType { Block, Info, Normal }
}
