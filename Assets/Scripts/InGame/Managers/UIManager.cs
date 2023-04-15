using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InGame.UI;

namespace InGame
{
    /// <summary>
    /// UI�̓������s��
    /// </summary>
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        [Header("�I�u�W�F�N�g�w��")]
        
        [Tooltip("���_�\��UI")]
        [SerializeField] private ScoreUI scoreUI;

        /// <summary>
        /// ���_�X�V
        /// </summary>
        public void OnPointsChanged(int changement)
        {
            //�X�V��̓��_���擾
            int currentPoints = GameManager.Instance.score;

            //�X�V������
            scoreUI.UpdateScore(currentPoints);
        }
    }
}