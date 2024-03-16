using System.Runtime.Serialization;

namespace APIMethods.Pages
{
	//Request
	[DataContract]
	public class ApiRequest
	{
		public ApiRequest(string authKey)
		{
			AuthKey = authKey;
			// Initialize other necessary and additional parameters
		}

		[DataMember(Name = "method_name")]
		public string MethodName { get; set; }

		[DataMember(Name = "auth_key")]
		public string AuthKey { get; set; }

		[DataMember(Name = "sender_email")]
		public string SenderEmail { get; set; }

		[DataMember(Name = "sender_ip")]
		public string SenderIp { get; set; }

		[DataMember(Name = "js_on")]
		public int? JsOn { get; set; }

		[DataMember(Name = "submit_time")]
		public int? SubmitTime { get; set; }

		[DataMember(Name = "message")]
		public string Message_ { get; set; }

		[DataMember(Name = "all_headers")]
		public string AllHeaders { get; set; }

		[DataMember(Name = "sender_info")]
		public string SenderInfo { get; set; }

		[DataMember(Name = "sender_nickname")]
		public string SenderNickname { get; set; }

		[DataMember(Name = "post_info")]
		public string PostInfo { get; set; }

		//Engine
		[DataMember(Name = "agent")]
		public string Agent { get; set; }
		// Include other necessary and additional parameters
	}
}
