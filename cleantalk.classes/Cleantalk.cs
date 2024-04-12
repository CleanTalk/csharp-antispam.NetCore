using Microsoft.AspNetCore.Http;
using System.Net.Sockets;
using System.Net;
using APIMethods.Pages;
using Newtonsoft.Json;
using System.Text;

namespace cleantalk.classes
{
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

	public static class ClientIpAddressHelper
	{
		// Get the client IP address
		public static string GetClientIpAddress(HttpContext httpContext)
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
	}

	// Create an instance of the ApiService
	public class ApiService
	{
		public static HttpClient _httpClient;
		public static string ApiUrl => "https://moderate.cleantalk.org/api2.0";

		public ApiService(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		// Create an instance of the ApiService
		public async Task<ApiResponse> CheckMessageAsync(ApiRequest request)
		{
			var jsonRequest = JsonConvert.SerializeObject(request);
			var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

			//var response = await _httpClient.PostAsync(ApiUrl, content);
			var response = await ApiService._httpClient.PostAsync(ApiService.ApiUrl, content);

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

}
