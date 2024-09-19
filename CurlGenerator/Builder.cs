using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CurlGenerator;

public interface IBuilder
{
    void AddHeaders(Dictionary<string,string> headers);
    void AddBody(string mode, string content);
    void AddMethod(string method, bool isBodyEmpty);
    void AddUrl(string url);

    string GetSnippet();
}


public class Builder : IBuilder
{
    private StringBuilder _snippet;
    private string _indent;
    private Settings _settings;
    private string _quoteType;

    public Builder(Settings settings)
    {
        _settings = settings;
        Reset();
    }

    public void Reset()
    {
        _snippet = new StringBuilder("curl");
        if (_settings.IsSilent)
        {
            _snippet.Append(Format(Format("-s")));
        }

        if (_settings.IsRedirect)
        {
            _snippet.Append(Format(Format("-L")));
        }
        
        //timeout
        if (_settings.TimeoutInSeconds > 0)
        {
            _snippet.Append(Format(Format("-m"))).Append($" {_settings.TimeoutInSeconds}");
        }
        //globof

        if (_settings.IsMultiline)
        {
            char tab = ' '; // \t ;  get from indentType
            int indentCount = 1; // 
            string indent = new string(tab, indentCount);
            _indent = $" {_settings.LineContinuationCharacter} \n {indent}";
        }
        else
        {
            _indent = " ";
        }

        _quoteType = _settings.QuoteType == QuoteTypes.Single_ ? "'" : "\"";
    }

    public void AddHeaders(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            if (string.IsNullOrEmpty(header.Key))
                return;
            // If the header value is an empty string then add a semicolon after key
            // otherwise the header would be ignored by curl
            _snippet.Append(_indent).Append($"{Format("-H")} {_settings.QuoteType}{Sanitize(header.Key)}");
            if (string.IsNullOrEmpty(header.Value))
            {
                _snippet.Append(";").Append(_settings.QuoteType);
            }
            else
            {
                _snippet.Append(": ").Append(Sanitize(header.Value))
                    .Append(_settings.QuoteType);
            }
        }
    }
    

    public void AddBody(string mode, string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            switch (mode)
            {
                case "raw":
                    bool isAsperandFound = content.Contains('@');
                    string optionName = isAsperandFound ? "--data-raw" : Format("-d");
                    _snippet.Append(_indent)
                        .Append(optionName)
                        .Append(" ")
                        .Append(_quoteType)
                        .Append(content)
                        .Append(_quoteType);
                    return;
            }
            _snippet.Append(content);
        }
    }

    public void AddMethod(string method, bool isBodyEmpty)
    {
        if(method == "HEAD")
            _snippet.Append($" {Format("-I")}");

        if (ShouldAddHttpMethod(method, isBodyEmpty))
        {
            _snippet.Append($" {Format("-X")} {method}");
        }
    }

    public void AddUrl(string url)
    {
        _snippet.Append($" {_settings.QuoteType}{url}{_settings.QuoteType}");
    }

    public string GetSnippet()
    {
        return _snippet.ToString();
    }

    private bool ShouldAddHttpMethod(string method, bool isBodyEmpty)
    {
        switch (method)
        {
            case "HEAD": return false;
            case "GET":
                return !isBodyEmpty;
            case "POST":
                return isBodyEmpty;
            default:
                return true;
        }
    }

    private string Format(string option)
    {
        if (!_settings.IsLongFormat)
        {
            return option;
        }

        switch (option)
        {
            case "-s":
                return "--silent";
            case "-L":
                return "--location";
            case "-m":
                return "--max-time";
            case "-I":
                return "--head";
            case "-X":
                return "--request";
            case "-H":
                return "--header";
            case "-d":
                return "--data";
            case "-F":
                return "--form";
            case "-g":
                return "--globoff";
            default:
                return "";
        }
    }

    private string Sanitize(string input, bool isBackSlashed = false, bool isUrlEncoded = false)
    {
        if (string.IsNullOrEmpty(input)) return "";

        if (isUrlEncoded)
            input = HttpUtility.UrlEncode(input);

        if (isBackSlashed)
            input = Regex.Replace(input,"/\\/", "\\\\");

        if (_settings.QuoteType == QuoteTypes.Double_)
        {
            input = Regex.Replace(input,"(?<!\\\\)\\\\\\\\", "\\\\\\\\\\\\\"");
        }
        else if (_settings.QuoteType == QuoteTypes.Single_)
        {
            input = Regex.Replace(input,"/'/", "\\\\''");
        }

        if (_settings.IsTrimmed)
        {
            return input.Trim();
        }

        return input;
    }

    private string IsBodyEmpty(string reqBody)
    {
        return "";
    }
    
}