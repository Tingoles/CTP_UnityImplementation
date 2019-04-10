using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    [SerializeField]
    private Team unit_team;

    public Vector2 Grid_Pos;

    public bool taken = false;

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

}
