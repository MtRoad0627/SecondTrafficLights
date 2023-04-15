using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        /// <summary>
        /// ���_
        /// </summary>
        public int score { get; private set; } = 0;

        [Header("�X�R�A�֌W")]

        [Tooltip("�ō����x�Ƃ̍��ɉ��悷�邩")]
        [SerializeField] private float scoreExponent = 2f;

        //�STrafficLightsSystem���������ς݂�
        private bool afterConnectionInitialized = false;

        private void Update()
        {
            //TrafficLightsSystem��������
            //Start()�ɒu����Road�ڑ��ɐ��肵�Ă��܂����߂����ɔz�u
            if (!afterConnectionInitialized)
            {
                InitilizeAfterRoadConnection();
            }
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
        /// �ԓ��B���ɉ��_
        /// </summary>
        public void OnCarArrived(float speedAverage, float speedMax)
        {
            //��������_���邩�v�Z
            int scoreAddition = (int)Mathf.Pow(speedMax - speedAverage, scoreExponent);

            //���_
            AddPoint(scoreAddition);
        }

        /// <summary>
        /// ���_
        /// </summary>
        private void AddPoint(int addition)
        {
            score += addition;
        }
    }
}