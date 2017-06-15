namespace NerdyBot.Models
{
  public class BotConfig
  {
    public char PrefixChar { get; set; }
    public string DiscordToken { get; set; }
    public string YoutubeAppName { get; set; }
    public string YoutubeAPIKey { get; set; }
    public string ImgurClientId { get; set; }
    public string GeniusAccessToken { get; set; }
    public string CleverBotApiKey { get; set; }
    public string MyAnimeListUsr { get; set; }
    public string MyAnimeListPwd { get; set; }
    public ulong AdminUserId { get; set; }
  }
}
