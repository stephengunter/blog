using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Views
{
	public class RefreshTokenRequest
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }

	}

	public class OAuthLoginRequest
	{
		public string Token { get; set; }

	}
}
