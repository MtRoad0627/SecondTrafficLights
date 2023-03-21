using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
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
                roadJoint.ArrangeRoadsClockwise();
            }

            //�������ς݂�
            afterConnectionInitialized = true;
        }
    }
}