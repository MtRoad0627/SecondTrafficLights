using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace InGame.UI
{
    /// <summary>
    /// �c�莞�Ԃ�\��
    /// </summary>
    public class TimeDisplay : MonoBehaviour
    {
        void Update()
        {
            GetComponent<TextMeshProUGUI>().text = MakeText();
        }

        private string MakeText()
        {
            string output = "";

            int currentTime = (int)GameManager.Instance.gameTimeLeft;

            int minute = currentTime / 60;
            output += minute.ToString();

            output += " : ";

            int second = currentTime % 60;
            if(second < 10)
            {
                //�u08�v�̂悤�ɕ\��������
                output += "0";
            }
            output += second;

            return output;
        }
    }
}
