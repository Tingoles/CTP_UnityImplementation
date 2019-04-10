using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    GRASS = 0,
    ROCK = 1
    //WATER = 2,
    //ENUM_SIZE = ROCK
}

public class Tile : MonoBehaviour
{
    public TerrainType terrainType;

    public bool unit_occupied = false;

    public Vector2 TileCoords;

    //pathfinding
    public int tile_ref;
    public Tile parent_tile;
    public float tile_cost;
    public float total_cost;
    public bool visited = false;


    public void setType(TerrainType type)
    {
        terrainType = type;

        setColour();        
    }

    void setColour()
    {
        switch (terrainType)
        {
            case TerrainType.GRASS:
                GetComponent<SpriteRenderer>().color = Color.green;
                break;
            case TerrainType.ROCK:
                GetComponent<SpriteRenderer>().color = Color.grey;
                break;
            //case TerrainType.WATER:
            //    GetComponent<SpriteRenderer>().color = Color.blue;
            //    break;
            default:
                Debug.Log("Terrain Enum Not Set Correctly");
                break;
        }
    }

    public bool getAvailible()
    {
        if(terrainType == TerrainType.ROCK)
        {
            return false;
        }
        else if(unit_occupied)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
