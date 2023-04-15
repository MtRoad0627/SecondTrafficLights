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
        [Header("���_�\��UI")]
        
        [SerializeField] private ScoreUI scoreUI;

        [Header("���_UI")]

        [Tooltip("�v���n�u")]
        [SerializeField] private ScoreAdditionUI scoreAdditionUIPrefab;

        [Header("�e")]
        [SerializeField] private Transform scoreAdditionUIParent;

        /// <summary>
        /// ���_�X�V
        /// </summary>
        public void OnPointsChanged(int changement)
        {
            //�X�V��̓��_���擾
            int currentPoints = GameManager.score;

            //�������_UI���X�V������
            scoreUI.UpdateScore(currentPoints);

            //���_UI����
            GenerateAdditionalPoint(changement);
        }

        /// <summary>
        /// ���_UI����
        /// </summary>
        private void GenerateAdditionalPoint(int additionalPoint)
        {
            //����
            GameObject ui = Instantiate(scoreAdditionUIPrefab.gameObject, scoreAdditionUIParent);

            //������
            ui.GetComponent<ScoreAdditionUI>().Initialize(additionalPoint);
        }
    }
}