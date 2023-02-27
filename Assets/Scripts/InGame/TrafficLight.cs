using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    /// <summary>
    /// �M���@�B
    /// �e���H�����[�ɂQ���A���������ꂽ�M���@�����B
    /// TrafficLightsSystem������Intersection�ɐڑ����邱�ƂŁA�L���������B
    /// </summary>
    public class TrafficLight : MonoBehaviour
    {
        public enum Color
        {
            green,
            yellow,
            red
        }

        public Color color { get; private set; }

        //�e�M���\�����ɕ\������Sprite
        [System.Serializable]
        private class LightColor
        {
            public Color color;
            public Sprite sprite;
        }

        [Tooltip("�e�F�ɑΉ����ĕ\������X�v���C�g")]
        [SerializeField] private LightColor[] lightColors;

        /// <summary>
        /// �F��؂�ւ�
        /// </summary>
        public void SetLight(Color color)
        {
            //�F���Z�b�g
            this.color = color;

            //���̐F��\��
            ShowColor(color);
        }

        /// <summary>
        /// �w�肳�ꂽ�F��\������
        /// </summary>
        private void ShowColor(Color color)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            //�Ή�����X�v���C�g��T��
            foreach(LightColor lightColor in lightColors)
            {
                if(lightColor.color == color)
                {
                    //>>�Ή�����X�v���C�g

                    //�\������
                    spriteRenderer.sprite = lightColor.sprite;

                    return;
                }
            }
        }
    }
}
