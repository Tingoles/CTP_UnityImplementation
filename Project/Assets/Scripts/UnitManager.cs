using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private GameObject unit_prefab;
    [SerializeField]
    private GameObject flag_prebab;
    [SerializeField]
    private GameObject squad_prefab;

    [SerializeField]
    private GameObject red_team_container;
    [SerializeField]
    private GameObject blue_team_container;

    [SerializeField]
    private int squad_size;
    private List<GameObject> RedSquads = new List<GameObject>();
    private List<GameObject> BlueSquads = new List<GameObject>();

    public int no_red_units;
    public int no_blue_units;

    private List<GameObject> SelectedUnits = new List<GameObject>();

    public bool place_flags;
    private bool red_placed = false;
    private bool blue_placed = false;

    public GameObject[,] placeUnits(GameObject[,] TileArray, int width, int height)
    {
        //setup squads
        int no_red_squads = no_red_units/ squad_size;
        int no_blue_squads = no_blue_units / squad_size;

        for(int i = 0; i < no_red_squads; i++)
        {
            GameObject Squad = Instantiate(squad_prefab);
            Squad.gameObject.name = "Red Team Squad " + (i + 1);
            Squad.transform.parent = red_team_container.transform;
            Squad.GetComponent<Squad>().SquadTeam = Team.RED;
            RedSquads.Add(Squad);

        }
        for (int i = 0; i < no_blue_squads; i++)
        {
            GameObject Squad = Instantiate(squad_prefab);
            Squad.gameObject.name = "Blue Team Squad " + (i + 1);
            Squad.transform.parent = blue_team_container.transform;
            Squad.GetComponent<Squad>().SquadTeam = Team.BLUE;
            BlueSquads.Add(Squad);
        }

        //spawn red team
        int squad_no;
        for (int i = 0; i < no_red_units; i++)
        {
            bool found = false;

            squad_no = Mathf.FloorToInt(i / squad_size);

            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    if (TileArray[k, j].GetComponent<Tile>().getAvailible())
                    {
                        if (place_flags && !red_placed && i > (no_red_units/2))
                        {
                            //Red flag
                            red_placed = true;
                            GameObject redFlag = Instantiate(flag_prebab);
                            redFlag.name = "Red Flag";
                            redFlag.transform.position = TileArray[k, j].transform.position;
                            //TileArray[k, j].GetComponent<Tile>().unit_occupied = true;
                            redFlag.transform.parent = red_team_container.transform;
                            redFlag.GetComponent<Flag>().Grid_Pos = new Vector2(k, j);
                            redFlag.GetComponent<Flag>().SetTeam(Team.RED);
                            found = true;
                            i--;

                            foreach(GameObject squad in BlueSquads)
                            {
                                squad.GetComponent<Squad>().targetFlag = redFlag.GetComponent<Flag>();
                            }
                            break;
                        }
                        else
                        {
                            //Red Units
                            GameObject temp = Instantiate(unit_prefab);
                            temp.name = "Red " + i;
                            temp.tag = "RedUnit";
                            temp.transform.position = TileArray[k, j].transform.position;
                            TileArray[k, j].GetComponent<Tile>().unit_occupied = true;
                            temp.transform.parent = RedSquads[squad_no].transform;
                            temp.GetComponent<Unit>().Grid_Pos = new Vector2(k, j);
                            temp.GetComponent<Unit>().SetTeam(Team.RED);
                            temp.GetComponent<Unit>().unit_squad = RedSquads[squad_no].GetComponent<Squad>();
                            RedSquads[squad_no].GetComponent<Squad>().AssignUnit(temp);
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    break;
                }
            }
        }

        //spawn blue team
        for (int i = 0; i < no_blue_units; i++)
        {
            bool found = false;

            squad_no = Mathf.FloorToInt(i / squad_size);

            for (int j = (height-1); j > 0; j--)
            {
                for (int k = (width-1); k > 0; k--)
                {
                    if (TileArray[k, j].GetComponent<Tile>().getAvailible())
                    {
                        if (place_flags && !blue_placed)
                        {
                            //Blue Flag
                            blue_placed = true;
                            GameObject blueFlag = Instantiate(flag_prebab);
                            blueFlag.name = "Blue Flag";
                            blueFlag.transform.position = TileArray[k, j].transform.position;
                            //TileArray[k, j].GetComponent<Tile>().unit_occupied = true;
                            blueFlag.transform.parent = blue_team_container.transform;
                            blueFlag.GetComponent<Flag>().Grid_Pos = new Vector2(k, j);
                            blueFlag.GetComponent<Flag>().SetTeam(Team.BLUE);
                            found = true;
                            i--;

                            foreach (GameObject squad in RedSquads)
                            {
                                squad.GetComponent<Squad>().targetFlag = blueFlag.GetComponent<Flag>();
                            }
                            break;
                        }
                        else
                        {
                            //Blue Units
                            GameObject temp = Instantiate(unit_prefab);
                            temp.name = "Blue " + i;
                            temp.tag = "BlueUnit";
                            temp.transform.position = TileArray[k, j].transform.position;
                            TileArray[k, j].GetComponent<Tile>().unit_occupied = true;
                            temp.transform.parent = BlueSquads[squad_no].transform;
                            temp.GetComponent<Unit>().Grid_Pos = new Vector2(k, j);
                            temp.GetComponent<Unit>().SetTeam(Team.BLUE);
                            temp.GetComponent<Unit>().unit_squad = BlueSquads[squad_no].GetComponent<Squad>();
                            BlueSquads[squad_no].GetComponent<Squad>().AssignUnit(temp);
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    break;
                }
            }
        }
        red_team_container.GetComponent<BattleManager>().GiveSquads(RedSquads);
        blue_team_container.GetComponent<BattleManager>().GiveSquads(BlueSquads);

        return TileArray;
    }

    public void SelectUnit(GameObject _unit)
    {
        //reset the selected units and clear
        foreach(GameObject unit in SelectedUnits)
        {
            if (unit.GetComponent<Unit>().getTeam() == Team.RED)
            {
                unit.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                unit.GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
        SelectedUnits.Clear();

        //select unit and turn it white
        SelectedUnits.Add(_unit);
        foreach (GameObject unit in SelectedUnits)
        {
            unit.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    //public void SelectUnit(Vector2 DragStart, Vector2 DragEnd)
    //{
    //    foreach (GameObject unit in SelectedUnits)
    //    {
    //        if (unit.GetComponent<Unit>().getTeam() == Team.RED)
    //        {
    //            unit.GetComponent<SpriteRenderer>().color = Color.red;
    //        }
    //        else
    //        {
    //            unit.GetComponent<SpriteRenderer>().color = Color.blue;
    //        }
    //    }
    //    SelectedUnits.Clear();

    //    float low_x;
    //    float high_x;
    //    float low_y;
    //    float high_y;



    //    if(DragStart.x < DragEnd.x)
    //    {
    //        low_x = DragStart.x;
    //        high_x = DragEnd.x;
    //    }
    //    else
    //    {
    //        low_x = DragEnd.x;
    //        high_x = DragStart.x;
    //    }

    //    if (DragStart.y < DragEnd.y)
    //    {
    //        low_y = DragStart.y;
    //        high_y = DragEnd.y;
    //    }
    //    else
    //    {
    //        low_y = DragEnd.y;
    //        high_y = DragStart.y;
    //    }

    //    Rect SelectionBox = new Rect(low_x, low_y, high_x, high_y);

    //    Debug.Log(low_x);
    //    Debug.Log(high_x);
    //    Debug.Log(low_y);
    //    Debug.Log(high_y);


    //    foreach (GameObject _unit in RedSquads)
    //    {
    //        if (SelectionBox.Contains(_unit.transform.position))
    //        {
    //            SelectedUnits.Add(_unit);
    //        }
    //    }

    //    foreach (GameObject unit in SelectedUnits)
    //    {
    //        unit.GetComponent<SpriteRenderer>().color = Color.white;
    //    }
    //}

    public void MoveSelected(Vector2 Target)
    {
        foreach(GameObject unit in SelectedUnits)
        {
            unit.GetComponent<Unit>().Move(Target);
        }
    }
}
