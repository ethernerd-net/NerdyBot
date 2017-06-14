using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Reflection;

namespace Cleverbot.Net
{
  public class CleverbotResponse
  {
    #region internals
    /*
     Json.NET automatically detects names using underscores and camelCase, so no need for unnecessary attributes ;)

     Oh, I didn't know this! 

     It couldn't find output or conversationid :(
    */

    [JsonProperty( "interaction_count" )]
    internal string interactionCount { get; set; }

    [JsonProperty( "input" )]
    internal string InputMessage { get; set; }

    [JsonProperty( "predicted_input" )]
    // ("predicted_input")]
    internal string PredictedInputMessage { get; set; }

    [JsonProperty( "accuracy" )]
    internal string Accuracy { get; set; }

    [JsonProperty( "output_label" )]
    internal string outputLabel { get; set; }

    [JsonProperty( "output" )]
    internal string Output { get; set; }

    [JsonProperty( "conversation_id" )]
    internal string ConvId { get; set; }

    [JsonProperty( "errorline" )]
    internal string ErrorLine { get; set; }

    [JsonProperty( "database_version" )]
    internal string DatabaseVersion { get; set; }

    [JsonProperty( "software_version" )]
    internal string SoftwareVersion { get; set; }

    [JsonProperty( "time_taken" )]
    internal string TimeTaken { get; set; }

    [JsonProperty( "random_number" )]
    internal string RandomNumber { get; set; }

    [JsonProperty( "time_second" )]
    internal string TimeSeconds { get; set; }

    [JsonProperty( "time_minute" )]
    internal string TimeMinutes { get; set; }

    [JsonProperty( "time_hour" )]
    internal string TimeHours { get; set; }

    [JsonProperty( "time_day_of_week" )]
    internal string TimeDayOfWeek { get; set; }

    [JsonProperty( "time_day" )]
    internal string TimeDays { get; set; }

    [JsonProperty( "time_month" )]
    internal string TimeMonths { get; set; }

    [JsonProperty( "time_year" )]
    internal string TimeYears { get; set; }

    [JsonProperty( "reaction" )]
    internal string Reaction { get; set; }

    [JsonProperty( "reaction_tone" )]
    internal string ReactionTone { get; set; }

    [JsonProperty( "emotion" )]
    internal string Emotion { get; set; }

    [JsonProperty( "emotion_tone" )]
    internal string EmotionTone { get; set; }

    [JsonProperty("clever_accuracy")]
    internal string CleverAccuracy { get; set; }

    [JsonProperty( "clever_output" )]
    internal string CleverOutput { get; set; }

    [JsonProperty( "clever_match" )]
    internal string CleverMatch { get; set; }

    [JsonProperty( "time_elapsed" )]
    internal string TimeElapsed { get; set; }

    [JsonProperty( "filtered_input" )]
    internal string FilteredInput { get; set; }

    [JsonProperty( "reaction_degree" )]
    internal string ReactionDegree { get; set; }

    [JsonProperty( "emotion_degree" )]
    internal string EmotionDegree { get; set; }

    [JsonProperty( "reaction_values" )]
    internal string ReactionValues { get; set; }

    [JsonProperty( "emotion_values" )]
    internal string EmotionValues { get; set; }

    [JsonProperty( "callback" )]
    internal string Callback { get; set; }

    // TODO: convince Rollo to make these a array/list in json
    // It failed
    [JsonProperty( "interaction_1" )]
    internal string Interaction1 { get; set; }

    [JsonProperty( "interaction_2" )]
    internal string Interaction2 { get; set; }

    [JsonProperty( "interaction_3" )]
    internal string Interaction3 { get; set; }

    [JsonProperty( "interaction_4" )]
    internal string Interaction4 { get; set; }

    [JsonProperty( "interaction_5" )]
    internal string Interaction5 { get; set; }

    [JsonProperty( "interaction_6" )]
    internal string Interaction6 { get; set; }

    [JsonProperty( "interaction_7" )]
    internal string Interaction7 { get; set; }

    [JsonProperty( "interaction_8" )]
    internal string Interaction8 { get; set; }

    [JsonProperty( "interaction_9" )]
    internal string Interaction9 { get; set; }

    [JsonProperty( "interaction_10" )]
    internal string Interaction10 { get; set; }

    [JsonProperty( "interaction_11" )]
    internal string Interaction11 { get; set; }

    [JsonProperty( "interaction_12" )]
    internal string Interaction12 { get; set; }

    [JsonProperty( "interaction_13" )]
    internal string Interaction13 { get; set; }

    [JsonProperty( "interaction_14" )]
    internal string Interaction14 { get; set; }

    [JsonProperty( "interaction_15" )]
    internal string Interaction15 { get; set; }

    [JsonProperty( "interaction_16" )]
    internal string Interaction16 { get; set; }

    [JsonProperty( "interaction_17" )]
    internal string Interaction17 { get; set; }

    [JsonProperty( "interaction_18" )]
    internal string Interaction18 { get; set; }

    [JsonProperty( "interaction_19" )]
    internal string Interaction19 { get; set; }

    [JsonProperty( "interaction_20" )]
    internal string Interaction20 { get; set; }

    [JsonProperty( "interaction_21" )]
    internal string Interaction21 { get; set; }

    [JsonProperty( "interaction_22" )]
    internal string Interaction22 { get; set; }

    [JsonProperty( "interaction_23" )]
    internal string Interaction23 { get; set; }

    [JsonProperty( "interaction_24" )]
    internal string Interaction24 { get; set; }

    [JsonProperty( "interaction_25" )]
    internal string Interaction25 { get; set; }

    [JsonProperty( "interaction_26" )]
    internal string Interaction26 { get; set; }

    [JsonProperty( "interaction_27" )]
    internal string Interaction27 { get; set; }

    [JsonProperty( "interaction_28" )]
    internal string Interaction28 { get; set; }

    [JsonProperty( "interaction_29" )]
    internal string Interaction29 { get; set; }

    [JsonProperty( "interaction_30" )]
    internal string Interaction30 { get; set; }

    [JsonProperty( "interaction_31" )]
    internal string Interaction31 { get; set; }

    [JsonProperty( "interaction_32" )]
    internal string Interaction32 { get; set; }

    [JsonProperty( "interaction_33" )]
    internal string Interaction33 { get; set; }

    [JsonProperty( "interaction_34" )]
    internal string Interaction34 { get; set; }

    [JsonProperty( "interaction_35" )]
    internal string Interaction35 { get; set; }

    [JsonProperty( "interaction_36" )]
    internal string Interaction36 { get; set; }

    [JsonProperty( "interaction_37" )]
    internal string Interaction37 { get; set; }

    [JsonProperty( "interaction_38" )]
    internal string Interaction38 { get; set; }

    [JsonProperty( "interaction_39" )]
    internal string Interaction39 { get; set; }

    [JsonProperty( "interaction_40" )]
    internal string Interaction40 { get; set; }

    [JsonProperty( "interaction_41" )]
    internal string Interaction41 { get; set; }

    [JsonProperty( "interaction_42" )]
    internal string Interaction42 { get; set; }

    [JsonProperty( "interaction_43" )]
    internal string Interaction43 { get; set; }

    [JsonProperty( "interaction_44" )]
    internal string Interaction44 { get; set; }

    [JsonProperty( "interaction_45" )]
    internal string Interaction45 { get; set; }

    [JsonProperty( "interaction_46" )]
    internal string Interaction46 { get; set; }

    [JsonProperty( "interaction_47" )]
    internal string Interaction47 { get; set; }

    [JsonProperty( "interaction_48" )]
    internal string Interaction48 { get; set; }

    [JsonProperty( "interaction_49" )]
    internal string Interaction49 { get; set; }

    [JsonProperty( "interaction_50" )]
    internal string Interaction50 { get; set; }

    [JsonProperty( "interaction_1_other" )]
    internal string Interaction1other { get; set; }

    internal List<string> allInteractions = new List<string>();

    #endregion

    /// <summary>
    /// Id to keep track of the conversation
    /// </summary>
    public string ConversationId => ConvId;

    /// <summary>
    /// Cleverbot's response message
    /// </summary>
    public string Response => Output;

    /// <summary>
    /// The user's latest message
    /// </summary>
    public string Input => InputMessage;

    private string apiKey;

    internal static async Task<CleverbotResponse> CreateAsync( string message, string conversationId, string apiKey )
    {
      HttpClient c = new HttpClient();

      string conversationLine = ( string.IsNullOrWhiteSpace( conversationId ) ? "" : $"&cs={conversationId}" );

      byte[] bytesReceived = await c.GetByteArrayAsync( $"https://www.cleverbot.com/getreply?key={ apiKey }&wrapper=cleverbot.net&input={ message }{ conversationLine }" ).ConfigureAwait( false );

      if ( bytesReceived == null )
        return null;
      string result = Encoding.UTF8.GetString( bytesReceived, 0, bytesReceived.Length );
      CleverbotResponse response = JsonConvert.DeserializeObject<CleverbotResponse>( result );
      if ( response == null )
        return null;
      response.apiKey = apiKey;
      response.CreateInteractionsList();

      return response;
    }

    /*internal static async Task<CleverbotResponse> CreateAsync(string message, string conversationId, string apiKey)
        => await Create(message, conversationId, apiKey);*/

    internal void CreateInteractionsList()
    {
      foreach ( var item in GetType().GetTypeInfo().DeclaredFields )
      {
        if ( item.Name.StartsWith( "interaction" ) )
        {
          if ( string.IsNullOrWhiteSpace( ( string )item.GetValue( this ) ) )
          {
            allInteractions.Add( item.GetValue( this ) as string );
          }
        }
      }
    }

    public CleverbotResponse Respond( string text )
    {
      return CreateAsync( text, ConversationId, apiKey ).GetAwaiter().GetResult();
    }

    public Task<CleverbotResponse> RespondAsync( string text )
    {
      return CreateAsync( text, ConversationId, apiKey );
    }


  }
}
