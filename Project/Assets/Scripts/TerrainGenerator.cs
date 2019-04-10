using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject TilePrefab;

    public int map_width;
    public int map_height;

    [SerializeField]
    private float start_grass_chance;

    [SerializeField]
    private int sim_runs;

    Vector2 tile_size;

    [HideInInspector]
    public GameObject[,] Tiles;
    private TerrainType[,] newTiles;

    //private float delta_time = 0;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    RunSimulation();
        //}

        //delta_time += Time.deltaTime;
        //if (delta_time > 0.1f)
        //{
        //    RunSimulation();
        //    delta_time = 0;
        //}

    }

    public GameObject[,] GenerateTerrain()
    {
        Tiles = new GameObject[map_width, map_height];
        newTiles = new TerrainType[map_width, map_height];

        tile_size = TilePrefab.GetComponent<SpriteRenderer>().size * TilePrefab.transform.localScale;

        for (int i = 0; i < map_height; i++)
        {
            for (int j = 0; j < map_width; j++)
            {
                GameObject temp = Instantiate(TilePrefab);
                Tile TileData = temp.GetComponent<Tile>();
                TileData.tile_ref = (i * map_width) + j;
                temp.name = "Tile " + j + "," + i;

                TileData.TileCoords = new Vector2(j, i);

                temp.transform.position = gameObject.transform.position + new Vector3((tile_size.x * j), -(tile_size.y * i));

                temp.transform.parent = gameObject.transform;

                //Random passable/impassable (dead/alive)
                if(Random.Range(0.01f, 100) > (start_grass_chance * 100))
                {
                    TileData.setType(TerrainType.ROCK);
                }
                else
                {
                    TileData.setType(TerrainType.GRASS);
                }

                Tiles[j, i] = temp;
            }
        }

        for (int i = 0; i < sim_runs; i++)
        {
            RunSimulation();
        }
        return Tiles;
    }

    private void RunSimulation()
    {
        //Based off Conways Game of Life https://gamedevelopment.tutsplus.com/tutorials/generate-random-cave-levels-using-cellular-automata--gamedev-9664

        for (int i = 0; i < map_height; i++)
        {
            for (int j = 0; j < map_width; j++)
            {
                Tile CurrentTile = Tiles[j, i].GetComponent<Tile>();

                int num = checkPassableNeighbors(j, i);

                if (CurrentTile.terrainType == TerrainType.GRASS)
                {
                    if (num < 3)
                    {
                        newTiles[j, i] = TerrainType.ROCK;
                    }
                    else
                    {
                        newTiles[j, i] = TerrainType.GRASS;
                    }
                }
                else //if rock
                {
                    if (num > 3)
                    {
                        newTiles[j, i] = TerrainType.GRASS;
                    }
                    else
                    {
                        newTiles[j, i] = TerrainType.ROCK;
                    }
                }
            }
        }

        //apply
        for (int i = 0; i < map_height; i++)
        {
            for (int j = 0; j < map_width; j++)
            {
                Tiles[j, i].GetComponent<Tile>().setType(newTiles[j, i]);
            }
        }
    }
    private int checkPassableNeighbors(int row, int col)
    {
        int no_passable_neighbors = 0;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = row + i;
                int neighbour_y = col + j;
                //If we're looking at the middle point
                if (i == 0 && j == 0)
                {
                    //Do nothing, we don't want to add ourselves in!
                }
                //In case the index we're looking at it off the edge of the map
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= map_width || neighbour_y >= map_height)
                {
                    //no_passable_neighbors++;
                }
                //Otherwise, a normal check of the neighbour
                else if (Tiles[neighbour_x, neighbour_y].GetComponent<Tile>().terrainType == TerrainType.GRASS)
                {
                    no_passable_neighbors++;
                }
            }
        }

        return no_passable_neighbors;
    }
}
