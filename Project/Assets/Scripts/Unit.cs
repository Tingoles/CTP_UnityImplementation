using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    NONE,
    RED,
    BLUE
}

public class Unit : MonoBehaviour
{
    [SerializeField]
    private Team unit_team;

    public Squad unit_squad;

    public int unit_index;

    public GameObject projectile_prefab;

    public Vector2 Grid_Pos;
    private Vector2 TargetGridPos;

    [SerializeField]
    private int health = 2;

    private GameController GameController;

    private int map_width;
    private int map_height;

    private bool move_coroutine_finished = true;
    public bool moving_to_cover = false;

    private List<Tile> OpenList = new List<Tile>();
    private List<Tile> ClosedList = new List<Tile>();

    private List<Tile> Path = new List<Tile>();

    //[HideInInspector]
    public List<Unit> EnemyList = new List<Unit>();
    public List<Unit> TargetsInRange = new List<Unit>();

    private float check_enemies_after = 1.5f;
    private float time_since_check = 0;

    private float fire_cooldown = 2;
    private float time_since_fire = 0;

    public float tile_cost_mod = 1;
    public float heuristic_cost_mod = 1;

    private GameObject Enemy_flag;

    private void Start()
    {
        GameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        map_width = GameController.TerrainGeneratorGO.GetComponent<TerrainGenerator>().map_width;
        map_height = GameController.TerrainGeneratorGO.GetComponent<TerrainGenerator>().map_height;
    }

    private void Update()
    {
        if (Enemy_flag == null)
        {
            GameObject[] flags = GameObject.FindGameObjectsWithTag("Flag");
            foreach (var flag in flags)
            {
                if (unit_team != flag.GetComponent<Flag>().getTeam())
                {
                    Enemy_flag = flag;
                }
            }
        }

        if(Path.Count > 0 && move_coroutine_finished == true)
        {
            move_coroutine_finished = false;
            StartCoroutine(MoveToNextPathPoint(0.5f));
        }
        else if (Path.Count == 0)
        {
            moving_to_cover = false;
        }

        time_since_check += Time.deltaTime;
        if (time_since_check > check_enemies_after)
        {
            TargetsInRange.Clear();
            time_since_check = 0;
            foreach (Unit enemy in EnemyList)
            { 
                if(Vector2.Distance(enemy.Grid_Pos, Grid_Pos) < 20 && enemy.gameObject.activeSelf)
                {
                    TargetsInRange.Add(enemy);
                }
            }
        }

        if(TargetsInRange.Count > 0 )
        {
            unit_squad.UnitFoundCombat(TargetsInRange[0].Grid_Pos, unit_index);

            int random = Random.Range(0, TargetsInRange.Count);

            time_since_fire += Time.deltaTime;
            if (fire_cooldown < time_since_fire)
            {
                time_since_fire = 0;
                FireAt(TargetsInRange[random]);
            }
        }
        else
        {
            unit_squad.CombatOver();
        }
    }

    private void FireAt(Unit target)
    {
        if (target != null)
        {
            GameObject projectile = Instantiate(projectile_prefab);
            projectile.transform.position = gameObject.transform.position;
            projectile.GetComponent<Rigidbody2D>().AddForce((target.transform.position - transform.position) * 2, ForceMode2D.Impulse);

            projectile.transform.up = target.transform.position - transform.position;

            Destroy(projectile, 0.5f);

            int hit_chance = 30;


            if (Random.Range(0, 100) < hit_chance)
            {
                target.Damage(1);
            }
        }
    }

    public void Damage(int damage)
    {
        health = health - damage;
        if(health < 1)
        {
            Die();
        }
    }

    private void Die()
    {
        unit_squad.UnitDeath(unit_index);
        if (Path.Count > 0)
        {
            Path[Path.Count-1].unit_occupied = false;
        }
        gameObject.SetActive(false);
    }

    public void SetTeam(Team team)
    {
        unit_team = team;
        switch (unit_team)
        {
            case Team.RED:
                GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case Team.BLUE:
                GetComponent<SpriteRenderer>().color = Color.blue;
                break;
            default:
                Debug.Log("Unit not assigned a team");
                break;
        }
    }

    public Team getTeam()
    {
        return unit_team;
    }

    public void Move(Vector2 _TargetGridPos)
    {
        //Debug.Log(gameObject.name + "Moving to: " + TargetGridPos.x + "," + TargetGridPos.y);


        GameController.TheGrid[(int)TargetGridPos.x, (int)TargetGridPos.y].GetComponent<Tile>().unit_occupied = false;

        TargetGridPos = _TargetGridPos;

        int safety = 0;
        while(!(GameController.TheGrid[(int)TargetGridPos.x, (int)TargetGridPos.y].GetComponent<Tile>().getAvailible()))
        {
            //cant move here, find close availible point
            if (TargetGridPos.x < 50)
            {
                TargetGridPos.x++;
            }
            else
            {
                TargetGridPos.x--;
            }
            if (TargetGridPos.y < 50)
            {
                TargetGridPos.y++;
            }
            else
            {
                TargetGridPos.y--;
            }

            TargetGridPos.x += Random.Range(-1, 1);
            TargetGridPos.y += Random.Range(-1, 1);

            safety++;

            if(safety > 20)
            {
                return;
            }
        }


        //update
        GameController.TheGrid[(int)Grid_Pos.x, (int)Grid_Pos.y].GetComponent<Tile>().unit_occupied = false;
        GameController.TheGrid[(int)TargetGridPos.x, (int)TargetGridPos.y].GetComponent<Tile>().unit_occupied = true;

        float tile_cost;
        float heuristic_cost;
        float total_cost;
        int list_index = 0;

        //the first origin tile
        Tile Origin_Tile = GameController.TheGrid[(int)Grid_Pos.x, (int)Grid_Pos.y].GetComponent<Tile>();

        //Clear from any previous moves
        OpenList.Clear();
        ClosedList.Clear();
        Path.Clear();

        OpenList.Add(Origin_Tile);

        int safety_iterator = 0;

        while ((Origin_Tile.TileCoords.x != TargetGridPos.x) || (Origin_Tile.TileCoords.y != TargetGridPos.y))
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int neighbour_x = (int)Origin_Tile.TileCoords.x + i;
                    int neighbour_y = (int)Origin_Tile.TileCoords.y + j;

                    if (i == 0 && j == 0) { } //this tile
                    else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= map_width || neighbour_y >= map_height) { } //checks if neighbor is off the map
                    else if (GameController.TheGrid[neighbour_x, neighbour_y].GetComponent<Tile>().visited == true) { } //checks essencially if on closed list
                    else if (GameController.TheGrid[neighbour_x, neighbour_y].GetComponent<Tile>().terrainType == TerrainType.GRASS) //finally, if not obstacle
                    {
                        Tile neigboring_tile = GameController.TheGrid[neighbour_x, neighbour_y].GetComponent<Tile>();

                        //Calc Tile Cost
                        if (j == 0 || i == 0)
                        {
                            tile_cost = Origin_Tile.tile_cost + (1f * tile_cost_mod);
                        }
                        else
                        {
                            //sqrt 2
                            tile_cost = Origin_Tile.tile_cost + (1.41f * tile_cost_mod);
                        }
                        float x_dis = Mathf.Abs(TargetGridPos.x - neighbour_x);
                        float y_dis = Mathf.Abs(TargetGridPos.y - neighbour_y);

                        heuristic_cost = Mathf.Sqrt((x_dis * x_dis) + (y_dis * y_dis)) * heuristic_cost_mod;
                        total_cost = tile_cost + heuristic_cost;

                        if (neigboring_tile.parent_tile == null)
                        {
                            neigboring_tile.parent_tile = Origin_Tile;

                            neigboring_tile.tile_cost = tile_cost;

                            neigboring_tile.total_cost = tile_cost + heuristic_cost;

                            //neigboring_tile.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                            OpenList.Add(neigboring_tile);
                        }
                        else
                        {
                            if (neigboring_tile.tile_cost > tile_cost)
                            {
                                neigboring_tile.tile_cost = tile_cost;
                                neigboring_tile.parent_tile = Origin_Tile;
                            }
                        }
                    }
                }
            }
            Origin_Tile.visited = true;
            
            ClosedList.Add(Origin_Tile);
            OpenList.Remove(Origin_Tile);

            float lowest_cost = 0;
            for (int i = 0; i < OpenList.Count; i++)
            {
                if (OpenList[i].total_cost < lowest_cost || lowest_cost == 0)
                {
                    lowest_cost = OpenList[i].total_cost;
                    list_index = i;
                }
            }
            Origin_Tile = OpenList[list_index];

            safety_iterator++;
            if(safety_iterator > 5000)
            {
                Debug.Log(gameObject.name + "stuck");
                break;
            }
        }
        //construct dat path
        Tile OnPath = Origin_Tile;

        while (OnPath.parent_tile != null)
        {
            Path.Add(OnPath);
            //OnPath.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            OnPath = OnPath.parent_tile;
        }
        Path.Reverse();


        //clean up
        foreach (Tile tile in OpenList)
        {
            tile.parent_tile = null;
        }
        foreach (Tile tile in ClosedList)
        {
            tile.parent_tile = null;
            tile.visited = false;
        }
    }

    IEnumerator MoveToNextPathPoint(float over_time)
    {
        transform.position = Path[0].transform.position;
        Grid_Pos = Path[0].TileCoords;
        Path.RemoveAt(0);

        yield return new WaitForSeconds(over_time);

        move_coroutine_finished = true;

        if (Enemy_flag)
        {
            if (Grid_Pos == Enemy_flag.GetComponent<Flag>().Grid_Pos)
            {
                GameController.GameOver(unit_team);
            }
        }
    }
}