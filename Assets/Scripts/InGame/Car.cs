using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class Car : MonoBehaviour
    {
        /// <summary>
        /// �������ꂽRoadJoint
        /// </summary>
        public RoadJoint spawnPoint { get; private set; }

        /// <summary>
        /// �ړI�nRoadJoint
        /// </summary>
        public RoadJoint destination { get; private set; }

        /// <summary>
        /// �������̏���������
        /// </summary>
        public void Initialize(RoadJoint spawnPoint, RoadJoint destination = null)
        {
            //�����|�C���g
            this.spawnPoint = spawnPoint;

            //�ړI�n
            if (destination == null)
            {
                //������ŖړI�n��ݒ�
                this.destination = GetDestination(this.spawnPoint);
            }
            else
            {
                //�ړI�n���w�肳��Ă���
                this.destination = destination;
            }
            
        }

        /// <summary>
        /// �����_���ɖړI�n�����߂�
        /// </summary>
        private RoadJoint GetDestination(RoadJoint spawnPoint)
        {
            //spawnPoint�Ƃ̏d��������郋�[�v
            RoadJoint destination;
            OutsideConnection[] outsideConnectionsAll = FindObjectsOfType<OutsideConnection>();
            while (true)
            {
                //�����_���ɖړI�n�����߂�
                destination = outsideConnectionsAll[Random.Range(0, outsideConnectionsAll.Length - 1)];

                //�d���`�F�b�N
                if (destination != spawnPoint)
                {
                    //>>�����|�C���g�ƖړI�n���d�����Ă��Ȃ�
                    break;
                }
            }

            return destination;
        }


    }
}