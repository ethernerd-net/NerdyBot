using Discord;
using Discord.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace NerdyBot.Tag
{
  internal class TagCommand
  {
    private void CommandTag( MessageEventArgs msg )
    {
      string[] args = msg.Message.Text.Substring( (cfg.Prefix + "n ").Length ).Split( ' ' );
      switch ( args[0].ToLower() )
      {
      case "create":
        if ( args.Count() >= 4 )
          CreateTag( msg, args );
        else
          throw new ArgumentException( "Invalid parameter count, check help for... guess what?" );
        break;

      case "delete":
        if ( args.Count() == 2 )
          DeleteTag( msg, args );
        else
          throw new ArgumentException( "Invalid parameter count, check help for... guess what?" );
        break;

      case "edit":
        if ( args.Count() >= 4 )
          EditTag( msg, args );
        break;

      case "list":
        ListTag( msg );
        break;

      default:
        if ( args.Count() == 1 )
          SendTag( msg, args );
        else
          throw new ArgumentException( "Invalid parameter count, check help for... guess what?" );
        break;
      }
    }

    private string GetTypeString( TagType type )
    {
      switch ( type )
      {
      case TagType.Text:
        return "T";
      case TagType.Sound:
        return "S";
      case TagType.Picture:
        return "P";
      default:
        throw new ArgumentException( "WTF??!" );
      }
    }

    private void CreateTag( MessageEventArgs msg, string[] args )
    {
      if ( tagcfg.Tags.Exists( t => t.Name == args[1].ToLower() ) )
        throw new ArgumentException( "Tag already exists!" );

      Tag tag = new Tag();
      tag.Name = args[1].ToLower();
      tag.Author = msg.User.ToString();
      tag.CreateDate = DateTime.Now;
      tag.Count = 0;
      tag.Volume = 100;
      tag.Items = new List<string>();

      switch ( args[2].ToLower() )
      {
      case "text":
        tag.Type = TagType.Text;
        AddTextToTag( tag, args.Skip( 3 ).ToArray() );
        break;

      case "sound":
        tag.Type = TagType.Sound;
        AddSoundToTag( tag, args.Skip( 3 ).ToArray() );
        break;

      case "pic":
        tag.Type = TagType.Picture;
        AddPictureToTag( tag, args.Skip( 3 ).ToArray() );
        break;
      default:
        throw new ArgumentException();
      }
      tagcfg.Tags.Add( tag );
      tagcfg.Save( TAGCFG );
      WriteInfo( "Tag '" + tag.Name + "' created!", msg.Channel );
    }

    private void DeleteTag( MessageEventArgs msg, string[] args )
    {
      var tag = tagcfg.Tags.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        throw new ArgumentException( "Tag doesn't exist!" );
      if ( tag.Type == TagType.Sound )
        Directory.Delete( Path.Combine( "sounds", tag.Name ), true );

      tagcfg.Tags.Remove( tag );
      tagcfg.Save( TAGCFG );
      WriteInfo( "Tag '" + tag.Name + "' delete!", msg.Channel );
    }

    private void EditTag( MessageEventArgs msg, string[] args )
    {
      var tag = tagcfg.Tags.FirstOrDefault( t => t.Name == args[1].ToLower() );
      if ( tag == null )
        throw new ArgumentException( "Tag doesn't exist!" );

      string[] entries = args.Skip( 3 ).ToArray();

      switch ( args[2] )
      {
      case "add":
        switch ( tag.Type )
        {
        case TagType.Text:
          AddTextToTag( tag, entries );
          break;
        case TagType.Sound:
          AddSoundToTag( tag, entries );
          break;
        case TagType.Picture:
          AddPictureToTag( tag, entries );
          break;
        default:
          throw new ArgumentException( "WTF?!?!" );
        }
        break;
      case "remove":
        int remCount = RemoveTagEntry( tag, entries );
        WriteInfo( remCount + " / " + entries.Count() + " removed", msg.Channel );
        break;
      case "rename":
        if ( tagcfg.Tags.FirstOrDefault( t => t.Name == entries[0] ) == null )
        {
          if ( tag.Type != TagType.Text )
          {
            string dirName = tag.Type == TagType.Sound ? "sounds" : "pics";
            Directory.Move( Path.Combine( dirName, tag.Name ), Path.Combine( dirName, entries[0] ) );
          }
          tag.Name = entries[0];
        }
        else
          throw new ArgumentException( "Tag name" + entries[0] + "already in use" );
        break;
      case "volume":
        break;
      default:
        throw new ArgumentException( "The option '" + args[2] + "' does not exist!" );
      }
      tagcfg.Save( TAGCFG );
    }

    private void ListTag( MessageEventArgs msg )
    {
      var tagsInOrder = tagcfg.Tags.OrderBy( x => x.Name );
      StringBuilder sb = new StringBuilder( "" );
      if ( tagsInOrder.Count() > 0 )
      {
        char lastHeader = '<';
        foreach ( Tag t in tagsInOrder )
        {
          if ( t.Name[0] != lastHeader )
          {
            if ( lastHeader != '<' )
              sb.Remove( sb.Length - 2, 2 );
            lastHeader = t.Name[0];
            sb.AppendLine();
            sb.AppendLine( "# " + lastHeader + " #" );
          }
          sb.Append( "[" + t.Name + "]" );
          sb.Append( "(" + GetTypeString( t.Type ) + "|" + t.Items.Count() + ")" );
          sb.Append( ", " );
        }
        sb.Remove( sb.Length - 2, 2 );
      }
      WriteBlock( sb.ToString(), "md", msg.Channel );
    }

    private async void SendTag( MessageEventArgs msg, string[] args )
    {
      var tag = tagcfg.Tags.FirstOrDefault( t => t.Name == args[0].ToLower() );
      if ( tag == null )
        throw new ArgumentException( "Tag doesn't exist!" );

      int idx = ( new Random() ).Next( 0, tag.Items.Count() );
      switch ( tag.Type )
      {
      case TagType.Text:
        WriteBlock( tag.Items[idx] );
        break;
      case TagType.Sound:
        SendAudio( await audioSvc.Join( msg.User.VoiceChannel ), tag, idx );
        break;
      case TagType.Picture:
        break;
      default:
        throw new ArgumentException( "WTF?!" );
      }
      tag.Count++;
    }

    private void AddTextToTag( Tag tag, string[] args )
    {
      string text = string.Empty;
      for ( int i = 0; i < args.Count(); i++ )
        text += " " + args[i];

      tag.Items = text.Split( new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries ).ToList();
    }
    private void AddSoundToTag( Tag tag, string[] args )
    {
      string path = Path.Combine( "sounds", tag.Name );
      Directory.CreateDirectory( path );
      for ( int i = 0; i < args.Count(); i++ )
      {
        DownloadAudio( args[i], Path.Combine( path, ( tag.Items.Count + i ) + ".mp3" ) );
        tag.Items.Add( args[i] );
      }
    }
    private void AddPictureToTag( Tag tag, string[] args )
    {
    }
    private int RemoveTagEntry( Tag tag, string[] args )
    {
      int remCount = 0;
      switch ( tag.Type )
      {
      case TagType.Text:
        string text = string.Empty;
        for ( int i = 0; i < args.Count(); i++ )
          text += " " + args[i];

        foreach ( string entry in text.Split( new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries ) )
          if ( tag.Items.Remove( entry ) )
            remCount++;

        break;
      case TagType.Sound:
      case TagType.Picture:
        for ( int i = 0; i < args.Count(); i++ )
          if ( tag.Items.Remove( args[i] ) )
            remCount++;

        break;
      default:
        throw new ArgumentException( "WTF?!?!" );
      }
      return remCount;
    }

    private void DownloadAudio( string url, string outp )
    {
      try
      {
        if ( url.EndsWith( ".mp3" ) )
        {
          Debug( "downloading " + url );
          ( new WebClient() ).DownloadFile( url, outp );
        }
        else
        {
          ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
          startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
          startInfo.FileName = "youtube-dl.exe";
          startInfo.Arguments = "\"" + url + "\" --extract-audio --audio-format mp3 --output \"" + outp + "\"";
          Process.Start( startInfo ).WaitForExit();
        }
      }
      catch ( Exception ex )
      {
        throw new ArgumentException( ex.Message );
      }
    }

    private void SendAudio( IAudioClient vClient, Tag tag, int idx )
    {
      string path = Path.Combine( "sounds", tag.Name, idx + ".mp3" );
      if ( !File.Exists( path ) )
        DownloadAudio( tag.Items[idx], path );


      Debug( "reading " + tag.Name );
      var channelCount = audioSvc.Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
      var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
      using ( var MP3Reader = new Mp3FileReader( path ) ) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
      using ( var resampler = new MediaFoundationResampler( MP3Reader, OutFormat ) ) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
      {
        resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality

        int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
        byte[] buffer = new byte[blockSize];
        int byteCount;

        Debug( "start sending audio: " + tag.Name );
        while ( ( byteCount = resampler.Read( buffer, 0, blockSize ) ) > 0 ) // Read audio into our buffer, and keep a loop open while data is present
        {
          Debug( "byteCount: " + byteCount );
          if ( byteCount < blockSize )
          {
            // Incomplete Frame
            for ( int i = byteCount; i < blockSize; i++ )
              buffer[i] = 0;
          }
          vClient.Send( buffer, 0, blockSize ); // Send the buffer to Discord
        }
        Debug( "finished sending" );
      }
      vClient.Wait();
    }
  }
}
