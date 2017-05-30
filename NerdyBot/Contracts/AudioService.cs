using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using Discord.Audio;
using Discord.Commands;
using NAudio.Wave;

namespace NerdyBot.Contracts
{
  public class AudioService
  {
    private ulong lastChannel = 0;
    private IAudioClient audioClient;
    private object playing = new object();

    public bool StopPlaying { get; set; }
    public Task DownloadAudio( string url, string outp )
    {
      return Task.Run( () =>
      {
        try
        {
          bool transform = false;
          string ext = Path.GetExtension( url );
          //Log( "downloading " + url );
          if ( ext != string.Empty )
          {
            string tempOut = Path.Combine( Path.GetDirectoryName( outp ), "temp" + ext );
            if ( ext == ".mp3" )
              tempOut = outp;

            if ( !Directory.Exists( Path.GetDirectoryName( tempOut ) ) )
              Directory.CreateDirectory( Path.GetDirectoryName( tempOut ) );

            ( new WebClient() ).DownloadFile( url, tempOut );

            transform = ( ext != ".mp3" );
          }
          else
          {
            //Externe Prozesse sind böse, aber der kann so viel :S
            //Ich könne allerdings auf die ganzen features verzichten und nen reinen yt dl anbieten
            //https://github.com/flagbug/YoutubeExtractor
            string tempOut = Path.Combine( Path.GetDirectoryName( outp ), "temp.%(ext)s" );
            ProcessStartInfo ytdl = new ProcessStartInfo();
            ytdl.WindowStyle = ProcessWindowStyle.Hidden;
            ytdl.FileName = "ext\\youtube-dl.exe";

            ytdl.Arguments = "--extract-audio --audio-quality 0 --no-playlist --output \"" + tempOut + "\" \"" + url + "\"";
            Process.Start( ytdl ).WaitForExit();
            transform = true;
          }
          //Log( "download complete" );
          if ( transform )
          {
            string tempFIle = Directory.GetFiles( Path.GetDirectoryName( outp ), "temp.*", SearchOption.TopDirectoryOnly ).First();
            //Log( "converting: " + Path.GetFileName( tempFIle ) );

            ProcessStartInfo ffmpeg = new ProcessStartInfo();
            ffmpeg.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpeg.FileName = "ext\\ffmpeg.exe";

            ffmpeg.Arguments = "-i " + tempFIle + " -f mp3 " + outp;
            Process.Start( ffmpeg ).WaitForExit();

            File.Delete( tempFIle );
            //Log( "conversion complete" );
          }
        }
        catch ( Exception ex )
        {
          throw new ArgumentException( ex.Message );
        }
      } );
    }
    public async void SendAudio( ICommandContext context, string localPath, float volume = 1f, bool delAfterPlay = false )
    {
      var channel = context.Guild.GetVoiceChannelsAsync().Result.FirstOrDefault( vc => vc.GetUserAsync( context.User.Id ).Result != null );

      if ( channel != null )
      {
        //Log( "playing " + Path.GetDirectoryName( localPath ), vUser.ToString() );
        if ( channel.Id != lastChannel )
        {
          audioClient = await channel.ConnectAsync();
          lastChannel = channel.Id;
        }
        lock ( playing )
        {
          try
          {
            var discord = audioClient.CreatePCMStream( AudioApplication.Mixed );
            var OutFormat = new WaveFormat( 48000, 16, 2 ); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            using ( var MP3Reader = new Mp3FileReader( localPath ) ) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
            using ( var resampler = new MediaFoundationResampler( MP3Reader, OutFormat ) ) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
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
                discord.Write( ScaleVolume.ScaleVolumeSafeNoAlloc( buffer, volume ), 0, blockSize ); // Send the buffer to Discord
              }
            }
            discord.FlushAsync();
          }
          catch ( Exception ex )
          {
            Console.WriteLine( ex.Message );
          }
          StopPlaying = false;
          if ( delAfterPlay )
            File.Delete( localPath );
        }
      }
    }
    public async void LeaveChannel()
    {
      if ( this.audioClient != null )
        await this.audioClient.StopAsync();
    }
  }
}
