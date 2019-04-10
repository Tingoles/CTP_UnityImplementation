using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private List<Squad> squads = new List<Squad>();
    public Team team;

    [SerializeField]
    private bool attack = false;

    public void GiveSquads(List<GameObject> _squads)
    {
        //as in assign our squads their unit

        foreach (GameObject squad in _squads)
        {
            if (attack)
            {
                squad.GetComponent<Squad>().orders = SQUADORDER.ATTACKFLAG;
            }
            else
            {
                squad.GetComponent<Squad>().orders = SQUADORDER.DEFENDFLAG;
            }
            squads.Add(squad.GetComponent<Squad>());
        }

        //assign roles

        //squads[0].orders = SQUADORDER.DEFENDFLAG;
        //squads[1].orders = SQUADORDER.ATTACKFLAG;

        List<GameObject> EnemyUnitsGO;
        List<Unit> EnemyUnits = new List<Unit>();
        if (team == Team.RED)
        {
            EnemyUnitsGO = new List<GameObject>(GameObject.FindGameObjectsWithTag("BlueUnit"));
        }
        else
        {
            EnemyUnitsGO = new List<GameObject>(GameObject.FindGameObjectsWithTag("RedUnit"));
        }

        foreach (GameObject GO in EnemyUnitsGO)
        {
            EnemyUnits.Add(GO.GetComponent<Unit>());
        }


        //give our units enemy lists
        foreach (Squad squad in squads)
        {
            foreach (GameObject Unit in squad.SquadUnits)
            {
                Unit.GetComponent<Unit>().EnemyList = EnemyUnits;
            }
        }
    }
}
