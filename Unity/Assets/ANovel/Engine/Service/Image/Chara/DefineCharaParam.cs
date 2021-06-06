namespace ANovel.Engine
{
	public class DefineCharaParam
	{
		/// <summary>
		/// プログラム上で扱われる名前です。
		/// ファイル名などはこちらで解決されます。
		/// </summary>
		[Argument(Required = true)]
		public string Value;
		/// <summary>
		/// スクリプト上で可読性の高い名前を付ける為に利用します
		/// </summary>
		[Argument]
		string m_Key = default;
		public string Key => m_Key ?? Value;
	}

	public class DefineCharaPoseParam : DefineCharaParam
	{
		public float OffsetX;
		public float OffsetY;
		public float OffsetZ;
		[RateArgument]
		public float Scale = 1f;

		public float? FaceWindowX;
		public float? FaceWindowY;
		public float? FaceWindowZ;
		public float? FaceWindowScale;
	}

	public class DefineCharaLevelParam : DefineCharaParam
	{
		public float OffsetX;
		public float OffsetY;
		public float OffsetZ;
		[RateArgument]
		public float Scale = 1f;
	}
}