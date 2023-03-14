using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        /// connectedRoads�����v���ɕ��בւ��ς݂�
        /// </summary>
        public bool sortedClockwise { get; private set; } = false;

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

        /// <summary>
        /// Intersection���猩��road�����v���ɕ��ׂ�
        /// </summary>
        public virtual void ArrangeRoadsClockwise()
        {
            //�eRoad�Ƃ�x������������̎��v���̊p�x�����߂�
            Dictionary<Road, float> angles = new Dictionary<Road, float>();
            foreach (Road road in connectedRoads)
            {
                //Road��Intersection�̍��x�N�g��
                Vector3 dif = road.transform.position - transform.position;

                //x���������Ƃ̕��������Ă��Ȃ��p�x�i�x�N�g���̂Ȃ��p�j
                float unsigned = Vector2.Angle(new Vector2(1, 0), dif);

                if (dif.y >= 0)
                {
                    //Road��Intersection���㑤�ɂ���Ƃ��A�O�`�P�W�O�x�ɂȂ��Ă���Ƃ����180�x����
                    angles[road] = 360f - unsigned;
                }
                else
                {
                    //���ɂ���ꍇ�͂��̂܂�
                    angles[road] = unsigned;
                }
            }

            //�p�x�����������ɕ��ёւ�
            List<Road> orderedRoad = new List<Road>();
            foreach (KeyValuePair<Road, float> roadAngle in angles.OrderBy(c => c.Value))
            {
                orderedRoad.Add(roadAngle.Key);
            }

            //���v���ɓo�^���Ȃ���
            connectedRoads = orderedRoad;

            //���v�����בւ��ς݂�
            sortedClockwise = true;
        }
    }
}