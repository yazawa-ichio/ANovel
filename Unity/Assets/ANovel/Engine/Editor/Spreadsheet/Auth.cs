using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ANovel.GoogleApis
{
	public class Auth
	{
		static string s_CachePath = "Library/ANovel.GoogleApis";

		public static void ClearCache(string name)
		{
			var cachePath = $"{s_CachePath}/{name}.json";
			if (File.Exists(cachePath))
			{
				File.Delete(cachePath);
			}
		}

		public static bool HasCache(string name)
		{
			var cachePath = $"{s_CachePath}/{name}.json";
			return File.Exists(cachePath);
		}

		public static void ClearAllCache()
		{
			if (Directory.Exists(s_CachePath))
			{
				Directory.Delete(s_CachePath, true);
			}
		}

		public static Task<string> GetAccessToken(string name, string scope)
		{
			return GetAccessToken(name, scope, GoogleApisSettings.Instance.Credentials);
		}

		public static async Task<string> GetAccessToken(string name, string scope, Credentials credentials)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrEmpty(scope)) throw new ArgumentNullException(nameof(scope));
			if (credentials == null) throw new ArgumentNullException(nameof(credentials));

			var cachePath = $"{s_CachePath}/{name}.json";
			if (File.Exists(cachePath))
			{
				var info = new FileInfo(cachePath);
				var json = File.ReadAllText(cachePath);
				var token = JsonUtility.FromJson<AccessToken>(json);
				var expiresIn = info.LastWriteTimeUtc.AddSeconds(token.expires_in - 10);
				if (DateTime.UtcNow < expiresIn)
				{
					return token.access_token;
				}
				else
				{
					return await Refresh(name, credentials, token);
				}
			}
			else
			{
				var code = await GetCode(scope, credentials);
				return await GetToken(name, code, credentials);
			}
		}


		static async Task<string> GetToken(string name, string code, Credentials credentials)
		{
			Debug.Log($"Request Token {name}");
			// OAuth 2.0認証を行い、アクセストークンを取得する
			using (var client = new HttpClient())
			{
				var port = GoogleApisSettings.Instance.RedirectUrlPorts;

				var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

				// 認証情報を指定する
				request.Content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("grant_type", "authorization_code"),
					new KeyValuePair<string, string>("code", code),
					new KeyValuePair<string, string>("client_id", credentials.ClientId),
					new KeyValuePair<string, string>("client_secret",credentials.ClientSecret),
					new KeyValuePair<string, string>("redirect_uri", $"http://localhost:{port}/")
				});

				// アクセストークンを取得する
				var response = await client.SendAsync(request);
				var content = await response.Content.ReadAsStringAsync();
				if (response.IsSuccessStatusCode)
				{
					var dir = Path.GetDirectoryName($"{s_CachePath}/{name}.json");
					if (!Directory.Exists(dir))
					{
						Directory.CreateDirectory(dir);
					}
					Debug.Log($"Save Token {content}");
					var tokens = JsonUtility.FromJson<AccessToken>(content);
					File.WriteAllText($"{s_CachePath}/{name}.json", JsonUtility.ToJson(tokens, true));
					return tokens.access_token;
				}
				else
				{
					throw new Exception("Failed to get access token from Google.");
				}
			}
		}


		static async Task<string> Refresh(string name, Credentials credentials, AccessToken token)
		{
			Debug.Log($"Request Refresh Token {name}");

			// OAuth 2.0認証を行い、アクセストークンとリフレッシュトークンを取得する
			using (var client = new HttpClient())
			{
				var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

				// 認証情報を指定する
				request.Content = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string, string>("grant_type", "refresh_token"),
					new KeyValuePair<string, string>("refresh_token", token.refresh_token),
					new KeyValuePair<string, string>("client_id", credentials.ClientId),
					new KeyValuePair<string, string>("client_secret", credentials.ClientSecret)
				});

				// アクセストークンとリフレッシュトークンを取得する
				var response = await client.SendAsync(request);
				var content = await response.Content.ReadAsStringAsync();
				if (response.IsSuccessStatusCode)
				{
					var dir = Path.GetDirectoryName($"{s_CachePath}/{name}.json");
					if (!Directory.Exists(dir))
					{
						Directory.CreateDirectory(dir);
					}
					Debug.Log($"Refresh Token {content}");
					var res = JsonUtility.FromJson<AccessToken>(content);
					token.expires_in = res.expires_in;
					token.access_token = res.access_token;
					File.WriteAllText($"{s_CachePath}/{name}.json", JsonUtility.ToJson(token, true));
					return token.access_token;
				}
				else
				{
					throw new Exception("Failed to get tokens from Google.");
				}
			}
		}

		static async Task<string> GetCode(string scope, Credentials credentials)
		{
			try
			{
				Debug.Log($"Request Auth Code {scope}");

				CancellationTokenSource source = new();
				var task = GetCodeImpl(scope, credentials, source.Token);
				while (!task.IsCompleted)
				{
					if (EditorUtility.DisplayCancelableProgressBar("Cancel Auth", "", 0))
					{
						source.Cancel();
					}
					await Task.Yield();
				}
				return task.Result;

			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		static async Task<string> GetCodeImpl(string scope, Credentials credentials, CancellationToken token)
		{
			var port = GoogleApisSettings.Instance.RedirectUrlPorts;


			// ローカルでWebサーバーを起動する
			var listener = new HttpListener();
			listener.Prefixes.Add($"http://localhost:{port}/");
			listener.Start();

			token.Register(() =>
			{
				listener.Stop();
			});

			// OAuth 2.0認証のリクエストに、ローカルで起動したWebサーバーのURIを指定する
			var authUrl = $"https://accounts.google.com/o/oauth2/auth?client_id={credentials.ClientId}&redirect_uri=http://localhost:{port}/&response_type=code&scope={scope}";

			Application.OpenURL(authUrl);

			// ローカルで起動したWebサーバーで、OAuth 2.0認証のレスポンスを受信する
			var context = await listener.GetContextAsync();

			var code = context.Request.QueryString["code"];
			context.Response.StatusCode = 200;
			var buf = System.Text.Encoding.UTF8.GetBytes("OK");
			context.Response.OutputStream.Write(buf, 0, buf.Length);
			context.Response.Close();

			// ローカルで起動したWebサーバーを停止する
			listener.Stop();

			return code;
		}


		[Serializable]
		class AccessToken
		{
			public string access_token;
			public string token_type;
			public string refresh_token;
			public int expires_in;
		}

	}
}