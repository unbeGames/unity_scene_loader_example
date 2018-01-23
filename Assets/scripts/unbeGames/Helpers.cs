using UnityEngine;

public static class Helpers {
	public static void Error(params object[] values){
		for(int i = 0; i < values.Length; i++)
			Debug.LogError(values[i]);
	}

	public static void Log(params object[] values){
		for(int i = 0; i < values.Length; i++)
			Debug.Log(values[i]);
	}
}
