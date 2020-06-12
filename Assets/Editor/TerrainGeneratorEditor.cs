using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGen = (TerrainGenerator)target;

        if (DrawDefaultInspector())
        {

        }

        if (GUILayout.Button("Generate"))
        {
            var generatedTerrain = GameObject.Find("Generated Terrain");
            if (generatedTerrain != null)
            {
                DestroyImmediate(generatedTerrain);
            }

            terrainGen.MakeTerrainAtEditor();
        }
    }
}
