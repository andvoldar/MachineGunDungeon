// Assets/Editor/DungeonsGeneratorEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// OJO: si tu clase se llama distinto o está en un namespace, ajusta el typeof y/o añade using
// using TuNombreDeNamespace;

[CustomEditor(typeof(DungeonsGenerator))]
public class DungeonsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);

        var dg = (DungeonsGenerator)target;

        if (GUILayout.Button("Generate Dungeon"))
        {
            dg.GenerateDungeon();
        }

        if (GUILayout.Button("Clear Dungeon"))
        {
            dg.ClearDungeon();
        }
    }
}
#endif
