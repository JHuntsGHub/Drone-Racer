using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    public void SinglePlayerPressed()
    {
        DataTransfer.MultiPlayer = false;
        SceneManager.LoadScene("Track_1(Single)");
    }

    public void MultiPlayerPressed()
    {
        DataTransfer.MultiPlayer = true;
        SceneManager.LoadScene("Track_1(Multi)");
    }

    public void ControlsPressed()
    {
        SceneManager.LoadScene("Controls");
    }

    public void QuitPressed()
    {
        Debug.Log("Closing program.");
        Application.Quit();
    }

    public void MenuPressed()
    {
        SceneManager.LoadScene("Menu");
    }
}
