using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InGame;

namespace Result
{
    /// <summary>
    /// �ŏI�I�ȓ��_��\��
    /// </summary>
    public class TotalPoint : MonoBehaviour
    {
        void Start()
        {
            GetComponent<TextMeshProUGUI>().text = (GameManager.score + GameManager.bonus).ToString();
        }
    }
}