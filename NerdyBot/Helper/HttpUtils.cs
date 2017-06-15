using System;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NerdyBot.Helper
{
  public class HttpUtils
  {
    public static string EntityToUnicode( string html )
    {
      var replacements = new Dictionary<string, string>();
      var regex = new Regex( "(&[a-z,0-9,#]{2,6};)" );
      foreach ( Match match in regex.Matches( html ) )
      {
        if ( !replacements.ContainsKey( match.Value ) )
        {
          var unicode = WebUtility.HtmlDecode( match.Value );
          if ( unicode.Length == 1 )
            replacements.Add( match.Value, unicode );
        }
      }
      foreach ( var replacement in replacements )
        html = html.Replace( replacement.Key, replacement.Value );
      return html;
    }
    public static string StripHTML( string input )
    {
      return Regex.Replace( input, "<.*?>", String.Empty );
    }
  }
}
