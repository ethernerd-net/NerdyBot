using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using NAudio.Wave;
using Discord.Audio;
using Discord.Commands;

namespace NerdyBot.Services
{
  public class AudioService
  {
    private ulong lastChannel = 0;
    private IAudioClient audioClient;
    private AudioOutStream discordStream;


    private MessageService svcMessage;
    private object playing = new object();

    public AudioService( MessageService svcMessage )
    {
      this.svcMessage = svcMessage;
    }

    public bool StopPlaying { get; set; }
    public async Task<byte[]> DownloadAudio( string url )
    {
      string dlFilePath = Download( url );
      if ( string.IsNullOrEmpty( dlFilePath ) )
        throw new Exception( $"Fehler beim Download von {url}" );

      if ( Path.GetExtension( url ) != ".mp3" )
        dlFilePath = Transform( dlFilePath );

      var audioBytes = File.ReadAllBytes( dlFilePath );
      File.Delete( dlFilePath );
      return audioBytes;
    }
    public async Task SendAudio( ICommandContext context, byte[] audio, float volume = 1f )
    {
      var channel = context.Guild.GetVoiceChannelsAsync().Result.FirstOrDefault( vc => vc.GetUserAsync( context.User.Id ).Result != null );

      if ( channel != null )
      {
        //this.svcMessage.Log( "playing " + Path.GetDirectoryName( localPath ), context.User.ToString() );
        if ( lastChannel != channel.Id )
        {
          lastChannel = channel.Id;
          audioClient = await channel.ConnectAsync();

          if ( discordStream != null )
            discordStream.Dispose();
          discordStream = audioClient.CreatePCMStream( AudioApplication.Mixed );
        }

        lock ( playing )
        {
          try
          {
            var OutFormat = new WaveFormat( 48000, 16, 2 ); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using ( var mp3Reader = new Mp3FileReader( new MemoryStream( audio ) ) ) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using ( var resampler = new MediaFoundationResampler( mp3Reader, OutFormat ) ) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
            {
              resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
              int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
              byte[] buffer = new byte[blockSize];
              int byteCount;

              while ( ( byteCount = resampler.Read( buffer, 0, blockSize ) ) > 0 && !StopPlaying ) // Read audio into our buffer, and keep a loop open while data is present
              {
                if ( byteCount < blockSize )
                {
                  // Incomplete Frame
                  for ( int i = byteCount; i < blockSize; i++ )
                    buffer[i] = 0;
                }
                discordStream.Write( ScaleVolume.ScaleVolumeSafeNoAlloc( buffer, volume ), 0, blockSize ); // Send the buffer to Discord
                discordStream.FlushAsync();
              }
            }
          }
          catch ( Exception ex )
          {
            this.svcMessage.Log( ex.Message, "Exception", Discord.LogSeverity.Error );
          }
          StopPlaying = false;
        }
      }
    }

    public async Task LeaveChannel()
    {
      if ( this.audioClient != null )
        await this.audioClient.StopAsync();
    }


    private string Download( string url )
    {
      var tempFileName = Guid.NewGuid();
      Directory.CreateDirectory( "temp" );
      string ext = Path.GetExtension( url );

      this.svcMessage.Log( "downloading " + url );

      if ( ext != string.Empty )
      {
        string tempOut = Path.Combine( "temp", $"{tempFileName}{ext}" );

        if ( !Directory.Exists( Path.GetDirectoryName( tempOut ) ) )
          Directory.CreateDirectory( Path.GetDirectoryName( tempOut ) );

        ( new WebClient() ).DownloadFile( url, tempOut );
      }
      else
      {
        //Externe Prozesse sind böse, aber der kann so viel :S
        //Ich könne allerdings auf die ganzen features verzichten und nen reinen yt dl anbieten
        //https://github.com/flagbug/YoutubeExtractor
        string tempOut = Path.Combine( "temp", $"{tempFileName}.%(ext)s" );
        ProcessStartInfo ytdl = new ProcessStartInfo();
        ytdl.WindowStyle = ProcessWindowStyle.Hidden;
        ytdl.FileName = "ext\\youtube-dl.exe";

        ytdl.Arguments = $"--extract-audio --audio-quality 0 --no-playlist --output \"{tempOut}\" \"{url}\"";
        Process.Start( ytdl ).WaitForExit();
      }
      this.svcMessage.Log( "download complete" );
      return Directory.GetFiles( "temp", $"{tempFileName}.*", SearchOption.TopDirectoryOnly ).FirstOrDefault();
    }
    private string Transform( string inFile )
    {
      string outFile = Path.Combine( Path.GetDirectoryName( inFile ), $"{Path.GetFileNameWithoutExtension( inFile )}.mp3" );

      this.svcMessage.Log( $"converting: {inFile}" );

      ProcessStartInfo ffmpeg = new ProcessStartInfo();
      ffmpeg.WindowStyle = ProcessWindowStyle.Hidden;
      ffmpeg.FileName = "ext\\ffmpeg.exe";

      ffmpeg.Arguments = $"-i {inFile} -f mp3 {outFile}";
      Process.Start( ffmpeg ).WaitForExit();

      File.Delete( inFile );
      this.svcMessage.Log( "conversion complete" );

      return outFile;
    }
  }
}
