using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 *      This class will be used to pass various data between scenes such as number of players, game mode/difficulty, player score...
 *      Open ended for the moment. More specificity will come as the game develops.
 */
public class DataTransfer : MonoBehaviour
{
    public static bool MultiPlayer { get; set; }
    public static int Player1_Score { get; set; }
    public static int Player2_Score { get; set; }
    public static bool onePlayerDead { get; set; }

    private static string nameOfPlayerDead;
    private static DataTransfer instance = null;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        onePlayerDead = false;
        nameOfPlayerDead = "";
    }

    public static void KillPlayer(string objectName, int score)
    {
        if (nameOfPlayerDead == "")
        {
            nameOfPlayerDead = objectName;
            EnterScore(objectName, score);
        }
        else if(nameOfPlayerDead != objectName)
        {
            EnterScore(objectName, score);
            SceneManager.LoadScene("GameOver");
        }
    }

    private static void EnterScore(string objectName, int score)
    {
        if (objectName.Contains("1"))
            Player1_Score = score;
        else
            Player2_Score = score;
    }
}
