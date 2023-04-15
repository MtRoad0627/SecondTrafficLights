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
        [Tooltip("�c�莞�Ԃ����ꖢ���ɂȂ�ƐԂ��Ȃ�")]
        [SerializeField] private float timeRed = 10f;

        void Update()
        {
            GetComponent<TextMeshProUGUI>().text = MakeText();

            //�c��10�b�����ɂȂ�ƐԂ�����
            if(GameManager.Instance.gameTimeLeft < timeRed)
            {
                GetComponent<TextMeshProUGUI>().color = Color.red;
            }
        }

        private string MakeText()
        {
            string output = "";

            int currentTime = (int)GameManager.Instance.gameTimeLeft + 1;

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
