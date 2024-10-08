﻿namespace XenForo.NET
{
	using System;

	using Newtonsoft.Json.Linq;

	using RestSharp;
	using RestSharp.Authenticators;
	using Script_Engine.Utilities;
	using XenForo.NET.Models.Api;
	using XenForo.NET.Models.Enums;

	public partial class XenForoApi
	{
		/// <summary>
		/// Gets the rest client.
		/// </summary>
		private RestClient Client
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the configuration.
		/// </summary>
		private XenForoConfig Config
		{
			get;
		}

		/// <summary>
		/// Gets the token.
		/// </summary>
		public Token Token
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is authenticated.
		/// </summary>
		public bool IsAuthenticated
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XenForoApi"/> class.
		/// </summary>
		public XenForoApi(XenForoConfig Config)
		{
			if (Config == null)
			{
				throw new Exception("Config cannot be null at XenForoApi(Config)!");
			}

			Logger.WriteLine("Initializing auth..");

			this.Config = Config;
			this.Client = new RestClient(Config.Url);
			Client.Proxy = null;
		}

		/// <summary>
		/// Authenticates the user using the specified credentials.
		/// </summary>
		/// <param name="Username">The username.</param>
		/// <param name="Password">The password.</param>

		public void Authenticate(string Username, string Password)
		{
			var Request = new RestRequest("?oauth/token", Method.POST);

			Request.AddParameter("client_id", this.Config.ClientId);
			Request.AddParameter("client_secret", this.Config.ClientSecret);
			Request.AddParameter("grant_type", "password");

			Request.AddParameter("username", Username);
			Request.AddParameter("password", Password);

			var Response = this.Client.Post(Request);

			if (Response.IsSuccessful)
			{
				var Json = JObject.Parse(Response.Content);

				if (Json != null && Json.HasValues)
				{
					if (Json.ContainsKey("access_token"))
					{
						if (Json.GetValue("token_type").ToObject<string>() == "Bearer")
						{
							this.Token = new Token(Json.GetValue("access_token").ToObject<string>(), Json.GetValue("refresh_token").ToObject<string>(), TokenType.Bearer);
							this.Client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(this.Token.AccessKey, "Bearer");
						}
						else
						{
							this.Token = new Token(Json.GetValue("access_token").ToObject<string>(), Json.GetValue("refresh_token").ToObject<string>(), TokenType.Query);
							this.Client.Authenticator = new OAuth2UriQueryParameterAuthenticator(this.Token.AccessKey);
						}

						// Issue with latest update of the addon, temp fix :
						Client.AddDefaultParameter("oauth_token", Token.AccessKey, ParameterType.QueryString);

						this.Token.SetDuration(TimeSpan.FromSeconds(Json.GetValue("expires_in").ToObject<int>()));

						// Check if we are authenticated..

						int VisitorId = this.GetVisitorId();

						if (VisitorId != -1)
						{
							this.IsAuthenticated = true;
						}
					}
				}
			}
		}

		/// <summary>
		/// Makes the request at the specified url.
		/// </summary>
		private int GetVisitorId()
		{
			var VisitorId = -1;
			var Request = new RestRequest() { Method = Method.GET };
			var Response = this.Client.Get(Request);

			if (Response.IsSuccessful)
			{
				var Json = JObject.Parse(Response.Content);

				if (Json != null && Json.HasValues)
				{
					if (Json.ContainsKey("system_info"))
					{
						var SystemArr = Json["system_info"];

						if (SystemArr.Type != JTokenType.Object)
						{
							return -1;
						}

						var SystemInfo = SystemArr.ToObject<JObject>();

						if (SystemInfo != null && SystemInfo.HasValues)
						{
							if (SystemInfo.ContainsKey("visitor_id"))
							{
								VisitorId = SystemInfo.GetValue("visitor_id").ToObject<int>();
							}
						}
					}
				}
			}

			return VisitorId;
		}
	}
}