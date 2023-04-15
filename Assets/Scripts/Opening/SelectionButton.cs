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
        /// �N���b�N���ꂽ�ۂ̏���
        /// </summary>
        public void OnClick()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}