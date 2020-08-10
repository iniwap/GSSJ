using System;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class FillScene : EditorWindow {
	private const int totalObjects = 50000;
	private const int objectsPerGroup = 15;
	private const int maxDepth = 5;
	private const float density = 0.5f;

	[MenuItem("Window/UniMerge/Fill large scene")]
	static void Init() {
		GetWindow(typeof(FillScene));
	}

	void OnGUI() {
		if (GUILayout.Button("Generate Objects")) {
			Transform currentParent = null;
			int count = 0;
			int depth = 0;
			while (count++ < totalObjects) {
				GameObject g = new GameObject();
				g.name = RandString(15);
				g.transform.parent = currentParent;
				if (depth < maxDepth) {
					if (Random.value > density) {
						currentParent = g.transform;
						depth++;
					}
				}
				if (currentParent) {
					if (currentParent.childCount > objectsPerGroup) {
						currentParent = currentParent.parent;
						depth--;
					}
				}
			}
		}
	}

	string RandString(int length) {
		StringBuilder sb = new StringBuilder(length);
		for (int i = 0; i < length; i++) {
			sb.Append((char) Random.Range(97, 122));
		}
		return sb.ToString();
	}
}
