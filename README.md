# csharp-antispam
CleanTalk service API for C# .Net Core. It is invisible protection from spam, no captchas, no puzzles, no animals, and no math.
# Actual API documentation
* [check_message](https://cleantalk.org/help/api-check-message) - Check IPs, Emails and messages for spam activity
* [check_newuser](https://cleantalk.org/help/api-check-newuser) - Check registrations of new users
* [spam_check](https://cleantalk.org/help/api-spam-check) - This method should be used for bulk checks of IP, Email for spam activity
* [ip_info](https://cleantalk.org/help/api-ip-info-country-code) - method returns a 2-letter country code (US, UK, CN, etc.) for an IP address
# How does the API stop spam?
* The API uses several simple tests to stop spammers.
* Spambot signatures.
* Blacklist checks by Email, IP, website domain names.
* Javascript availability.
* Comment submit time.
* Relevance test for the comment.
# How does the API work?
API sends the comment's text and several previous approved comments to the server. The server evaluates the relevance of the comment's text on the topic, tests for spam and finally provides a solution - to publish or to put in manual moderation queue of comments. If a comment is placed in manual moderation queue, the plugin adds a rejection explanation to the text of the comment.
# Requirements
* [.Net Core v8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [CleanTalk account](https://cleantalk.org/register?product=anti-spam)
* # SPAM test examples
* ## Using the check_message method to check contact form
```
public const string AuthKey = "your_auth_key";

public async Task OnPostAsync()
{
  // Calculate js_on parameter
  int jsOn = int.TryParse(Request.Form["jsOn"], out int jsOnResult) ? jsOnResult : 0;

  // Calculate submit_time parameter
  int submitTime = int.TryParse(Request.Form["pageLoadTime"], out int result) ? result : 0;
  // Calculate submit_time and js_on parameters
  
  var senderIp = GetClientIpAddress(HttpContext);
  
  // Get all headers
  var headersDictionary = HttpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value);
  string allHeadersJson = JsonConvert.SerializeObject(headersDictionary);

  // Get sender_info
  var senderInfoDictionary = new Dictionary<string, string>
  {
    { "REFFERRER", HttpContext.Request.Headers["Referer"] },
    { "USER_AGENT", HttpContext.Request.Headers["User-Agent"] },
    { "page_url", $"{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}" },
    { "ct_bot_detector_event_token", Request.Form["ct_bot_detector_event_token"] } //     To get this param:
        //         1. add a script to the web-page: <script src="https://moderate.cleantalk.org/ct-bot-detector-wrapper.js" id="ct_bot_detector-js"></script>
        //         2. parse the newly added hidden input on the web form, the name atrribute of input is "ct_bot_detector_event_token" 
        //     @var string
  };

  string senderInfoJson = JsonConvert.SerializeObject(senderInfoDictionary);
  
  // Create a dictionary to hold the post_info data
  var postInfo = new Dictionary<string, string>
  {
    { "comment_type", "general_comment" }
  };

  // Convert the dictionary to a JSON string
  var postInfoJson = JsonConvert.SerializeObject(postInfo);
  var apiRequest = new ApiRequest(AuthKey)
  {
    MethodName = "check_message",
    SenderNickname = "Mike",
    SenderEmail = Email,
    SenderIp = senderIp,
    Message_ = Message,
    JsOn = jsOn,
    SubmitTime = submitTime,
    AllHeaders = allHeadersJson,
    SenderInfo = senderInfoJson,
    Agent = "php-api",
    PostInfo = postInfoJson,
  };
  var apiService = new ApiService(new HttpClient());
  var apiResponse = await apiService.CheckMessageAsync(apiRequest);

  if (apiResponse.allow == 0)
  {
    // Do not send the form
    errorMessage = apiResponse.Comment;
    return;
  }
    // Send the form
    successMessage = "The message was sent successfully.";
    return;
}
```
Result on screen:
![screen_example1](https://github.com/Barogg/csharp-antispam.NetCore/assets/38746827/1e77fd8f-de39-4d23-8ce6-39b8c67391be)
API returns response object:
* stop_queue — stop queue for comment approvement (comment is a 100% spam);
* js_disabled — JavaScript is disabled;
* blacklisted — sender is in the CleanTalk Blacklists;
* comment — comment of server's decision or of other errors (wrong Access key etc.);
* codes — answer codes;
* spam — spam, possible to send it to manual moderation if flag stop_queue == 0;
* account_status — is account enabled or not (0 or 1);
* fast_submit — too fast form submitting;
* allow — is message allowed (1) or not (0).
* id — message ID (helpful for our support);

More examples for other methods will be added in the future.
