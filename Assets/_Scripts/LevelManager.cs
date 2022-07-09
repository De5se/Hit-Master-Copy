using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    public static void Restart()
    {
        SceneManager.LoadScene($"Game");
    }

    public void StartGame()
    {
        playerController.enabled = true;
    }
}
