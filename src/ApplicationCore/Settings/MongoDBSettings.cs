using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ApplicationCore.Settings
{
	public class MongoDBSettings
	{
		public string Host { get; set; }

		public int Port { get; set; }

		public string DBName { get; set; }


		public string Username { get; set; }

		public string Password { get; set; }

		public string ConnectionString
			=> $"mongodb://{HttpUtility.UrlEncode(Username)}:{HttpUtility.UrlEncode(Password)}@{Host}:{Port}/admin";
	}
}
