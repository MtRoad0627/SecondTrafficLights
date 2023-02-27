using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace InGame
{
    /// <summary>
    /// �����_�B
    /// �R�ȏ�̓��H���Ȃ�RoadJoint
    /// </summary>
    public class Intersection : RoadJoint
    {
        [Tooltip("�Ή�����M���@�V�X�e���Bnull�Ȃ�M���@�����̌����_")]
        [SerializeField] private TrafficLightsSystem? trafficLightSystem;

        /// <summary>
        /// TrafficLightsSystem�ɓ��H�A�M���@��o�^����
        /// ������TrafficLight��Activate���s����
        /// Road���o�^�����O��Start�ɓo�^����ƕs�����������̂ŁA���̌�ɌĂԕK�v������
        /// </summary>
        public void InitializeTrafficLightSystem()
        {
            //�M���@�������ꍇ�̓L�����Z��
            if(trafficLightSystem == null)
            {
                return;
            }

            //���v����Road����ׂ�
            Road[] orderedRoads = ArrangeRoadsClockwise(connectedRoads.ToArray());

            //TrafficLightsSystem�ɓo�^
            trafficLightSystem.RegisterTrafficLights(orderedRoads, edges);
        }

        /// <summary>
        /// Intersection���猩��road�����v���ɕ��ׂ�
        /// </summary>
        private Road[] ArrangeRoadsClockwise(Road[] roads)
        {
            //�eRoad�Ƃ�x������������̎��v���̊p�x�����߂�
            Dictionary<Road, float> angles = new Dictionary<Road, float>();
            foreach(Road road in roads)
            {
                //Road��Intersection�̍��x�N�g��
                Vector3 dif = road.transform.position - transform.position;

                //x���������Ƃ̕��������Ă��Ȃ��p�x�i�x�N�g���̂Ȃ��p�j
                float unsigned = Vector2.Angle(new Vector2(1, 0), dif); 

                if (dif.y >= 0)
                {
                    //Road��Intersection���㑤�ɂ���Ƃ��A�O�`�P�W�O�x�ɂȂ��Ă���Ƃ����180�x����
                    angles[road] = 360f - unsigned;
                }
                else
                {
                    //���ɂ���ꍇ�͂��̂܂�
                    angles[road] = unsigned;
                }
            }

            //�p�x�����������ɕ��ёւ�
            List<Road> orderedRoad = new List<Road>();
            foreach(KeyValuePair<Road, float> roadAngle in angles.OrderBy(c => c.Value))
            {
                orderedRoad.Add(roadAngle.Key);
            }

            return orderedRoad.ToArray();
        }
    }
}