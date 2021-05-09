using ANovel.Core;

namespace ANovel.Service
{

	public class CharaObjectConfig
	{
		public bool FaceWindow = true;

		public string Pose;
		public string Face;
		public string Level;

		[SkipInjectParam]
		public string ImagePath { get; internal set; }
		[SkipInjectParam]
		public FaceWindowConfig FaceWindowConfig { get; set; }

		public void Init(string name, CharaMetaData meta, IEnvData data)
		{
			data.TryGet(name, out CharaObjectEnvData envData);
			if (!string.IsNullOrEmpty(Face))
			{
				envData.Face = Face;
			}
			else
			{
				Face = envData.Face;
			}
			if (!string.IsNullOrEmpty(Pose))
			{
				envData.Pose = Pose;
			}
			else
			{
				Pose = envData.Pose;
			}
			if (!string.IsNullOrEmpty(Level))
			{
				envData.Level = Level;
			}
			else
			{
				Level = envData.Level;
			}
			ImagePath = meta.GetPath(name, envData);
			if (FaceWindow && data.TryGetSingle(out FaceWindowEnvData faceWindow) && faceWindow.Name == name)
			{
				FaceWindowConfig = new FaceWindowConfig();
				meta.UpdateFaceWindow(name, envData, FaceWindowConfig);
				data.UpdateSingle<FaceWindowEnvData, FaceWindowConfig>(FaceWindowConfig);
			}
			data.Set(name, envData);
		}
	}

	public struct CharaObjectEnvData
	{
		public string Face;
		public string Pose;
		public string Level;
	}

}