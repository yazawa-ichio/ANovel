using ANovel.Commands;
using ANovel.Core;
using ANovel.Service;

namespace ANovel
{
	public class EngineEnvDataProcessor : IEnvDataCustomProcessor
	{
		public int Priority => 0;

		public void PostUpdate(EnvDataUpdateParam param)
		{
			if (!param.ClearCurrentText)
			{
				return;
			}
			if (param.Text == null)
			{
				return;
			}
			var data = param.Data;
			if (!data.TryGetSingle(out MessageEnvData message))
			{
				return;
			}
			if (data.TryGetSingle<MessageStatusEnvData>(out var status) && status.Hide)
			{
				param.AddCommand(new MessageShowCommand());
			}
		}

		public void PreUpdate(EnvDataUpdateParam param)
		{
			if (!param.ClearCurrentText)
			{
				return;
			}
			var text = param.Text;
			var data = param.Data;
			data.DeleteSingle<FaceWindowEnvData>();
			if (text == null)
			{
				data.DeleteSingle<MessageEnvData>();
				return;
			}
			ParseMessage(param, out var message);
			SetFaceWindowCommand(param, message.Chara);
			data.SetSingle(message);
		}

		protected void ParseMessage(EnvDataUpdateParam param, out MessageEnvData data)
		{
			var text = param.Text;
			var top = text.GetLine(0).Trim();
			if (top.StartsWith("【") && top.EndsWith("】"))
			{
				string name = top.Substring(1, top.Length - 2);
				string key = name;
				var split = name.IndexOf('/');
				if (split >= 0)
				{
					key = name.Substring(split + 1);
					name = name.Substring(0, split);
				}
				data = new MessageEnvData
				{
					Name = name,
					Chara = key,
					Message = text.GetRange(1),
				};
			}
			else
			{
				data = new MessageEnvData
				{
					Name = "",
					Chara = "",
					Message = text.Get(),
				};
			}
		}

		void SetFaceWindowCommand(EnvDataUpdateParam param, string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return;
			}
			key = CharaMetaData.GetKey(param.Meta, key);
			var cmd = new FaceWindowShowCommand();
			var data = PrefixedEnvData.Get(param.Data, ImageService.Category.Chara);
			if (data.TryGet<CharaObjectEnvData>(key, out var chara))
			{
				var meta = CharaMetaData.Get(param.Meta, key);
				meta.UpdateFaceWindow(key, chara, cmd.Config);
			}
			else
			{
				cmd.Config.Name = key;
			}
			param.AddCommand(cmd);
		}

	}
}