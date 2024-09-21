namespace CurlGenerator;


public class Settings
{
    public bool IsMultiline { get; set; }
    public bool IsLongFormat { get; set; }
    public string LineContinuationCharacter { get; set; } = LineContinuationCharacters.Linux;
    public string QuoteType { get; set; } = QuoteTypes.Single_;
    public int TimeoutInSeconds { get; set; }
    public bool IsRedirect { get; set; }
    public bool IsTrimmed { get; set; }
    public bool IsSilent { get; set; }
    
    // handler related
    public const string CanSend = "canSend";
    //public const string Match = "matchCurl";
    public const string OutputCurl = "outputCurl";
    public const string Expected = "expectedCurl";
}

public class LineContinuationCharacters
{
    public static string OSX = "\\";
    public static string Linux = "\\";
    public static string WindowsCmd = "^";
    public static string Powershell = "`";
}

public class QuoteTypes
{
    public static string Double_ = "\"";
    public static string Single_ = "'";
}

public enum Mode
{
    None,
    FormData,
    FormUrlEncoded,
    Raw,
    Binary,
    GraphQL
}