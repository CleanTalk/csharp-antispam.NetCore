using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;
using System.Net.Sockets;
using System.Net;

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

		// API
		public class ApiException : Exception
		{
			public ApiException(string message) : base(message) { }
		}

		// Response
        public class ApiResponse
        {
            public int Spam { get; set; }
			public int allow { get; set; }
			public string Comment { get; set; }
        }

		// Create an instance of the ApiService
		public class ApiService
        {
            private readonly HttpClient _httpClient;
            private const string ApiUrl = "https://moderate.cleantalk.org/api2.0";

            public ApiService(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task<ApiResponse> CheckMessageAsync(ApiRequest request)
            {
				//using var httpClient = new HttpClient();
				var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiUrl, content);
                response.EnsureSuccessStatusCode();

				 var jsonResponse = await response.Content.ReadAsStringAsync();

				 // Print the API response to the console
				Console.WriteLine("API Response:");
				Console.WriteLine(jsonResponse);
				try
				{
					var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);
					return apiResponse;
				}
				catch (JsonReaderException)
				{
					throw new ApiException($"Failed to deserialize API response: {jsonResponse}");
				}
			}
        }

		// GET
        public void OnGet()
        {
        }

		// Get the client IP address
		private string GetClientIpAddress(HttpContext httpContext)
		{
			if (httpContext.Request.Headers != null && httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
			{
				string ipAddressList = httpContext.Request.Headers["X-Forwarded-For"];
				if (!string.IsNullOrEmpty(ipAddressList))
				{
					string[] ipAddresses = ipAddressList.Split(",");
					if (ipAddresses.Length > 0)
					{
						string clientIpAddress = ipAddresses[0].Trim();
						if (IPAddress.TryParse(clientIpAddress, out IPAddress ip))
						{
							if (ip.AddressFamily == AddressFamily.InterNetwork)
							{
								return clientIpAddress;
							}
						}
					}
				}
			}

			// Fallback to the remote IP address
			string remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
			IPAddress ipAddress;
			if (IPAddress.TryParse(remoteIpAddress, out ipAddress))
			{
				if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					// Convert IPv6 localhost to IPv4 localhost
					if (IPAddress.IsLoopback(ipAddress))
					{
						return "127.0.0.1";
					}
				}
			}
			return remoteIpAddress;
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
