using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour {

    public Text sp_scoresAre, mp_scoresAre, sp_p1_Text, mp_p2_Text, mp_scoreText_p1, mp_scoreText_p2;
    public Text sp_score, mp_score_p1, mp_score_p2;

    // Use this for initialization
    void Start () {

        if (DataTransfer.MultiPlayer)
        {
            sp_scoresAre.enabled = false;
            sp_p1_Text.enabled = false;
            sp_score.enabled = false;

            mp_score_p1.text = "" + DataTransfer.Player1_Score;
            mp_score_p2.text = "" + DataTransfer.Player2_Score;
        }
        else
        {
            mp_scoresAre.enabled = false;
            mp_p2_Text.enabled = false;
            mp_scoreText_p1.enabled = false;
            mp_scoreText_p2.enabled = false;

            sp_score.text = "" + DataTransfer.Player1_Score;
            mp_score_p1.enabled = false;
            mp_score_p2.enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
