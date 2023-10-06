using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ANOVEL_URP
using UnityEngine.Rendering.Universal;
#endif

namespace ANovel.Engine
{
	class RendererIndexAttribute : PropertyAttribute
	{
#if ANOVEL_URP && UNITY_EDITOR
		[CustomPropertyDrawer(typeof(RendererIndexAttribute))]
		class RendererIndexPropertyDrawer : PropertyDrawer
		{
			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				return base.GetPropertyHeight(property, label) * 2;
			}

			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				position.height /= 2f;
				EditorGUI.LabelField(position, "Renderer");
				position.y += position.height;
				if (UniversalRenderPipeline.asset == null)
				{
					EditorGUI.LabelField(position, "Disabled URP");
					return;
				}
				var method = typeof(UniversalRenderPipelineAsset).GetProperty("rendererDisplayList", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var list = (GUIContent[])method.GetValue(UniversalRenderPipeline.asset);
				property.intValue = EditorGUI.Popup(position, property.intValue + 1, list) - 1;
			}

		}
#endif
	}

}