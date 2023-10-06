using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ANovel.Engine.PostEffects
{
#if UNITY_EDITOR
	[CustomEditor(typeof(PostEffectService))]
	class PostEffectServiceInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			SerializedProperty property = serializedObject.GetIterator();
			bool expanded = true;
			while (property.NextVisible(expanded))
			{
				if (property.propertyPath == "m_PostEffects")
				{
					DrawPostEffects(property);
					continue;
				}
				using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
				{
					EditorGUILayout.PropertyField(property, true);
				}
				expanded = false;
			}
			serializedObject.ApplyModifiedProperties();
		}

		protected void DrawPostEffects(SerializedProperty property)
		{
			EditorGUILayout.LabelField("PostEffects");
			for (int i = 0; i < property.arraySize; i++)
			{
				DrawPostEffectItem(property, i, property.GetArrayElementAtIndex(i));
			}
			if (GUILayout.Button("+"))
			{
				Add();
			}
		}


		protected void DrawPostEffectItem(SerializedProperty root, int index, SerializedProperty property)
		{
			if (property.managedReferenceValue == null)
			{
				root.DeleteArrayElementAtIndex(index);
				return;
			}
			GUILayout.BeginHorizontal("box");
			var enabled = property.FindPropertyRelative("m_Enabled");
			enabled.boolValue = EditorGUILayout.Toggle(enabled.boolValue, GUILayout.Width(15));
			EditorGUILayout.LabelField(property.managedReferenceValue.GetType().Name);
			if (GUILayout.Button("x", GUILayout.ExpandWidth(false)))
			{
				root.DeleteArrayElementAtIndex(index);
				return;
			}
			GUILayout.EndHorizontal();
			if (property.hasChildren)
			{
				using (new EditorGUI.IndentLevelScope())
				{
					var end = property.GetEndProperty();
					bool expanded = true;
					while (property.NextVisible(expanded) && !SerializedProperty.EqualContents(property, end))
					{
						EditorGUILayout.PropertyField(property, true);
						expanded = false;
					}
				}
			}
		}

		void Add()
		{
			GenericMenu menu = new GenericMenu();
			foreach (var type in TypeCache.GetTypesDerivedFrom<IPostEffect>())
			{
				if (type.IsAbstract || type.IsInterface)
				{
					continue;
				}
				menu.AddItem(new GUIContent(type.FullName.Replace("ANovel.Engine.PostEffects.", "").Replace(".", "/")), false, (x) =>
				{
					var target = (System.Type)x;
					var so = serializedObject;
					so.Update();
					var property = so.FindProperty("m_PostEffects");
					property.InsertArrayElementAtIndex(property.arraySize);
					property = property.GetArrayElementAtIndex(property.arraySize - 1);
					property.managedReferenceValue = System.Activator.CreateInstance(target);
					so.ApplyModifiedProperties();

				}, type);
			}
			menu.ShowAsContext();
		}

	}
#endif
}