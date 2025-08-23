// Assets/Editor/DungeonsGeneratorEditor.cs
using UnityEditor;
using UnityEngine;

// Personaliza el Inspector para la clase DungeonsGenerator
[CustomEditor(typeof(DungeonsGenerator))]
public class DungeonsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Dibuja el inspector por defecto (campos públicos, Tooltips, etc.)
        DrawDefaultInspector();

        GUILayout.Space(8);

        var dg = (DungeonsGenerator)target;

        // Botón para generar mazmorra en el Editor
        if (GUILayout.Button("Generate Dungeon"))
        {
            dg.GenerateDungeon();
        }

        // Botón para limpiar mazmorra en el Editor
        if (GUILayout.Button("Clear Dungeon"))
        {
            dg.ClearDungeon();
        }
    }
}
