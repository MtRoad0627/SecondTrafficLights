using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InGame.UI
{
    /// <summary>
    /// ���_���ɁA�t�F�[�h�A�E�g���Ă���UI�B
    /// Animation�x�[�X�œ���
    /// </summary>
    public class ScoreAdditionUI : MonoBehaviour
    {
        /// <summary>
        /// ������
        /// </summary>
        /// <param name="scoreAdditional">���_���ꂽ��</param>
        public void Initialize(int scoreAdditional)
        {
            GetComponent<TextMeshProUGUI>().text = MakeText(scoreAdditional);
        }

        /// <summary>
        /// �A�j���[�V�������I�������A������
        /// </summary>
        public void OnAnimationFinished()
        {
            Destroy(this.gameObject);
        }

        private string MakeText(int score)
        {
            return "+" + score.ToString();
        }
    }
}