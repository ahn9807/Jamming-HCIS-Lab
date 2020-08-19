using UnityEngine;
using System.Collections;

public class AnimalGenerator : MonoBehaviour
{
    public GameObject animalPrefab;
    public float sizeMultiplier;
    public float spwanDensity;
    public float minSpwanAltitude;
    public float maxSpwanAlitiude;
    public bool SpwanOnAir;
    public float altitude;

    int chunkSize = 95;
    int chunkNumber = 1;
    float chunkScale;
    float totalDensity;

    // Use this for initialization
    void Start()
    {
        chunkNumber = TerrainGenerator.chunkRenderNumber;
        chunkSize = MapGenerator.mapChunkSize;
        chunkScale = TerrainGenerator.scale;

        totalDensity = spwanDensity * spwanDensity / (chunkSize * chunkSize);

        GenerateAnimal();
    }

    public void GenerateAnimal()
    {
        int initialChunkCoordX = 0;
        int initialChunkCoordY = 0;

        for (int yOffset = -chunkNumber; yOffset <= chunkNumber; yOffset++)
        {
            for (int xOffset = -chunkNumber; xOffset <= chunkNumber; xOffset++)
            {
                Vector2 chunkCoord = new Vector2(initialChunkCoordX + xOffset, initialChunkCoordY + yOffset);
                GenerateAniamlAtChunk(chunkCoord);
            }
        }
    }

    public void GenerateAniamlAtChunk(Vector2 coord)
    {
        Vector2 position = coord * chunkSize;
        int yBound = Mathf.RoundToInt((position.x - chunkSize / 2f) * chunkScale);
        int xBound = Mathf.RoundToInt((position.y - chunkSize / 2f) * chunkScale);

        for (int yOffset = yBound; yOffset <= yBound + chunkSize * chunkScale; yOffset++)
        {
            for (int xOffset = xBound; xOffset <= xBound + chunkSize * chunkScale; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(xOffset, yOffset);
                if (Random.Range(0, 1f) < totalDensity)
                {
                    SpwanAnimal(viewedChunkCoord);
                }
            }
        }
    }

    // Update is called once per frame
    public GameObject SpwanAnimal(Vector2 position)
    {
        RaycastHit raycastHit;
        Vector3 origin = new Vector3(position.x, 100f, position.y);

        Vector3 spwanNormal;
        Vector3 spwanPosition;
        Quaternion spwanRotation;

        bool isHit = Physics.Raycast(origin, Vector3.down, out raycastHit, Mathf.Infinity);

        if(isHit == true)
        {
            spwanPosition = raycastHit.point;

            if (SpwanOnAir)
                spwanPosition = new Vector3(spwanPosition.x, altitude, spwanPosition.z);
            else if(spwanPosition.y > maxSpwanAlitiude || spwanPosition.y < minSpwanAltitude)
            {
                return null;
            }
            spwanNormal = raycastHit.normal;

            spwanRotation = Quaternion.FromToRotation(Vector3.up, 360 * spwanNormal) * animalPrefab.transform.rotation;
            spwanRotation *= Quaternion.Euler(0, Random.Range(0, 360f), 0);
        } else
        {
            return null;
        }

        //spwan Object
        GameObject animal = Instantiate(animalPrefab);
        animal.transform.parent = transform;
        animal.transform.position = spwanPosition;
        animal.transform.rotation = spwanRotation;
        animal.transform.localScale *= sizeMultiplier;

        return animal;
    }
}