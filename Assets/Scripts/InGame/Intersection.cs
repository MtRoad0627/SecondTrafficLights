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
        [SerializeField] private TrafficLightsSystem trafficLightSystem;

        /// <summary>
        /// TrafficLightsSystem�ɓ��H�A�M���@��o�^������
        /// Road���o�^�����O��Start�ɓo�^����ƕs�����������̂ŁA���̌�ɌĂԕK�v������
        /// </summary>
        public override void ArrangeRoadsClockwise()
        {
            base.ArrangeRoadsClockwise();

            //�M���@�������ꍇ�̓L�����Z��
            if (trafficLightSystem == null)
            {
                return;
            }

            //TrafficLightsSystem�ɓo�^
            trafficLightSystem.RegisterTrafficLights(connectedRoads.ToArray(), edges);
        }
    }
}