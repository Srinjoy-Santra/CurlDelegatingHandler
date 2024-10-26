using System.Collections.Specialized;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Web;

namespace CurlGenerator;

public interface IBuilder
{
    void AddHeaders(Dictionary<string,string> headers);
    void AddBody(Mode mode, string content);
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
    

    public void AddBody(Mode mode, string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            switch (mode)
            {
                case Mode.Raw:
                    bool isAsperandFound = content.Contains('@');
                    string optionName = isAsperandFound ? "--data-raw" : Format("-d");
                    _snippet.Append(_indent)
                        .Append(optionName)
                        .Append(" ")
                        .Append(_quoteType)
                        .Append(content)
                        .Append(_quoteType);
                    return;
                case Mode.FormData:
                    string[] data = content.Split("--");
                    foreach (string datum in data)
                    {
                        string[] lines = datum.Split("\r\n");
                        if (lines.Length > 5)
                        {
                            bool isFileType = !lines[1].Contains("text/plain");
                            if (!isFileType)
                            {
                                string key = lines[2].Split("=")[1];
                                string value = lines[4];

                                _snippet.Append(_indent).Append(Format("-F"));
                                _snippet.Append(" ").Append(_quoteType);
                                _snippet.Append(Sanitize(key)).Append("=");
                                _snippet.Append(QuoteTypes.Double_).Append(Sanitize(value)).Append(QuoteTypes.Double_);
                                
                                // TODO:contentType check and append
                                _snippet.Append(_quoteType);
                            }
                        }
                    }

                    return;
                case Mode.FormUrlEncoded:
                    NameValueCollection queryString = HttpUtility.ParseQueryString(content);
                    foreach (string? key in queryString)
                    {
                        if (key is null)
                        {
                            if(queryString.Count > 1)
                                continue;
                            _snippet.Append(_indent).Append(Format("-d"));
                            _snippet.Append(" ").Append(_quoteType);
                            _snippet.Append(content);
                            _snippet.Append(_quoteType);

                        }
                        else
                        {
                             
                            string value = queryString[key] ?? string.Empty;
                        
                            _snippet.Append(_indent).Append(Format("-d"));
                            _snippet.Append(" ").Append(_quoteType);
                            _snippet.Append(UrlEncode(key)).Append("=");
                            _snippet.Append(UrlEncode(value));
                            _snippet.Append(_quoteType);
                        }
                    }

                    return;
            }
            _snippet.Append(content);
        }
    }

    private string UrlEncode(string value)
    {
        if (value.Contains("'"))
        {
            string singleQuoteRemovedValue = value.Substring(1,value.Length-2);

            string encodedValue = Sanitize(singleQuoteRemovedValue, isUrlEncoded: true);
            return $@"'\''{encodedValue}'\''";
        }

        return Sanitize(value, isUrlEncoded:true);
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
        var queries = HttpUtility.ParseQueryString(url);
        if (queries.Count > 0)
        {
            string[] urlParts = url.Split("?");
            if (urlParts.Length == 2)
            {
                StringBuilder urlBuilder = new();
                int queryCount = 0;
                foreach (string? key in queries)
                {
                    if (key is null) continue;
                    string value = queries[key] ?? string.Empty;
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = Uri.EscapeDataString(value);
                    }

                    urlBuilder.Append(key).Append("=").Append(value);
                    queryCount ++;
                    if(queryCount != queries.Count)
                        urlBuilder.Append("&");
                }

                url = urlBuilder.ToString();
            }
        }
        _snippet.Append($" {_settings.QuoteType}{url}{_settings.QuoteType}");
    }

    public string GetSnippet()
    {
        return _snippet.ToString();
    }

    private bool ShouldAddHttpMethod(string method, bool isBodyEmpty)
    {
        bool isBodyPruningDisabled = true;
        
        switch (method)
        {
            case "HEAD": 
                return false;
            case "GET":
                return !isBodyEmpty && isBodyPruningDisabled;
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
            input = Uri.EscapeDataString(input);

        if (isBackSlashed)
            input = Regex.Replace(input,"/\\/", "\\\\");

        if (_settings.QuoteType == QuoteTypes.Double_)
        {
            input = Regex.Replace(input,"(?<!\\\\)\\\\\\\\", "\\\\\\\\\\\\\"");
        }
        else if (_settings.QuoteType == QuoteTypes.Single_)
        {       
            input = input.Replace("\\", @"\\");
            input = Regex.Replace(input,"'", "'\\''");
            input = Regex.Replace(input,"\"", "\\\"");
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