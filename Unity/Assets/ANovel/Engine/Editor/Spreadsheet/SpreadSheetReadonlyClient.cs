using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel.GoogleApis
{
	public class SpreadSheetReadonlyClient
	{
		Credentials m_Credentials;

		public SpreadSheetReadonlyClient()
		{
			m_Credentials = GoogleApisSettings.Instance.Credentials;
		}

		public SpreadSheetReadonlyClient(Credentials credentials)
		{
			m_Credentials = credentials;
		}

		Task<string> GetAccessToken()
		{
			return Auth.GetAccessToken("SpreadSheetReadonly", "https://www.googleapis.com/auth/spreadsheets.readonly", m_Credentials);
		}

		async Task<string> HttpGet(string url)
		{
			var accessToken = await GetAccessToken();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var response = await client.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadAsStringAsync();
				}
				else
				{
					throw new Exception($"Failed to get data from Google Spreadsheet. \n{await response.Content.ReadAsStringAsync()}");
				}
			}
		}

		public async Task<SpreadSheet> Get(string spreadsheetId)
		{
			var url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}";
			var json = await HttpGet(url);
			return JsonUtility.FromJson<SpreadSheet>(json);
		}

		public async Task<ValueRange> GetValues(string spreadsheetId, string range)
		{
			var url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}";
			var json = await HttpGet(url);
			return ValueRange.CreateFromJson(json);
		}

	}





}