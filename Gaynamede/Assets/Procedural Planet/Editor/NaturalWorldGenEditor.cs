using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(NaturalWorldGen))]
public class NaturalWorldGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NaturalWorldGen myScript = (NaturalWorldGen)target;
        if(GUILayout.Button("Build Object"))
        {
            myScript.setup();
        }
        if(GUILayout.Button("Test Build"))
        {
            myScript.displayMap ();
        }
    }
}