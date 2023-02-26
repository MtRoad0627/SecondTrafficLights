using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class TrafficLightsSystem : MonoBehaviour
    {
        [Tooltip("�Ή�����M���@")]
        [SerializeField] private TrafficLight[] trafficLights;

        private Dictionary<Road, TrafficLight> correspondingTrafficLight = new Dictionary<Road, TrafficLight>();

        public enum InitiallyGreen
        {
            odd,
            even
        }

        [Tooltip("������ԂŗΐM���ɂȂ����")]
        [SerializeField] private InitiallyGreen initiallyGreen = InitiallyGreen.even;

        /// <summary>
        /// �N���ς݂�TrafficLight��o�^�B�eTrafficLight�̏���������������
        /// </summary>
        /// <param name="roads">���v���ɓo�^���邱��</param>
        public void RegisterTrafficLights(Road[] roads, Dictionary<Road, int> edges)
        {
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
                    case InitiallyGreen.even:
                        if (cnt % 2 == 0)
                        {
                            trafficLights[cnt].SetLight(TrafficLight.Color.green);
                        }
                        else
                        {
                            trafficLights[cnt].SetLight(TrafficLight.Color.red);
                        }
                        break;

                    case InitiallyGreen.odd:
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
        }
    }
}