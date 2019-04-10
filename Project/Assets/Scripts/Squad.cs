using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SQUADORDER
{
    ATTACKFLAG,
    DEFENDFLAG
}

public class Squad : MonoBehaviour
{
    private GameController GameController;
    int map_width;
    int map_height;

    public Team SquadTeam;

    public SQUADORDER orders = SQUADORDER.ATTACKFLAG;

    private bool needs_new_orders = true;

    public List<GameObject> SquadUnits = new List<GameObject>();

    public Flag targetFlag;

    private bool in_combat = false;

    private float time_since_last_order = 8;

    private float time_between_orders = 12;


    private void Start()
    {
        GameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        map_width = GameController.TerrainGeneratorGO.GetComponent<TerrainGenerator>().map_width;
        map_height = GameController.TerrainGeneratorGO.GetComponent<TerrainGenerator>().map_height;
    }

    private void Update()
    {
        time_since_last_order += Time.deltaTime;

        if (targetFlag)
        {
            if (!targetFlag.taken && needs_new_orders && (time_since_last_order > time_between_orders))
            {
                time_since_last_order = 0;
                needs_new_orders = false;

                switch (orders)
                {
                    case SQUADORDER.ATTACKFLAG:
                        {
                            for (int i = 0; i < SquadUnits.Count; i++)
                            {
                                SquadUnits[i].GetComponent<Unit>().Move(targetFlag.Grid_Pos);
                            }
                            break;
                        }
                    case SQUADORDER.DEFENDFLAG:
                        {
                            FindNearestCover(targetFlag.Grid_Pos);
                            break;
                        }
                    default:
                        Debug.Log("Invalid Order type, how have you managed this?");
                        break;
                }
            }

            if (needs_new_orders == false && time_since_last_order > 40)
            {
                orders = SQUADORDER.ATTACKFLAG;
                needs_new_orders = true;
            }
        }
    }

    public void UnitFoundCombat(Vector2 EnemyPos, int unit_index)
    {
        if(!in_combat)
        {
            in_combat = true;
            if (orders == SQUADORDER.ATTACKFLAG)
            {
                FindNearestCover(EnemyPos, unit_index);
            }
        }
    }

    public void UnitDeath(int unit_index)
    {
        
    }

    public void CombatOver()
    {
        if (in_combat)
        {
            in_combat = false;
            orders = SQUADORDER.ATTACKFLAG;
            needs_new_orders = true;
        }
    }

    public void FindNearestCover(Vector2 FromGridPos, int around_unit = 0)
    {
        //nearest cover to the passed in position

        int AroundUnit_x = (int)SquadUnits[around_unit].GetComponent<Unit>().Grid_Pos.x;
        int AroundUnit_y = (int)SquadUnits[around_unit].GetComponent<Unit>().Grid_Pos.y;

        float x_dis = AroundUnit_x - FromGridPos.x;
        float y_dis = AroundUnit_y - FromGridPos.y;

        List<Vector2> CoverPositions = new List<Vector2>();

        int x = 0;
        int y = 0;

        //Debug.Log(gameObject.name + " x: " + x_dis + " y: " + y_dis);

        if(Mathf.Abs(x_dis) > Mathf.Abs(y_dis))
        {
            //cover from horizontal
            if(x_dis > 0)
            {
                //cover from right
                x = -1;
            }
            else
            {
                //cover from left
                x = 1;
            }
        }
        else
        {
            //cover from vertical
            if (y_dis > 0)
            {
                //cover from down
                y = -1;
            }
            else
            {
                //cover from up
                y = 1;
            }
        }

        for (int i = -12; i < 13; i++)
        {
            for (int j = -12; j < 13; j++)
            {
                int tile_x = AroundUnit_x + i;
                int tile_y = AroundUnit_y + j;

                if (i == 0 && j == 0) { } //this tile
                else if (tile_x < 0 || tile_y < 0 || tile_x >= map_width || tile_y >= map_height) { } //checks if off the map
                else if (GameController.TheGrid[tile_x, tile_y].GetComponent<Tile>().getAvailible() == true)
                {
                    if (tile_x + x < 0 || tile_y + y < 0 || tile_x + x >= map_width || tile_y + y >= map_height) { } //checks if adjacent off map
                    else
                    {
                        if (GameController.TheGrid[tile_x + x, tile_y + y].GetComponent<Tile>().terrainType == TerrainType.ROCK)
                        {
                            CoverPositions.Add(new Vector2(tile_x, tile_y));
                        }
                    }
                }
            }
        }
        //Vector2.Distance(CoverPositions[0], new Vector2(squad_leader_x, squad_leader_y);

        CoverPositions.Sort(CoverSort);

        for(int i = 0; i < SquadUnits.Count; i++)
        {
            if (i < CoverPositions.Count)
            {
                if (SquadUnits[i].GetComponent<Unit>().moving_to_cover == false)
                {
                    SquadUnits[i].GetComponent<Unit>().Move(CoverPositions[i]);
                    SquadUnits[i].GetComponent<Unit>().moving_to_cover = true;
                }
            }
        }
    }

    public void AssignUnit(GameObject Unit)
    {
        Unit.GetComponent<Unit>().unit_index = SquadUnits.Count;
        SquadUnits.Add(Unit);
    }

    int CoverSort(Vector2 a, Vector2 b)
    {
        int squad_leader_x = (int)SquadUnits[0].GetComponent<Unit>().Grid_Pos.x;
        int squad_leader_y = (int)SquadUnits[0].GetComponent<Unit>().Grid_Pos.y;

        return Vector2.Distance(a, new Vector2(squad_leader_x, squad_leader_y)).CompareTo(Vector2.Distance(b, new Vector2(squad_leader_x, squad_leader_y)));
    }
}
