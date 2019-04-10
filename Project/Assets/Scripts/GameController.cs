using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject TerrainGeneratorGO;

    public GameObject UnitManagerGO;
    private UnitManager UnitManager;

    //this would be great if it was a 2D array of tiles
    public GameObject[,] TheGrid;

    public GameObject GameOverText;


    float time_held = 0;
    bool drag_and_drop = false;
    Vector2 DragOrigin;
    Vector2 DragEnd;


    void Start ()
    {
        TerrainGenerator terrain_generator = TerrainGeneratorGO.GetComponent<TerrainGenerator>();
        TheGrid = terrain_generator.GenerateTerrain();

        UnitManager = UnitManagerGO.GetComponent<UnitManager>();

        TheGrid = UnitManager.placeUnits(TheGrid, terrain_generator.map_width, terrain_generator.map_height);
	}

    private void Update()
    {
        if (drag_and_drop == true)
        {
            time_held += Time.deltaTime;
        }
        else
        {
            time_held = 0;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
            if (hit)
            {
                Debug.Log(hit.transform.name);
                if (hit.transform.tag == "RedUnit")
                {
                    UnitManager.SelectUnit(hit.transform.gameObject);
                }
            }

            drag_and_drop = true;
            DragOrigin = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
        }

        if (Input.GetMouseButtonUp(0))
        {
            drag_and_drop = false;
            if (time_held > 0.3f)
            {
                DragEnd = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

                //UnitManager.SelectUnit(DragOrigin, DragEnd);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);
            if (hit)
            {
                Debug.Log(hit.transform.name);
                if (hit.transform.tag == "Tile")
                {
                    UnitManager.MoveSelected(hit.transform.gameObject.GetComponent<Tile>().TileCoords);
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void GameOver(Team winningUnitTeam)
    {
        if (winningUnitTeam == Team.BLUE)
        {
            GameOverText.GetComponent<Text>().color = Color.blue;
            GameOverText.GetComponent<Text>().text = "Blue Team Wins!";

        }
        else
        {
            //red
            GameOverText.GetComponent<Text>().color = Color.red;
            GameOverText.GetComponent<Text>().text = "Red Team Wins!";
        }

        StartCoroutine(EndGame(5.0f));
    }

    public IEnumerator EndGame(float time_till_end)
    {
        yield return new WaitForSeconds(time_till_end);
        SceneManager.LoadScene(0);
    }
}
