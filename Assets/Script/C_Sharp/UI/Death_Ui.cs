using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Death_Ui : MonoBehaviour
{
    [SerializeField] private GameObject LoadingScreenWidget;
    bool Is_ReGame;
    public void Re_Game()
    {
        Game_State_Manager.Instance.Setstate(GameState.Play);
        LoadingScreenWidget.GetComponent<LoadingSceneStstem>().LoadScene("Game_Level");
        Is_ReGame = true;
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        Game_State_Manager.Instance.Setstate(GameState.Pause);
    }

    private void OnEnable()
    {
        Game_State_Manager.Instance.Setstate(GameState.Pause);
        GetComponent<AudioSource>().Play();
    }

    private void OnDisable()
    {
        if(Is_ReGame)
            GameInstance.Reset_Gameinstance();
    }
}
