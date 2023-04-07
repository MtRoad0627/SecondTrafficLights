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
        /// connectedRoads�������v���ɕ��בւ��ς݂�
        /// </summary>
        public bool sortedAnticlockwise { get; private set; } = false;

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
        /// Intersection���猩��road�𔽎��v���ɕ��ׂ�
        /// </summary>
        public virtual void ArrangeRoadsAnticlockwise()
        {
            //�eRoad�Ƃ�x������������̎��v���̊p�x�����߂�
            Dictionary<Road, float> angles = new Dictionary<Road, float>();
            foreach (Road road in connectedRoads)
            {
                //Road��Intersection�̍��x�N�g��
                Vector3 dif = road.transform.position - transform.position;

                angles[road] = MyMath.GetAngular(dif);
            }

            //�p�x�����������ɕ��ёւ�
            List<Road> orderedRoad = new List<Road>();
            foreach (KeyValuePair<Road, float> roadAngle in angles.OrderBy(c => c.Value))
            {
                orderedRoad.Add(roadAngle.Key);
            }

            //�o�^���Ȃ���
            connectedRoads = orderedRoad;

            //���בւ��ς݂�
            sortedAnticlockwise = true;
        }

        /// <summary>
        /// ��̓��H�ɋ��ʂ���RoadJoint
        /// </summary>
        public static RoadJoint FindCommonJoint(Road road0, Road road1)
        {
            RoadJoint[] joints0 = road0.connectedJoints;

            RoadJoint commonJoint = null;

            foreach(RoadJoint joint in joints0)
            {
                if (joint.connectedRoads.Contains(road1))
                {
                    commonJoint = joint;
                    break;
                }
            }

            return commonJoint;
        }
    }
}