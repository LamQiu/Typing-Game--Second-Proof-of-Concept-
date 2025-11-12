using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ExtensionMethods {


	#region transform methods
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Set World X/Y/Z/XY
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void SetX(this Transform tf, float newX){
		Vector3 pos = tf.position;
		pos.x = newX;
		tf.position = pos;
	}

	public static void SetY(this Transform tf, float newY){
		Vector3 pos = tf.position;
		pos.y = newY;
		tf.position = pos;
	}

	public static void SetZ(this Transform tf, float newZ){
		Vector3 pos = tf.position;
		pos.z = newZ;
		tf.position = pos;
	}

	public static void SetXY(this Transform tf, float newX, float newY){
		Vector3 pos = tf.position;
		pos.x = newX;
		pos.y = newY;
		tf.position = pos;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Set Local X/Y/Z/XY
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void SetLocalX(this Transform tf, float newX){
		Vector3 pos = tf.localPosition;
		pos.x = newX;
		tf.localPosition = pos;
	}

	public static void SetLocalY(this Transform tf, float newY){
		Vector3 pos = tf.localPosition;
		pos.y = newY;
		tf.localPosition = pos;
	}

	public static void SetLocalZ(this Transform tf, float newZ){
		Vector3 pos = tf.localPosition;
		pos.z = newZ;
		tf.localPosition = pos;
	}

	public static void SetLocalXY(this Transform tf, float newX, float newY){
		Vector3 pos = tf.localPosition;
		pos.x = newX;
		pos.y = newY;
		tf.localPosition = pos;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Shift X/Y/Z
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void ShiftX(this Transform tf, float shiftAmt){
		Vector3 pos = tf.position;
		pos.x += shiftAmt;
		tf.position = pos;
	}

	public static void ShiftY(this Transform tf, float shiftAmt){
		Vector3 pos = tf.position;
		pos.y += shiftAmt;
		tf.position = pos;
	}

	public static void ShiftZ(this Transform tf, float shiftAmt){
		Vector3 pos = tf.position;
		pos.z += shiftAmt;
		tf.position = pos;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Shift X/Y/Z
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void ShiftLocalX(this Transform tf, float shiftAmt){
		Vector3 pos = tf.localPosition;
		pos.x += shiftAmt;
		tf.localPosition = pos;
	}

	public static void ShiftLocalY(this Transform tf, float shiftAmt){
		Vector3 pos = tf.localPosition;
		pos.y += shiftAmt;
		tf.localPosition = pos;
	}

	public static void ShiftLocalZ(this Transform tf, float shiftAmt){
		Vector3 pos = tf.localPosition;
		pos.z += shiftAmt;
		tf.localPosition = pos;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Flip X/Y/Z
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public static void FlipX(this Transform tf){
		Vector3 scale = tf.localScale;
		scale.x *= -1f;
		tf.localScale = scale;
	}

	public static void FlipY(this Transform tf){
		Vector3 scale = tf.localScale;
		scale.y *= -1f;
		tf.localScale = scale;
	}

	public static void FlipZ(this Transform tf){
		Vector3 scale = tf.localScale;
		scale.z *= -1f;
		tf.localScale = scale;
	}


	#endregion


	#region list methods
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Shuffle List
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public static List<Object> shuffleList(this List<Object> l){
		//Fisher-Yates
		int n = l.Count;
		for (int i = 0; i < n; i++)
		{
			// NextDouble returns a random number between 0 and 1.
			// ... It is equivalent to Math.random() in Java.
			int r = i + (int)(Random.Range(0f,1f) * (n - i));
			Object t = l[r];
			l[r] = l[i];
			l[i] = t;
		}
		return l;
	}
	public static List<T> ShuffleList<T>(this List<T> list)
	{
		int n = list.Count;
		for (int i = 0; i < n; i++)
		{
			int r = i + (int)(Random.Range(0f, 1f) * (n - i));
			T temp = list[r];
			list[r] = list[i];
			list[i] = temp;
		}
		return list;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Get Random Element from List
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public static Object getRandomElement(this List<Object> l){
		//Fisher-Yates
		int n = Random.Range(0,l.Count-1);
		return l[n];
	}

	#endregion

	#region vector methods

	public static Vector3 SetX(this Vector3 vector, float x)
	{
		vector.x = x;
		return vector;
	}
	public static Vector3 SetY(this Vector3 vector, float y)
	{
		vector.y = y;
		return vector;
	}

	public static Vector3 SetZ(this Vector3 vector, float z)
	{
		vector.z = z;
		return vector;
	}
	public static Vector3 UnitDirectionVector(this Vector3 vector, float degrees){
		vector.x = Mathf.Cos(degrees * Mathf.Deg2Rad);
		vector.y = Mathf.Sin (degrees * Mathf.Deg2Rad);
		return vector;
	}
	public static Vector3 ToVector3(this Vector2 v, float z = 0f)
	{
		return new Vector3(v.x, v.y, z);
	}
	public static Vector2 SetX(this Vector2 vector, float x)
	{
		vector.x = x;
		return vector;
	}
	public static Vector2 SetY(this Vector2 vector, float y)
	{
		vector.y = y;
		return vector;
	}
	public static Vector2 ToVector2(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}
	#endregion

	#region Color

	public static Color SetAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}

	#endregion
	
	public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
	{
		if (Mathf.Approximately(inMax, inMin))
			return outMin;

		return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
	}
}
