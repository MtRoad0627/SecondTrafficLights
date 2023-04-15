using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Opening
{
    /// <summary>
    /// ��������V�[���J�ڂ���{�^��
    /// </summary>
    public class SceneButton : MonoBehaviour
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