using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Universe.SceneTask.Runtime;

using static UnityEditor.AssetDatabase;
using static UnityEditor.EditorApplication;
using static UnityEditor.EditorBuildSettings;
using static UnityEditor.PlayModeStateChange;
using static UnityEditor.SceneManagement.EditorSceneManager;
using static UnityEngine.PlayerPrefs;
using static UnityEngine.SceneManagement.SceneManager;
using static Universe.Editor.UPrefs;

namespace Universe.Toolbar.Editor
{
	[InitializeOnLoad]
	public class SceneSwitcher
	{
		#region Constructor
		
		static SceneSwitcher()
		{
			playModeStateChanged += OverridePlayMode;

			UpdateOverrideState();
		}
		
		#endregion
		
		
		#region Main

		[RuntimeInitializeOnLoadMethod]
		private static void SubscribeToSceneLoading()
		{
			Level.OnLevelLoaded += ExpandScenes;
		}
		
		private static void OverridePlayMode(PlayModeStateChange state)
		{
			UpdateOverrideState();

			if (s_overridePlaymode) UseLevel(state);
			else					UseDefault(state);
		}

		public static void UseLevel(PlayModeStateChange state)
		{
			if (state == ExitingEditMode)
			{
				CacheCurrentSelection();
				SaveOpenScenes();
				CacheCurrentScenes();
				LoadGameStarter();
				isPlaying = true;
				return;
			}
			
			if (state == ExitingPlayMode)
			{
				LoadCachedScenes();
				isPlaying = false;
				return;
			}

			if (state == EnteredEditMode)
			{
				ClearBuffers();
				CleanStartScene();
				SaveOpenScenes();
			}
		}

		private static void UseDefault(PlayModeStateChange state)
		{
			if (state == ExitingEditMode)
			{
				SaveOpenScenes();
				CacheCurrentScenes();
				isPlaying = true;
				return;
			}
			
			if (state == ExitingPlayMode)
			{
				LoadCachedScenes();
				isPlaying = false;
				return;
			}

			if (state == EnteredEditMode)
			{
				ClearBuffers();
				SaveOpenScenes();
			}
		}
		
		#endregion
		

		#region Utils

		private static void  UpdateOverrideState()
		{
			var overrideBit = GetInt(EDITOR_OVERRIDE_PLAYMODE);
			
			s_overridePlaymode = (overrideBit == 1);
		}

		private static void CacheCurrentScenes()
		{
			var c = sceneCount;

			for (var i = 1; i < c; i++) 
			{
				var scene = GetSceneAt (i);
				
				SetString($"[CustomPlayMode]{i.ToString()}", scene.path);
			}
		}
		
		private static void LoadCachedScenes()
		{
			var j = 1;
			
			while (HasKey($"[CustomPlayMode]{j.ToString()}"))
			{
				LoadScene(GetString($"[CustomPlayMode]{j.ToString()}"), LoadSceneMode.Additive);
				DeleteKey($"[CustomPlayMode]{j.ToString()}");
				j++;
			}
		}
		
		private static void LoadGameStarter()
		{
			if (scenes.Length <= 0)
			{
				Debug.LogError("[CustomPlayMode]Can't enter play mode because GameStarter is not in build settings at index 0");
				return;
			}
			
			playModeStartScene = LoadAssetAtPath<SceneAsset>(scenes[0].path);
		}

		private static void CleanStartScene() =>
			playModeStartScene = null;
		

		private static void ExpandScenes(LevelData level)
		{
			var loadedSceneCount = sceneCount;
			var gameType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
			if (gameType is null) return;
			
			var gameWindow = EditorWindow.GetWindow(gameType, false);
			if (gameWindow.maximized) return;
			
			var hierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
			if (hierarchyType is null) return;
			
			var hierarchyWindow = EditorWindow.GetWindow(hierarchyType, false);
			if (hierarchyWindow is null) return;

			var method = hierarchyType.GetMethod("SetExpanded", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method is null) return;

			var selections = Selection.gameObjects;

			for (var i = 0; i < loadedSceneCount; i++)
			{
				var scene = GetSceneAt(i);
				var roots = scene.GetRootGameObjects();
				var rootAmount = roots.Length;

				method.Invoke(hierarchyWindow, new object[] { scene.handle, true });

				for (var j = 0; j < rootAmount; j++)
				{
					var root = roots[j];
					var transform = root.transform;
					
					ExpandSelections(transform, selections, method);
				}
			}
		}

		private static bool ExpandSelections(Transform root, GameObject[] to, MethodInfo with)
		{
			if (with is null) return false;
			
			var hierarchyWindow = EditorWindow.focusedWindow;
			var length = to.Length;
			var expanded = false;
			
			for (var i = 0; i < length; i++)
			{
				var gameObject = to[i];
				var transform = gameObject.transform;
				var parentPath = GetRootParentPathRecursively(transform);
				if (parentPath.Count < 1) continue;
				
				var rootParent = parentPath[0];
				if (!rootParent.Equals(root)) continue;

				var pathLength = parentPath.Count;
				for (int j = 0; j < pathLength - 1; j++)
				{
					var elementTransform = parentPath[j];
					var element = elementTransform.gameObject;
					var elementID = element.GetInstanceID();

					with.Invoke(hierarchyWindow, new object[] { elementID, true });
				}
				
				expanded = true;
			}

			return expanded;
		}
		
		private static List<Transform> GetRootParentPathRecursively(Transform of)
		{
			var result = new List<Transform>();
			var parent = of.parent;

			if (parent)  
				result.AddRange(GetRootParentPathRecursively(parent));
			
			result.Add(of);
			
			return result;
		}

		private static void CacheCurrentSelection()
		{
			var sceneSelectionBuffers = new Dictionary<Scene, SelectionBuffer>();
			var selections = Selection.gameObjects;
			var length = selections.Length;
			
			for (var i = 0; i < length; i++)
			{
				var gameObject = selections[i];
				var scene = gameObject.scene;
				
				if (!sceneSelectionBuffers.ContainsKey(scene))
				{
					SetActiveScene(scene);
					var gameObjectTemplate = new GameObject(SELECTION_BUFFER_NAME);
					var newBuffer = gameObjectTemplate.AddComponent<SelectionBuffer>();

					sceneSelectionBuffers.Add(scene, newBuffer);
					MarkSceneDirty(scene);
				}
				
				var buffer = sceneSelectionBuffers[scene];
				
				buffer.Add(gameObject);
			}
		}

		
		private static void ClearBuffers()
		{
			var sceneAmount = sceneCount;

			for (var i = 0; i < sceneAmount; i++)
			{
				var scene = GetSceneAt(i);
				var roots = scene.GetRootGameObjects();
				var rootAmount = roots.Length;

				for (var j = 0; j < rootAmount; j++)
				{
					var root = roots[j];
					if (!root.name.Equals(SELECTION_BUFFER_NAME)) continue;
					
					GameObject.DestroyImmediate(root);
					MarkSceneDirty(scene);
				}
			}
		}

		#endregion
		
		
		#region Private

		private const string SELECTION_BUFFER_NAME = "[SelectionBuffer]";
		
		public static bool s_overridePlaymode;

		#endregion
	}
}