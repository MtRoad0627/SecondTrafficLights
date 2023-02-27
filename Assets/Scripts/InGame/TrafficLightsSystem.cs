using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace InGame
{
    /// <summary>
    /// Intersection�Ɉ�Έ�Ή�
    /// ������TrafficLight�����܂Ƃ߂�
    /// �v���C���[�̓��͖͂{�N���X���󂯎��
    /// </summary>
    public class TrafficLightsSystem : MonoBehaviour
    {
        [Tooltip("�Ή�����M���@")]
        [SerializeField] private TrafficLight[] trafficLights;

        private Dictionary<Road, TrafficLight> correspondingTrafficLight = new Dictionary<Road, TrafficLight>();

        /// <summary>
        /// �ΐF�ɂȂ��Ă���M���@�̑g�ݍ��킹
        /// </summary>
        public enum GreenPattern
        {
            odd,
            even
        }

        [Tooltip("������ԂŗΐM���ɂȂ����")]
        [SerializeField] private GreenPattern initiallyGreen = GreenPattern.even;

        //���ݗ΂ɂȂ��Ă���p�^�[��
        private GreenPattern currentPattern;

        //���F�M��

        /// <summary>
        /// �N���ς݂�TrafficLight��o�^�B�eTrafficLight�̏���������������
        /// </summary>
        /// <param name="roads">���v���ɓo�^���邱��</param>
        public void RegisterTrafficLights(Road[] roads, Dictionary<Road, int> edges)
        {
            //5�ȏ�̐M���@�ɂ͑Ή��ł��Ȃ�
            Debug.Assert(roads.Length < 5);

            //���v��菇��TrafficLight���N���E�o�^
            trafficLights = new TrafficLight[roads.Length];
            for(int cnt = 0; cnt < roads.Length; cnt++)
            {
                //TrafficLight��o�^
                trafficLights[cnt] = roads[cnt].ActivateTrafficLight(edges[roads[cnt]]);

                //Road���o�^
                correspondingTrafficLight[roads[cnt]] = trafficLights[cnt];
            }

            //�����F���Z�b�g
            SetInitialLight();
        }

        /// <summary>
        /// TrafficLight�̏����F���Z�b�g
        /// �������邢�͊��TrafficLight��΂ɂ��A�c���Ԃɂ���
        /// </summary>
        private void SetInitialLight()
        {
            for (int cnt = 0; cnt < trafficLights.Length; cnt++)
            {
                switch (initiallyGreen)
                {
                    case GreenPattern.even:
                        if (cnt % 2 == 0)
                        {
                            trafficLights[cnt].SetLight(TrafficLight.Color.green);
                        }
                        else
                        {
                            trafficLights[cnt].SetLight(TrafficLight.Color.red);
                        }
                        break;

                    case GreenPattern.odd:
                        if (cnt % 2 == 0)
                        {
                            trafficLights[cnt].SetLight(TrafficLight.Color.green);
                        }
                        else
                        {
                            trafficLights[cnt].SetLight(TrafficLight.Color.red);
                        }
                        break;
                }
            }

            //�p�^�[�����L��
            currentPattern = initiallyGreen;
        }

        /// <summary>
        /// �M���@�؂�ւ�
        /// </summary>
        public void ToggleLights()
        {

        }

        /// <summary>
        /// �w�肳�ꂽ�p�^�[���̎��̃p�^�[����Ԃ�
        /// �w�肳�ꂽ�̂��Ō�Ȃ�΍ŏ��̃p�^�[����Ԃ�
        /// </summary>
        private GreenPattern GetNextPattern(GreenPattern pattern)
        {
            //���������Ԗڂ����擾����
            GreenPattern[] allPatterns = Enum.GetValues(typeof(GreenPattern)).Cast<GreenPattern>().ToArray();
            int thisIndex = Array.IndexOf(allPatterns, pattern);

            if (thisIndex < allPatterns.Length - 1)
            {
                return allPatterns[thisIndex + 1];
            }
            else
            {
                return allPatterns[0];
            }
        }
    }
}