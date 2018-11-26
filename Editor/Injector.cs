using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
//using ModestTree;
//using Zenject;



namespace XD {

	[InitializeOnLoad]
	public static class Injector {

		static Type[] inChildrenTypes;
		static readonly Type attrType = typeof(FindInChildrenByNameAttribute);

		static Injector() {
			Init();
		}

		static void Init() {
			var ass = typeof(FindInChildrenByNameAttribute).Assembly;
			inChildrenTypes = ass.GetTypes().Where(typ => typ.GetAllInstanceFields().Any(f => f.HasAttribute(attrType))).ToArray();

			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= UpdateScene;
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += UpdateScene;

			EditorApplication.playModeStateChanged -= OnPlayModeChange;
			EditorApplication.playModeStateChanged += OnPlayModeChange;

			UpdateAllScenes();
		}

		private static void OnPlayModeChange(PlayModeStateChange obj) {
			if (obj == PlayModeStateChange.ExitingEditMode) {
				UpdateAllScenes();
			}
		}

		static void UpdateAllScenes() {
			for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++) {
				UpdateScene(UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i));
			}
		}


		private static void UpdateScene(UnityEngine.SceneManagement.Scene scn, string path) {
			UpdateScene(scn);
		}


		private static void UpdateScene(UnityEngine.SceneManagement.Scene scn) {
			foreach (var type in inChildrenTypes) {
				var fields = type.GetAllInstanceFields().Where(f => f.HasAttribute(attrType));
				
				foreach (var monobeh in scn.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren(type, true)).Select(c => c as MonoBehaviour)) {
					var so = new SerializedObject(monobeh);
					so.Update();

					var props = new List<SerializedProperty>();

					foreach (var field in fields) {
						var fnam = field.GetAttribute<FindInChildrenByNameAttribute>().name;
						try {
							var injectType = field.FieldType;
							bool injectGO = (injectType == typeof(GameObject));

							UnityEngine.Object __ = null;
							if (!injectGO) {
								__ = monobeh.GetComponentsInChildren(field.FieldType, true).First(ff => ff.gameObject.name.ToLower().Replace(" ", "") == fnam.ToLower());
							} else {
								__ = monobeh.GetComponentsInChildren(typeof(Transform), true).First(ff => ff.gameObject.name.ToLower().Replace(" ", "") == fnam.ToLower()).gameObject;
							}
							var old = so.FindProperty(field.Name).objectReferenceValue;
							var prop = so.FindProperty(field.Name);
							prop.objectReferenceValue = __;
							props.Add(prop);
							if (old != __) {
								Debug.Log($"<color=green>[Injector]</color> Found {__.name}", __);
							}
						} catch {
							Debug.LogError($"Could not find {field.GetAttribute<FindInChildrenByNameAttribute>().name} <{field.FieldType}>", monobeh);
						}

						// in case of runtime:
						//field.SetValue(monobeh, monobeh.transform.Find(field.GetAttribute<FindInChildrenByNameAttribute>().name).GetComponent(field.FieldType));
					}
					if (so.ApplyModifiedPropertiesWithoutUndo()) {
						Debug.Log($"<color=green>[Injector]</color> Updating references in {monobeh.name}", monobeh);
					}

					foreach (var p in props) {
						if (p.objectReferenceValue == null) {
							Debug.LogError($"[Injector] {p.displayName} is still null", p.serializedObject.targetObject);
						}
					}

					so.Dispose();
				}
			}
		}

		[MenuItem("Tools/Update Injector Refs")]
		static void UpdateRefs() {
			UpdateAllScenes();
		}


		/// EXTENSIONES ///
		 
		public static FieldInfo[] DeclaredInstanceFields(this Type type) {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetRuntimeFields()
                .Where(x => x.DeclaringType == type && !x.IsStatic).ToArray();
#else
			return type.GetFields(
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
#endif
		}


		public static IEnumerable<FieldInfo> GetAllInstanceFields(this Type type) {
			foreach (var fieldInfo in type.DeclaredInstanceFields()) {
				yield return fieldInfo;
			}

			if (type.BaseType() != null && type.BaseType() != typeof(object)) {
				foreach (var fieldInfo in type.BaseType().GetAllInstanceFields()) {
					yield return fieldInfo;
				}
			}
		}


		public static Type BaseType(this Type type) {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return type.GetTypeInfo().BaseType;
#else
			return type.BaseType;
#endif
		}


		public static bool HasAttribute(
	this MemberInfo provider, params Type[] attributeTypes) {
			return provider.AllAttributes(attributeTypes).Any();
		}

		public static bool HasAttribute<T>(this MemberInfo provider)
			where T : Attribute {
			return provider.AllAttributes(typeof(T)).Any();
		}

		public static IEnumerable<T> AllAttributes<T>(
			this MemberInfo provider)
			where T : Attribute {
			return provider.AllAttributes(typeof(T)).Cast<T>();
		}

		public static IEnumerable<Attribute> AllAttributes(
			this MemberInfo provider, params Type[] attributeTypes) {
			Attribute[] allAttributes;
#if NETFX_CORE
            allAttributes = provider.GetCustomAttributes<Attribute>(true).ToArray();
#else
			allAttributes = System.Attribute.GetCustomAttributes(provider, typeof(Attribute), true);
#endif
			if (attributeTypes.Length == 0) {
				return allAttributes;
			}

			return allAttributes.Where(a => attributeTypes.Any(x => a.GetType().DerivesFromOrEqual(x)));
		}

		public static T GetAttribute<T>(this MemberInfo provider)
	where T : Attribute {
			return provider.AllAttributes<T>().Single();
		}


		public static bool DerivesFromOrEqual(this Type a, Type b) {
#if UNITY_WSA && ENABLE_DOTNET && !UNITY_EDITOR
            return b == a || b.GetTypeInfo().IsAssignableFrom(a.GetTypeInfo());
#else
			return b == a || b.IsAssignableFrom(a);
#endif
		}
	}


	[CustomPropertyDrawer(typeof(FindInChildrenByNameAttribute))]
	public class InjectorDrawer : DecoratorDrawer {
		public override void OnGUI(Rect position) {
			position.height = 16f;
			position.x = 0;
			position.y -= 1;
			var attr = (FindInChildrenByNameAttribute)attribute;
			GUI.Label(position, "[i]", EditorStyles.miniLabel);
		}

		public override float GetHeight() {
			return 0;// base.GetHeight() / 1.5f;
		}
	}
}
