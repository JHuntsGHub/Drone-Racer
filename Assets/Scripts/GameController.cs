using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

/*
 *  GameController() controlls the flow of the game. It's main function is managing the target rings.
 *  //TODO Later this class should take in all the Quadcopter's final scores and pass them to DataTransfer as well as set up the initial conditions of the race (ie number of players, single/muli player and vr/not vr.
 */
public class GameController : MonoBehaviour {

    /*
     *  The public (ie Dragged in from Unity) variable the GameController class needs are as follows:
     *      > clock  -->  This is to update each of the player's UI with the time played.
     */
    public Text clock;

    public static GameObject[] targets;
    public static bool cheatsOn { set; get; }
    private static int currentTarget { set; get; }
    private static int totalTargets { set; get; }
    private int lapNumber { set; get; }

    private DateTime startTime;
    

	// Use this for initialization
	private void Start () {
        cheatsOn = false;
        startTime = DateTime.Now;
        totalTargets = 0;
        currentTarget = 0;
        InitialiseTargets();
        lapNumber = 1;
	}

    /*
     *  InitialiseTargets() finds, sorts and places the target rings.
     */
    private void InitialiseTargets()
    {
        //  Firstly, I'm creating a temporary array of game objects. This will be looped through to determine how many targets are in the scene.
        GameObject[] temp = FindObjectsOfType<GameObject>();
        foreach(GameObject go in temp)
            if (go.tag == "Target") { totalTargets++; }

        //  Now that we know how many targets we have, we can initialise our targets array.
        targets = new GameObject[totalTargets];

        //  Next we add all the targets to the array in the correct order.
        foreach(GameObject go in temp)
            if(go.tag == "Target") { targets[Int32.Parse(go.name.Replace("Target_", ""))] = go; }

        //  Printing the contents of the array for testing.
        //foreach (GameObject target in targets)
        //    Debug.Log(target.name);

        //  Moves all targets except the first five well out of the players' view.
        for (int i = 5; i < targets.Length; i++)
            targets[i].transform.position += new Vector3(0, -100, 0);
    }
	
	// Update is called once per frame
	private void Update () {
        clock.text = Clock();
        Cheats();
	}

    /*
     * This allows the player to ignore angular velocity and damage.
     */
    private void Cheats()
    {
        if (Input.GetButtonDown("Restart"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.I))
            cheatsOn = !cheatsOn;
        if (Input.GetKeyDown(KeyCode.End))
            EndGame();
        if (Input.GetKeyDown(KeyCode.Delete))
            SceneManager.LoadScene("Menu");
    }

    /*
     *  Clock() returns a string showing how much time has passed since the Start() method was called.
     */
    private string Clock()
    {
        TimeSpan current = DateTime.Now.Subtract(startTime);
        string min, sec, milli;
        min = "" + current.Minutes;
        sec = "" + current.Seconds;
        milli = "" + current.Milliseconds;

        char[] charArray;

        if (min.Length == 1)
            min = "0" + min;

        if (sec.Length == 1)
            sec = "0" + sec;
        else if (sec.Length > 2)
        {
            charArray = sec.ToCharArray();
            sec = "" + charArray[charArray.Length - 3] + charArray[charArray.Length - 2];
        }

        if (milli.Length == 1)
            sec = "0" + milli;
        else if (milli.Length > 2)
        {
            charArray = milli.ToCharArray();
            milli = "" + charArray[charArray.Length - 3] + charArray[charArray.Length - 2];
        }
        
        return min + ":" + sec + ":" + milli;
    }

    /*
     *  RingManager() takes in a target ring.
     *  It checks to see if the target ring is the next in line to be hit, if so:
     *      > it removes that target from view
     *      > places the fifth next target in line into view.
     *  This means there will only be five visible targets at any given time.
     */
    public static void RingManager(GameObject target)
    {
        if (target.name == "Target_" + (currentTarget % totalTargets))
        {
            targets[currentTarget % totalTargets].GetComponentInChildren<AudioSource>().Play();
            targets[currentTarget % totalTargets].transform.position += new Vector3(0, -100, 0);
            currentTarget++;
            if (currentTarget + 4 < totalTargets * 3)
                targets[(currentTarget + 4) % totalTargets].transform.position += new Vector3(0, 100, 0);
            else if (currentTarget == totalTargets)
                EndGame();
            //Debug.Log("Target_" + ((currentTarget - 1) % totalTargets) + " hit and moved down. Target_" + ((currentTarget + 4) % totalTargets));
        }
    }

    /*
     *  EndGame() is called once all the targets have been hit (or if the 'end' key is pressed).
     *  //TODO This method is currently empty. Once multiplayer and AIQuad are implemented, the EndGame() method can be filled out properly.
     */
    private static void EndGame()
    {
        //DataTransfer.Player1_Score = 
    }
}
