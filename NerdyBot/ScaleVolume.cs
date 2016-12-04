using System;
using System.Diagnostics.Contracts;

namespace NerdyBot
{
  public static class ScaleVolume
  {
    public static byte[] ScaleVolumeSafeAllocateBuffers( byte[] audioSamples, float volume )
    {
      Contract.Requires( audioSamples != null );
      Contract.Requires( audioSamples.Length % 2 == 0 );
      Contract.Requires( volume >= 0f && volume <= 1f );

      var output = new byte[audioSamples.Length];
      if ( Math.Abs( volume - 1f ) < 0.0001f )
      {
        Buffer.BlockCopy( audioSamples, 0, output, 0, audioSamples.Length );
        return output;
      }

      // 16-bit precision for the multiplication
      int volumeFixed = ( int )Math.Round( volume * 65536d );

      for ( var i = 0; i < output.Length; i += 2 )
      {
        // The cast to short is necessary to get a sign-extending conversion
        int sample = ( short )( ( audioSamples[i + 1] << 8 ) | audioSamples[i] );
        int processed = ( sample * volumeFixed ) >> 16;

        output[i] = ( byte )processed;
        output[i + 1] = ( byte )( processed >> 8 );
      }

      return output;
    }

    public static byte[] ScaleVolumeSafeNoAlloc( byte[] audioSamples, float volume )
    {
      Contract.Requires( audioSamples != null );
      Contract.Requires( audioSamples.Length % 2 == 0 );
      Contract.Requires( volume >= 0f && volume <= 1f );

      if ( Math.Abs( volume - 1f ) < 0.0001f )
        return audioSamples;

      // 16-bit precision for the multiplication
      int volumeFixed = ( int )Math.Round( volume * 65536d );

      for ( int i = 0, length = audioSamples.Length; i < length; i += 2 )
      {
        // The cast to short is necessary to get a sign-extending conversion
        int sample = ( short )( ( audioSamples[i + 1] << 8 ) | audioSamples[i] );
        int processed = ( sample * volumeFixed ) >> 16;

        audioSamples[i] = ( byte )processed;
        audioSamples[i + 1] = ( byte )( processed >> 8 );
      }

      return audioSamples;
    }
  }
}
