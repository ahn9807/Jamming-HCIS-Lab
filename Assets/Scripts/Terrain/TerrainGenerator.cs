using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public bool generatedAtInGame;

    public static Vector2 offetPosition;
    public const int chunkRenderNumber = 3;
    public Material mapMaterial;
    static MapGenerator mapGenerator;
    public const float scale = 1f;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    List<TerrainChunk> terrainChunkList = new List<TerrainChunk>();

    public void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        if(generatedAtInGame)
        {
            MakeChunks(MapGenerator.mapChunkSize - 1);
        }
    }

    public void MakeChunks(int chunkSize)
    {
        var terrainParentObject = new GameObject("Generated Terrain");
        terrainParentObject.transform.position = transform.position;
        terrainParentObject.transform.parent = transform.parent;

        int currentChunkCoordX = Mathf.RoundToInt(offetPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(offetPosition.y / chunkSize);


        for (int yOffset = -chunkRenderNumber; yOffset<=chunkRenderNumber; yOffset++)
        {
            for(int xOffset = -chunkRenderNumber;xOffset<=chunkRenderNumber; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                terrainChunkList.Add(new TerrainChunk(viewedChunkCoord, chunkSize, terrainParentObject.transform, mapMaterial));
            }
        }
    }

    public void MakeTerrainAtEditor()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        MakeChunks(MapGenerator.mapChunkSize - 1);
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = new Material(material);

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.sharedMaterial.mainTexture = texture;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
    }
}
