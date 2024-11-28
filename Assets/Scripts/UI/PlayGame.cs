using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayGame : MonoBehaviour
{
    public void play1()
    {
        SceneManager.LoadScene("Scene1");
    }

    public void play2()
    {
        SceneManager.LoadScene("Scene2");
    }

    public void restart()
    {
        SceneManager.LoadScene("Scene0");
    }
}
