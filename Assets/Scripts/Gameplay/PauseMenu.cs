using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZaoMeng.Gameplay
{
    /// <summary>
    /// ESC 暂停菜单：切 Time.timeScale，UI 面板负责显示。
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;   // 拖：暂停 UI 面板

        private bool paused;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            pausePanel.SetActive(paused);
        }

        public void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
        }

        /// <summary>重新开始本场景（重开）。</summary>
        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Main");
        }
    }
}