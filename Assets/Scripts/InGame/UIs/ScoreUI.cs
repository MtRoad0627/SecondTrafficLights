using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InGame.UI
{
    /// <summary>
    /// ���_��\������UI
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        private void Start()
        {
            //����������
            UpdateScore(GameManager.Instance.score);
        }

        /// <summary>
        /// �\�����X�V
        /// </summary>
        public void UpdateScore(int score)
        {
            this.GetComponent<TextMeshProUGUI>().text = MakeText(score);
        }

        /// <summary>
        /// �\�����镶��������
        /// </summary>
        private string MakeText(int score)
        {
            return score.ToString() + " pt";
        }
    }
}
