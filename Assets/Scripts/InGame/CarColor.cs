using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    /// <summary>
    /// �Ԃ̃J���[�����O
    /// Car�N���X�ɖ������ē��삷��B
    /// ���̃N���X���̂͐F��ς��Ȃ��i�p���N���X���F��ς���j
    /// </summary>
    public class CarColor : MonoBehaviour
    {
        /// <summary>
        /// �F���X�V
        /// </summary>
        /// <param name="happinessRatio">�����x�̍ő�l�ɑ΂��銄��</param>
        public void UpdateColor(float happinessRatio)
        {
            Color color = CalculateColor(happinessRatio);

            SetColor(color);
        }

        /// <summary>
        /// �K���x��F�ɕϊ�
        /// </summary>
        protected virtual Color CalculateColor(float happinessRatio)
        {
            //�΁�������

            float b = 0f;
            float r, g;

            if (happinessRatio < 0.5f)
            {
                r = 1f;
                g = happinessRatio / 0.5f;
            }
            else
            {
                g = 1f;
                r = (1f - happinessRatio) / 0.5f;
            }

            return new Color(r, g, b);
        }

        /// <summary>
        /// ���ۂɐF��ς���
        /// </summary>
        protected virtual void SetColor(Color color){}
    }
}
