using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Opening
{
    public class SelectionButton : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        /// <summary>
        /// クリックされた際の処理
        /// </summary>
        public void OnClick()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}