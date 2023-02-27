using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// ���[�ɐڑ����Ă���RoadJoint�Q�B���Ԃ�edge�ɑΉ�
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
    }
}