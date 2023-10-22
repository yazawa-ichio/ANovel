using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel.GoogleApis
{

	public class SpreadSheetClient
	{
		Credentials m_Credentials;

		public SpreadSheetClient()
		{
			m_Credentials = GoogleApisSettings.Instance.Credentials;
		}

		public SpreadSheetClient(Credentials credentials)
		{
			m_Credentials = credentials;
		}

		Task<string> GetAccessToken()
		{
			return Auth.GetAccessToken("SpreadSheet", "https://www.googleapis.com/auth/spreadsheets", m_Credentials);
		}

		async Task<string> HttpGet(string url)
		{
			var accessToken = await GetAccessToken();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var response = await client.GetAsync(url);

				// 取得したデータを表示する
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadAsStringAsync();
				}
				else
				{
					throw new Exception($"Failed to get data from Google Spreadsheet..\n{await response.Content.ReadAsStringAsync()}");
				}
			}
		}

		async Task<string> Send(string url, HttpMethod method, HttpContent content)
		{
			var accessToken = await GetAccessToken();

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				var response = await client.SendAsync(new HttpRequestMessage(method, url)
				{
					Content = content
				});

				// 取得したデータを表示する
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadAsStringAsync();
				}
				else
				{
					throw new Exception($"Failed from Google Spreadsheet.\n{await response.Content.ReadAsStringAsync()}");
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

		public async Task Update(string spreadsheetId, ValueRange valueRange, ValueInputOption valueInputOption = ValueInputOption.RAW)
		{
			var url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{valueRange.Range}?valueInputOption={valueInputOption}";
			var content = new StringContent(valueRange.ToJson());
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			var json = await Send(url, HttpMethod.Put, content);
			Debug.Log($"update {spreadsheetId} {valueRange.Range}\n{json}");
		}

		public async Task Append(string spreadsheetId, ValueRange valueRange, ValueInputOption valueInputOption = ValueInputOption.RAW)
		{
			var url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{valueRange.Range}:append?valueInputOption={valueInputOption}";
			var content = new StringContent(valueRange.ToJson());
			content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			var json = await Send(url, HttpMethod.Post, content);
			Debug.Log($"append {spreadsheetId} {valueRange.Range}\n{json}");
		}

		public async Task<ClearResponse> Clear(string spreadsheetId, string range)
		{
			var url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}:clear";
			var json = await Send(url, HttpMethod.Post, null);
			Debug.Log($"clear {spreadsheetId} {range}\n{json}");
			return JsonUtility.FromJson<ClearResponse>(json);
		}


		public async Task AddSheet(string spreadsheetId, string name)
		{
			var url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}:batchUpdate";
			StringBuilder sb = new();
			using (sb.AppendScope("{", "}"))
			{
				sb.Append("\"requests\": ");
				using (sb.AppendScope("[", "]"))
				{
					using (sb.AppendScope("{", "}"))
					{
						sb.Append("\"addSheet\": {");
						sb.Append($"\"properties\": {{\"title\": \"{name}\"}}");
						sb.Append("}");
					}
				}
			}
			var content = new StringContent(sb.ToString());
			var json = await Send(url, HttpMethod.Post, content);
			Debug.Log($"AddSheet {spreadsheetId} \n{json}");
		}

	}

}