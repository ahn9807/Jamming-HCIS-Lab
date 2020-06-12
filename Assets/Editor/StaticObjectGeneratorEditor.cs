using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StaticObjectGenerator))]
public class StaticObjectGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StaticObjectGenerator objectGen = (StaticObjectGenerator)target;

        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
            objectGen.GenerateTerrainObjectAtEditor();
        }
    }
}
