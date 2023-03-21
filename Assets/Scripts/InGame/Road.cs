using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame
{
    /// <summary>
    /// ���H�B
    /// �Ԃ����̏�𑖂�
    /// �����������Ƃ��āA�����I��RoadJoint�ɐڑ�����
    /// </summary>
    public class Road : MonoBehaviour
    {
        [Tooltip("���H�̗��[�̋�I�u�W�F�N�g")]
        [SerializeField] private GameObject[] edgeObjects;

        [Tooltip("���[�ɐݒu����Ă���M���@�BedgeObjects�Ɠ������Ԃœo�^���邱��")]
        [SerializeField] private TrafficLight[] trafficLights;

        [Tooltip("Edge0�̊J�n�ʒu�O���i�����j�Ԑ����炢��邱��")]
        [SerializeField] private Transform[] _startingPoint0;
        [Tooltip("Edge1�̊J�n�ʒu�O���i�����j�Ԑ����炢��邱��")]
        [SerializeField] private Transform[] _startingPoint1;

        /// <summary>
        /// Edge0�̊J�n�ʒu�B�i�s�����������珇��
        /// </summary>
        public Vector2[] startingPoint0
        {
            get
            {
                Vector2[] output = new Vector2[_startingPoint0.Length];
                for(int cnt = 0; cnt < _startingPoint0.Length; cnt++)
                {
                    output[cnt] = _startingPoint0[cnt].position;
                }

                return output;
            }
        }

        /// <summary>
        /// Edge1�̊J�n�ʒu�B�i�s�����������珇��
        /// </summary>
        public Vector2[] startingPoint1
        {
            get
            {
                Vector2[] output = new Vector2[_startingPoint1.Length];
                for (int cnt = 0; cnt < _startingPoint1.Length; cnt++)
                {
                    output[cnt] = _startingPoint1[cnt].position;
                }

                return output;
            }
        }

        /// <summary>
        /// �Б��Ԑ���
        /// </summary>
        public uint lanes
        {
            get
            {
                return (uint)startingPoint0.Count();
            }
        }

        /// <summary>
        /// �������x�N�g���B0�Ԗڂ�edge0��edge1�A1�Ԗڂ͔���
        /// </summary>
        public Vector2[] alongVectors { get; private set; } = new Vector2[2];

        /// <summary>
        /// ���[�ɐڑ����Ă���RoadJoint�Q�B���Ԃ�edge�ɑΉ��B0�Ԗڂ�edge0�n�_�B
        /// </summary>
        public RoadJoint[] connectedJoints { get; private set; } = new RoadJoint[2];

        /// <summary>
        /// ���H�ɐڑ��ς݂�
        /// </summary>
        public bool isInitialized { get; private set; } = false;

        private void Start()
        {
            //�z�u�œK��
            OptimizeArrangement();
            GetVectors();
        }

        /// <summary>
        /// ���[�ɍŋߖT��RoadJoint��T�����A�z�u���œK��
        /// </summary>
        private void OptimizeArrangement()
        {
            //�ڑ�RoadJoint���擾
            GetConnectedJoints();

            //�œK���O�̗��[�����ԃx�N�g��
            Vector3 originalPath = edgeObjects[0].transform.position - edgeObjects[1].transform.position;
            //�œK����
            Vector3 optimizedPath = connectedJoints[0].transform.position - connectedJoints[1].transform.position;

            //�ړ�
            transform.position = (connectedJoints[0].transform.position + connectedJoints[1].transform.position) / 2;

            //>>�g�傠�邢�͏k��
            //�{��
            float extensionCoef = optimizedPath.magnitude / originalPath.magnitude;
            //�������Ɋg��E�k��
            transform.localScale = new Vector3(extensionCoef * transform.localScale.x, transform.localScale.y, transform.localScale.z);

            //��]
            Quaternion rotation = Quaternion.FromToRotation(originalPath, optimizedPath);
            transform.rotation *= rotation;

            //�������ς݂�
            isInitialized = true;
        }

        /// <summary>
        /// �������x�N�g�����Ƃ�
        /// </summary>
        private void GetVectors()
        {
            alongVectors[0] = edgeObjects[1].transform.position - edgeObjects[0].transform.position;
            alongVectors[1] = -alongVectors[0];
        }

        /// <summary>
        /// ���[�ɍŋߖT��RoadJoint���擾
        /// </summary>
        private void GetConnectedJoints()
        {
            //���[�ɂ���
            for (int cnt = 0; cnt < 2; cnt++)
            {
                Vector3 edgePosition = edgeObjects[cnt].transform.position;

                //�ŋߖT��T���ēo�^
                connectedJoints[cnt] = GetNearestJoint(edgePosition);

                //RoadJoint���ɂ������o�^����
                connectedJoints[cnt].RegisterRoad(this, cnt);
            }
        }

        /// <summary>
        /// �ł��߂�RoadJoint��T��
        /// </summary>
        private RoadJoint GetNearestJoint(Vector3 searchingPoint)
        {
            //�SRoadJoint���擾
            RoadJoint[] allJoints = FindObjectsOfType<RoadJoint>();

            RoadJoint nearestJoint = null;

            //�ŋߖT����`�T��
            float minDistance = float.MaxValue;
            foreach (RoadJoint joint in allJoints)
            {
                Vector3 difference = searchingPoint - joint.transform.position;
                float distance = difference.magnitude;

                if (distance < minDistance)
                {
                    nearestJoint = joint;
                    minDistance = distance;
                }
            }

            //null���Ƃ��������iRoadJoint�����m����Ă��Ȃ��j
            Debug.Assert(nearestJoint != null);

            return nearestJoint;
        }

        /// <summary>
        /// �B���Ă���M���@���N��
        /// </summary>
        /// <param name="side">���̂ǂ���̒[�̐M���@��</param>
        /// <returns>�N�������M���@�̎Q��</returns>
        public TrafficLight ActivateTrafficLight(int side)
        {
            //�M���@���N��
            trafficLights[side].gameObject.SetActive(true);

            //�M���@�̎Q�Ƃ�Ԃ�
            return trafficLights[side];
        }

        /// <summary>
        /// �^����ꂽroad�ɂ���joint�ƈقȂ�[��Ԃ�
        /// </summary>
        public RoadJoint GetDiffrentEdge(RoadJoint joint)
        {
            //joint��road�̂ǂ��炩�̒[�̂͂�
            Debug.Assert(connectedJoints.Contains(joint));

            if (connectedJoints[0] == joint)
            {
                return connectedJoints[1];
            }
            else
            {
                return connectedJoints[0];
            }
        }

        /// <summary>
        /// �^����ꂽ�[��edge�ԍ���Ԃ�
        /// </summary>
        public uint GetEdgeNumber(RoadJoint edge)
        {
            if (connectedJoints[0] == edge)
            {
                return 0;
            }
            else if(connectedJoints[1] == edge)
            {
                return 1;
            }
            else
            {
                Debug.LogError("�[����Ȃ�RoadJoint���n���ꂽ");
                return 0;
            }
        }

        /// <summary>
        /// �J�n�ʒu��Ԃ�
        /// </summary>
        public Vector2 GetStartingPoint(uint edgeID, uint laneID)
        {
            Debug.Assert(edgeID < 2);
            Debug.Assert(laneID < lanes);

            if (edgeID == 0)
            {
                return startingPoint0[laneID];
            }
            else
            {
                return startingPoint1[laneID];
            }
        }
    }
}