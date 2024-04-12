using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using cleantalk.classes;

namespace APIMethods.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        public string FirstName { get; set; } = "";

		[BindProperty]
		public string LastName { get; set; } = "";

		[BindProperty]
		public string Email { get; set; } = "";

		[BindProperty]
		public string Message { get; set; } = "";

		[BindProperty]
		public string Subject { get; set; } = "";

		[BindProperty]
		public string Phone { get; set; } = "";

		public string errorMessage = "";
		public string successMessage = "";

		private readonly IHttpContextAccessor _httpContextAccessor;

		public ContactModel(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		// GET
        public void OnGet()
        {
        }

		// Your auth key: https://cleantalk.org/my
		public const string AuthKey = "your_auth_key";
		
		// POST
		public async Task OnPostAsync()
        {
			if (!ModelState.IsValid)
			{
				foreach (var modelState in ModelState.Values)
				{
					foreach (var error in modelState.Errors)
					{
						Console.WriteLine(error.ErrorMessage);
					}
				}
				errorMessage = "Data validation failed.";
				return;
			}
			// Calculate js_on parameter
			int jsOn = int.TryParse(Request.Form["jsOn"], out int jsOnResult) ? jsOnResult : 0;

			// Calculate submit_time parameter
			int submitTime = int.TryParse(Request.Form["pageLoadTime"], out int result) ? result : 0;
			// Calculate submit_time and js_on parameters

			var senderIp = ClientIpAddressHelper.GetClientIpAddress(HttpContext);

			// Get all headers
			var headersDictionary = HttpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value);
			string allHeadersJson = JsonConvert.SerializeObject(headersDictionary);

			// Get sender_info
			var senderInfoDictionary = new Dictionary<string, string>
{
				{ "REFFERRER", HttpContext.Request.Headers["Referer"] },
				{ "USER_AGENT", HttpContext.Request.Headers["User-Agent"] },
				{ "page_url", $"{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}" },
				{ "ct_bot_detector_event_token", Request.Form["ct_bot_detector_event_token"] }
			};

			string senderInfoJson = JsonConvert.SerializeObject(senderInfoDictionary);
			Console.WriteLine($"Sender Info JSON: {senderInfoJson}");

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

			string serializedApiRequest = JsonConvert.SerializeObject(apiRequest);
			Console.WriteLine($"Serialized API Request: {serializedApiRequest}");

			var apiService = new ApiService(new HttpClient());
			try
			{
				var apiResponse = await apiService.CheckMessageAsync(apiRequest);

			if (apiResponse.allow == 0)
			{
				// Do not send the form
				errorMessage = apiResponse.Comment;
				return;
			}
			// Send the form
			successMessage = "The message was sent successfully.";
			FirstName = "";
			LastName = "";
			Email = "";
			Message = "";
			Subject = "";
			Phone = "";
			ModelState.Clear();
			return;
			}
			catch (ApiException ex)
			{
				errorMessage = $"An error occurred while communicating with the API: {ex.Message}";
			}

        }
    }
}
