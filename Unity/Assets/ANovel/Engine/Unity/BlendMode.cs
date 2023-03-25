namespace ANovel
{
	public enum BlendMode
	{
		Normal = 0,
		// ディザ合成
		//Dissolve = 1,
		// 背景
		//Behine = 2,
		// 消去
		//Clear = 3,
		// 比較（暗）
		Darken = 4,
		// 乗算
		Multiply,
		// 焼き込みカラー
		ColorBurn,
		// 焼き込み（リニア）
		LinearBurn,
		// カラー比較（暗）
		DarkerColor,
		// 比較（明）
		Lighten,
		// スクリーン
		Screen,
		// 覆い焼きカラー
		ColorDodge,
		// 覆い焼（リニア）加算
		LinearDodge,
		// カラー比較（明）
		LighterColor,
		// オーバーレイ
		Overlay,
		// ソフトライト
		SoftLight,
		// ハードライト
		HardLight,
		// ビビットライト
		VividLight,
		// リニアライト
		LinearLight,
		// ピンライト
		PinLight,
		// ハードミックス
		HardMix,
		// 差の絶対値
		Difference,
		// 減算
		Subtract,
		// 除算
		Divide,
		// 除外
		Exclusion,
		// 色相
		Hue,
		// 彩度
		Saturation,
		// カラー
		Color,
		// 輝度
		Luminosity,
	}
}