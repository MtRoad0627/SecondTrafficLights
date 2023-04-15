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

        [Header("���ԕ\��")]

        [SerializeField] private CountDownDisplay countDownDisplay;

        [SerializeField] private TimeDisplay timeDisplay;
        [SerializeField] private Transform timeDisplayParent;

        private GameObject currentTimeDisplayObject;

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

        /// <summary>
        /// �J�E���g�_�E���I��������
        /// </summary>
        public void OnCountDownFinished()
        {
            Destroy(countDownDisplay.gameObject);

            currentTimeDisplayObject = Instantiate(timeDisplay.gameObject, timeDisplayParent);
        }

        /// <summary>
        /// �Q�[���I��������
        /// </summary>
        public void OnGameFinished()
        {
            Destroy(currentTimeDisplayObject);
        }
    }
}