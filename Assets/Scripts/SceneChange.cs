using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public void changeScene(string sceneName)
    {
        // 버튼 클릭 사운드
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        // GameScene으로 갈 때 기존 GameManager가 있으면 파괴
        if (sceneName == "GameScene" && GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        SceneManager.LoadScene(sceneName);
    }

    public void exitGame()
    {
        // 버튼 클릭 사운드
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        Application.Quit();
    }
}
