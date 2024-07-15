
using UnityEngine;
using UnityEditor;

namespace harapeco.editor
{
	public class OpenSavePath : EditorWindow
	{
		[MenuItem("Tools/永続保存フォルダを開く")]
		public static void Open()
		{
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				System.Diagnostics.Process.Start(Application.persistentDataPath);
			}
			else if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				EditorUtility.RevealInFinder(Application.persistentDataPath);
			}
		}

        [MenuItem("Tools/一時保存フォルダを開く")]
		public static void OpenTmp()
		{
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				System.Diagnostics.Process.Start(Application.temporaryCachePath);
			}
			else if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				EditorUtility.RevealInFinder(Application.temporaryCachePath);
			}
		}
	}

}

