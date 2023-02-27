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

        public enum States
        {
            initializing,
            still,
            yellowChanging
        }

        public States state { get; private set; } = States.initializing;

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

        [Tooltip("���F�M���ɂȂ��Ă��鎞�ԁi�b�j")]
        [SerializeField] private float yellowTime = 1.5f;

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

            //����������
            state = States.still;
        }

        /// <summary>
        /// TrafficLight�̏����F���Z�b�g
        /// �������邢�͊��TrafficLight��΂ɂ��A�c���Ԃɂ���
        /// </summary>
        private void SetInitialLight()
        {
            //�M���@�̐F���Z�b�g
            SetLightsStill(initiallyGreen);

            //�p�^�[�����L��
            currentPattern = initiallyGreen;
        }

        /// <summary>
        /// Still��ԁi�΁E�ԁj�̃p�^�[���ɂȂ�悤�ɐM���@�̐F���Z�b�g����
        /// </summary>
        private void SetLightsStill(GreenPattern greenPattern)
        {
            //�΁E�ԐM�����擾
            TrafficLight[] greens = GetGreenLightsInPattern(greenPattern);
            TrafficLight[] reds = GetRedLightsInPattern(greenPattern);

            //�ΐM�����Z�b�g
            foreach(TrafficLight light in greens)
            {
                light.SetLight(TrafficLight.Color.green);
            }

            //�ԐM�����Z�b�g
            foreach(TrafficLight light in reds)
            {
                light.SetLight(TrafficLight.Color.red);
            }
        }

        /// <summary>
        /// YellowChanging��ԁi���F�E�ԁj�̃p�^�[���ɂȂ�悤�ɐM���@�̐F���Z�b�g����
        /// </summary>
        private void SetLightsYellow(GreenPattern greenPattern)
        {
            //���E�ԐM�����擾
            //GreenPattern�ɂ�����΂����F�ɃZ�b�g�ɂ���΂���
            TrafficLight[] yellows = GetGreenLightsInPattern(greenPattern);
            TrafficLight[] reds = GetRedLightsInPattern(greenPattern);

            //���M�����Z�b�g
            foreach (TrafficLight light in yellows)
            {
                light.SetLight(TrafficLight.Color.yellow);
            }

            //�ԐM�����Z�b�g
            foreach (TrafficLight light in reds)
            {
                light.SetLight(TrafficLight.Color.red);
            }
        }

        /// <summary>
        /// �^����ꂽ�p�^�[���ŗΐM���ɂȂ�M����Ԃ�
        /// </summary>
        private TrafficLight[] GetGreenLightsInPattern(GreenPattern greenPattern)
        {
            List<TrafficLight> output = new List<TrafficLight>();

            //���`�T��
            for (int cnt = 0; cnt < trafficLights.Length; cnt++)
            {
                switch (greenPattern)
                {
                    case GreenPattern.even:
                        if (cnt % 2 == 0)
                        {
                            output.Add(trafficLights[cnt]);
                        }
                        break;

                    case GreenPattern.odd:
                        if (cnt % 2 == 1)
                        {
                            output.Add(trafficLights[cnt]);
                        }
                        break;
                }
            }

            return output.ToArray();
        }

        /// <summary>
        /// �^����ꂽ�p�^�[���ŐԐM���ɂȂ�M���@��Ԃ�
        /// </summary>
        private TrafficLight[] GetRedLightsInPattern(GreenPattern greenPattern)
        {
            //�ΐM�����擾
            TrafficLight[] green = GetGreenLightsInPattern(greenPattern);

            //�]���ہi�ԁj���擾
            List<TrafficLight> output = new List<TrafficLight>();
            foreach(TrafficLight light in trafficLights)
            {
                if (!green.Contains(light))
                {
                    //>>�ΐM���ł͂Ȃ�

                    //�o�^
                    output.Add(light);
                }
            }

            return output.ToArray();
        }

        /// <summary>
        /// �M���@�؂�ւ�
        /// </summary>
        public void ToggleLights()
        {
            //Still��Ԃ���Ȃ���΃L�����Z��
            //���F�M�����ɏd�����󂯕t���Ȃ�
            if (state != States.still)
            {
                return;
            }

            //���F�M�����n�߂�
            StartYellow();

            //�M���؂�ւ���\��
            Invoke(nameof(StartNextPatternLights), yellowTime);
        }

        /// <summary>
        /// ���F�M�����I���A���̃p�^�[���̐M����\������
        /// </summary>
        public void StartNextPatternLights()
        {
            //�X�e�[�g��̃K�[�h����
            if (state != States.yellowChanging)
            {
                return;
            }

            //���̃p�^�[���ֈڂ�
            currentPattern = GetNextPattern(currentPattern);

            //���̃p�^�[���̗ΐԐM����\��
            SetLightsStill(currentPattern);

            //�ΐԃX�e�[�g��
            state = States.still;
        }

        /// <summary>
        /// ���F�M�����n�߂�
        /// </summary>
        private void StartYellow()
        {
            //���F�M���X�e�[�g��
            state = States.yellowChanging;

            //���F�M����
            SetLightsYellow(currentPattern);
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