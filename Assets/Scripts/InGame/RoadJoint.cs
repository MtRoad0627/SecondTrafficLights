using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    /// <summary>
    /// Road��Road���Ȃ�Joint
    /// �Ȃ���p�ɂ͂���RoadJoint
    /// �����_�A�O�E�ւ̐ڑ����͎q�N���X���g�p
    /// </summary>
    public class RoadJoint : MonoBehaviour
    {
        /// <summary>
        /// ����Joint�ɐڑ����Ă���Road
        /// </summary>
        public List<Road> connectedRoads { get; private set; } = new List<Road>();

        /// <summary>
        /// �econnectedRoad�̂ǂ���̒[���q�����Ă��邩
        /// </summary>
        public Dictionary<Road, int> edges { get; private set; } = new Dictionary<Road, int>();

        /// <summary>
        /// �q����������o�^
        /// </summary>
        /// <param name="edge">���H�̂ǂ���̒[���iRoad.Edge�̔ԍ��ɑΉ��j</param>
        public void RegisterRoad(Road road, int edge)
        {
            //����o�^
            connectedRoads.Add(road);

            //�[��o�^
            edges[road] = edge;
        }
    }
}