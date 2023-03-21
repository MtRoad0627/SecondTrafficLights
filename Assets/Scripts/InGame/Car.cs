using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class Car : MonoBehaviour
    {
        [Tooltip("�X�s�[�h")]
        [SerializeField] private float speed = 5f;

        [SerializeField] private float zPos = -2f;

        private enum State
        {
            runningRoad,
            runningJoint,
            changingLane
        }

        private State state;

        /// <summary>
        /// �������ꂽRoadJoint
        /// </summary>
        public RoadJoint spawnPoint { get; private set; }

        /// <summary>
        /// �ړI�nRoadJoint
        /// </summary>
        public RoadJoint destination { get; private set; }

        /// <summary>
        /// ���̔z��̏��ɉ����đ��s
        /// </summary>
        public Queue<Road> routes { get; private set; }

        ///<summary>
        ///���ݑ����Ă��铹�H
        ///</summary>
        private Road currentRoad;

        /// <summary>
        /// ���ݑ����Ă��铹�H�̎Ԑ��ԍ�
        /// </summary>
        private uint currentLane = 0;

        /// <summary> 
        /// ���ݎg���Ă铹�����x�N�g��
        /// </summary>
        private Vector2 currentAlongRoad;

        /// <summary>
        /// ���ݑ����Ă铹�ő��s��������
        /// </summary>
        private float currentDistanceInRoad = 0f;

        private void Update()
        {
            Run();
        }

        /// <summary>
        /// ���s�B�O�i���Ȃ�������˂�
        /// </summary>
        private void Run()
        {
            switch (state)
            {
                case State.runningRoad:
                    RunRoad();
                    break;
            }
        }

        /// <summary>
        /// �������̏���������
        /// </summary>
        public void Initialize(RoadJoint spawnPoint, Road spawnRoad, uint spawnLane, RoadJoint destination = null)
        {
            //�����|�C���g
            this.spawnPoint = spawnPoint;

            //�ړI�n
            if (destination == null)
            {
                //������ŖړI�n��ݒ�
                this.destination = ChooseDestinationRadomly(this.spawnPoint);
            }
            else
            {
                //�ړI�n���w�肳��Ă���
                this.destination = destination;
            }

            //�������H�͊m�肵�Ă���̂ŁA���̎���joint����̃��[�g�𓾂�
            this.routes = GetRoute(spawnRoad.GetDiffrentEdge(spawnPoint), destination);

            //����n�߂�
            StartRunningRoad(spawnRoad, spawnLane, spawnPoint);
        }

        /// <summary>
        /// �����_���ɖړI�n�����߂�
        /// </summary>
        private RoadJoint ChooseDestinationRadomly(RoadJoint spawnPoint)
        {
            //spawnPoint�Ƃ̏d��������郋�[�v
            RoadJoint destination;
            OutsideConnection[] outsideConnectionsAll = FindObjectsOfType<OutsideConnection>();
            while (true)
            {
                //�����_���ɖړI�n�����߂�
                destination = outsideConnectionsAll[Random.Range(0, outsideConnectionsAll.Length - 1)];

                //�d���`�F�b�N
                if (destination != spawnPoint)
                {
                    //>>�����|�C���g�ƖړI�n���d�����Ă��Ȃ�
                    break;
                }
            }

            return destination;
        }

        /// <summary>
        /// ���[�g���擾
        /// </summary>
        /// <param name="startingPoint">�J�n�ʒu</param>
        /// <param name="target">�ړI�n</param>
        /// <returns></returns>
        private Queue<Road> GetRoute(RoadJoint startingPoint, RoadJoint target)
        {
            //TODO
            Queue<Road> output = new Queue<Road>();

            Road nextRoad = startingPoint.connectedRoads[Random.Range(0, startingPoint.connectedRoads.Count)];

            output.Enqueue(nextRoad);

            return output;
        }

        /// <summary>
        /// ���H�𑖂�n�߂�
        /// </summary>
        private void StartRunningRoad(Road road, uint laneID, RoadJoint startingJoint)
        {
            uint edgeID = road.GetEdgeNumber(startingJoint);

            //�L��
            currentRoad = road;
            currentLane = laneID;
            currentDistanceInRoad = 0f;
            currentAlongRoad = road.alongVectors[edgeID];

            //>>���݈ʒu�𓹘H�ɊJ�n�ʒu�ɒ���
            //�ʒu
            Vector2 startingPointPosition = road.GetStartingPoint(edgeID, laneID);
            transform.position = (Vector3)startingPointPosition + new Vector3(0, 0, zPos);
            //�p�x
            transform.rotation = GetRotatoinInRoad(currentAlongRoad);

            //�X�e�[�g��ύX
            state = State.runningRoad;
        }

        /// <summary>
        /// runningJoint�X�e�[�g�ɓ���
        /// </summary>
        private void StartRunningJoint()
        {
            //�X�e�[�g��ύX
            state = State.runningJoint;
        }

        /// <summary>
        /// runningRoad���̑��s����
        /// </summary>
        private void RunRoad()
        {
            //�O�i
            AdvanceRoad();
            
            //�I�[��ʂ�߂������m�F
            if (currentDistanceInRoad >= currentAlongRoad.magnitude)
            {
                //Joint��]���[�h�ɓ���
                StartRunningJoint();
            }
        }

        /// <summary>
        /// runningRoad���̑��s����
        /// </summary>
        private void AdvanceRoad()
        {
            float advancedDistance = GetSpeedInRoad() * Time.deltaTime;
            //�O���i���[���h�j�I
            transform.position += (Vector3)(currentAlongRoad.normalized * advancedDistance);
            //�����I
            currentDistanceInRoad += advancedDistance;
        }

        /// <summary>
        /// runningRoad���̏󋵂ɑΉ����鑬�x�����߂�
        /// </summary>
        private float GetSpeedInRoad()
        {
            return speed;
        }

        /// <summary>
        /// RunRoad���̉�]��Ԃ�
        /// </summary>
        private Quaternion GetRotatoinInRoad(Vector2 alongVector)
        {
            return Quaternion.FromToRotation(Vector3.right, alongVector);
        }
    }
}