using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorJob : EditorWindow
{

    
    // Add a menu item to open the window
    [MenuItem("Window/Editor Jobs")]
    public static void ShowWindow()
    {
        GetWindow<EditorJob>("Editor Jobs");
    }

    private void OnGUI()
    {
        GUILayout.Label("This is Editor Script.", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Create a button to clear all PlayerPrefs
        if (GUILayout.Button("Clear All PlayerPrefs"))
        {
            // Delete all PlayerPrefs data
            PlayerPrefs.DeleteAll();
            // Log a message to the console when PlayerPrefs is cleared
            Debug.Log("Successfully cleared all PlayerPrefs.");
        }

        GUILayout.Space(5);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(5);
        
    }
}
