using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        //�STrafficLightsSystem���������ς݂�
        private bool intersectionIsInitialized = false;

        private void Update()
        {
            //TrafficLightsSystem��������
            //Start()�ɒu����Road�ڑ��ɐ��肵�Ă��܂����߂����ɔz�u
            if (!intersectionIsInitialized)
            {
                InitilizeTrafficLightsSystems();
            }
        }

        /// <summary>
        /// �S���H���ڑ��ς݂��m�F���āA�ڑ��ς݂Ȃ�STrafficLightsSystem��������������
        /// </summary>
        private void InitilizeTrafficLightsSystems()
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

            //�STrafficLightsSystem��������������
            Intersection[] allIntersections = FindObjectsOfType<Intersection>();
            foreach(Intersection intersection in allIntersections) 
            {
                intersection.InitializeTrafficLightSystem();
            }

            //�������ς݂�
            intersectionIsInitialized = true;
        }
    }
}