using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    /// <summary>
    /// �Ԃ̃J���[�����O
    /// Car�N���X�ɖ������ē��삷��B
    /// �����Sprite�p
    /// </summary>
    public class CarColorSprite : CarColor
    {
        /// <summary>
        /// ���ۂɐF��ς���
        /// </summary>
        protected override void SetColor(Color color)
        {
            GetComponent<SpriteRenderer>().color = color;
        }
    }
}
