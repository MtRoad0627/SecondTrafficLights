using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InGame
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        public enum Sequence
        {
            countDown,
            playing,
            gameFinished
        }

        public Sequence sequence { get; private set; } = Sequence.countDown;

        /// <summary>
        /// ���_
        /// </summary>
        public static int score { get; private set; } = 0;

        [Header("�X�R�A�֌W")]

        [Tooltip("�ō����x�Ƃ̍��ɉ��悷�邩")]
        [SerializeField] private float scoreExponent = 2f;

        [Tooltip("���_��P���g�傷��")]
        [SerializeField] private float scoreCoef = 100f;

        [Header("�^�C��")]

        [Tooltip("�Q�[�����ԁi�b�j")]
        [SerializeField] private float gameTime = 61f;

        [Header("�Q�[���I��")]
        
        [Tooltip("�Q�[���I�����Ă��烊�U���g��ʂɈڂ�܂ł̎���")]
        [SerializeField] private float gameFinishedWait = 3f;

        [Tooltip("���U���g��ʂ̃V�[����")]
        [SerializeField] private string resultSceneName = "Result";

        public float countDownTimeLeft { get; private set; } = 3f;

        public float gameTimeLeft { get; private set; } = 61f;

        //�STrafficLightsSystem���������ς݂�
        private bool afterConnectionInitialized = false;

        private void Start()
        {
            //�ϐ�������
            gameTimeLeft = gameTime;
            score = 0;
        }

        private void Update()
        {
            //TrafficLightsSystem��������
            //Start()�ɒu����Road�ڑ��ɐ��肵�Ă��܂����߂����ɔz�u
            if (!afterConnectionInitialized)
            {
                InitilizeAfterRoadConnection();
            }

            ManageTime();
        }

        /// <summary>
        /// �S���H���ڑ��ς݂��m�F���āA���̌�ɏ��������K�v�ȃI�u�W�F�N�g��������������
        /// </summary>
        private void InitilizeAfterRoadConnection()
        {
            //���ڑ��̓��H�����݂��Ă���΃L�����Z��
            Road[] allRoads = FindObjectsOfType<Road>();
            foreach(Road road in allRoads)
            {
                if (!road.isInitialized)
                {
                    //>>���ڑ�
                    return;
                }
            }

            //>>�S���H���ڑ��ς�
            
            //�����_�E�M���@��������
            InitializeIntersections();

            //CarGenerator��������
            CarGenerator.Initialize();

            //Navigator��������
            Navigator.Instance.SetUp();
        }

        /// <summary>
        /// �STrafficLightsSystem��������������
        /// </summary>
        private void InitializeIntersections()
        {
            //�SRoadJoints�̎��v���\�[�g���ς܂���
            //+�STrafficLightsSystem��������������
            RoadJoint[] allJoints = FindObjectsOfType<RoadJoint>();
            foreach (RoadJoint roadJoint in allJoints)
            {
                roadJoint.ArrangeRoadsAnticlockwise();
            }

            //�������ς݂�
            afterConnectionInitialized = true;
        }

        /// <summary>
        /// �c�莞�Ԃ̊Ǘ�
        /// </summary>
        private void ManageTime()
        {
            switch (sequence)
            {
                case Sequence.countDown:
                    countDownTimeLeft -= Time.deltaTime;

                    //���Ԃ�������playing�V�[�P���X�ɐ؂�ւ�
                    if(countDownTimeLeft <= 0f)
                    {
                        TransferToPlaying();
                    }
                    break;

                case Sequence.playing:
                    gameTimeLeft -= Time.deltaTime;

                    if(gameTimeLeft <= 0f)
                    {
                        TransferToFinished();
                    }
                    break;
            }
        }

        /// <summary>
        /// Playing�V�[�P���X�Ɉڍs
        /// </summary>
        private void TransferToPlaying()
        {
            sequence = Sequence.playing;

            UIManager.Instance.OnCountDownFinished();
        }

        /// <summary>
        /// Finished�V�[�P���X�Ɉڍs
        /// </summary>
        private void TransferToFinished()
        {
            sequence = Sequence.gameFinished;

            Invoke(nameof(GotoResult), gameFinishedWait);

            UIManager.Instance.OnGameFinished();
        }

        /// <summary>
        /// �ԓ��B���ɉ��_
        /// </summary>
        public void OnCarArrived(float speedAverage, float speedMax)
        {
            //Playing�V�[�P���X�̂�
            if(sequence != Sequence.playing)
            {
                return;
            }

            //��������_���邩�v�Z
            int scoreAddition = CalculatePoint(speedAverage, speedMax);

            //���_
            AddPoint(scoreAddition);
        }

        /// <summary>
        /// ���σX�s�[�h�𓾓_�ɕϊ�����
        /// </summary>
        private int CalculatePoint(float speedAverage, float speedMax)
        {
            float difference = speedAverage�@/ speedMax;
            float powered = Mathf.Pow(difference, scoreExponent);

            return (int)(powered * scoreCoef);
        }

        /// <summary>
        /// ���_
        /// </summary>
        private void AddPoint(int addition)
        {
            //���_
            score += addition;

            //UI�X�V
            UIManager.Instance.OnPointsChanged(addition);
        }

        /// <summary>
        /// Result��ʂ֑J��
        /// </summary>
        private void GotoResult()
        {
            SceneManager.LoadScene(resultSceneName);
        }
    }
}