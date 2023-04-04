using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class Car : MonoBehaviour
    {
        [Tooltip("�X�s�[�h")]
        [SerializeField] private float speed = 5f;

        [Tooltip("RoadJoint������]���x")]
        [SerializeField] private float angularSpeed = 30f;

        [Tooltip("�Ԑ��ύX���̉�]���x")]
        [SerializeField] private float angularSpeedChangingLane = 10f;

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
        /// ���̔z��̏��ɉ����đ��s�BJoint�Ȃ���I�������_�ŏ���
        /// </summary>
        public Queue<Road> routes { get; private set; }

        ///<summary>
        ///���ݑ����Ă��铹�H
        ///</summary>
        public Road currentRoad { get; private set; }

        /// <summary>
        /// ���ݑ����Ă��铹�H�̎Ԑ��ԍ�
        /// </summary>
        public uint currentLane { get; private set; } = 0;

        /// <summary> 
        /// ���ݎg���Ă铹�����x�N�g��
        /// </summary>
        private Vector2 currentAlongRoad;

        /// <summary>
        /// ���ݑ����Ă铹�ő��s��������
        /// </summary>
        private float currentDistanceInRoad = 0f;

        /// <summary>
        /// ���̋����܂œ��B�����runningRoad�I��
        /// </summary>
        private float targetDistanceInRoad = 0f;

        /// <summary>
        /// ���݂�Joint�ړ��O���BJoint���ړ����I�������_�ōX�V���A����Joint�̋O������
        /// </summary>
        private CurveRoute currentCurveRoute;

        /// <summary>
        /// RunningJoint���̉�]�p
        /// </summary>
        private float currentAngle;

        /// <summary>
        /// ����RunningRoad��laneID
        /// </summary>
        private uint nextLaneID;

        /// <summary>
        /// ����RoadJoint�BrunningRoad�J�n���ɍX�V
        /// </summary>
        private RoadJoint nextRoadJoint;

        /// <summary>
        /// ���֌��������H�����s�ȂƂ��Btrue�Ȃ�runningJoint�ł͂Ȃ�changingLane�Ɉڂ�B
        /// </summary>
        private bool nextIsParallel;

        /// <summary>
        /// �ԗ��̐��ʃx�N�g��
        /// </summary>
        public Vector2 front
        {
            get
            {
                return transform.right;
            }
        }

        [Tooltip("���H�̊p�x�i�x�j������ȉ��Ȃ畽�s�Ƃ݂Ȃ�")]
        [SerializeField] private float roadsParallelThreshold = 10f;

        [Tooltip("�Ԑ��ύX���A�ړI�̎Ԑ��܂ł̉�]���a������ȉ��ɂȂ������]�ړ��ɂ�钲�����J�n����B")]
        [SerializeField] private float thresholdRadiusChangingLane = 5f;

        [Tooltip("�Ԑ��ύX���A�ړI�̎Ԑ��Ƃ̍ő�p�x")]
        [SerializeField] private float angleMaxChangingLane = 10f;

        [Tooltip("�Ԑ��ύX���A���H�Ƃ̊p�x������ȉ��ɂȂ����瓹�H�ƕ��s�Ƃ݂Ȃ�")]
        [SerializeField] private float parallelThresholdChangingLane = 3f;

        [Tooltip("���꒼����Ɣ��f����O�ς�臒l")]
        [SerializeField] private float onSameLineThreshold = 0.05f;

        /// <summary>
        /// �Ԑ��ύX�ŃJ�[�u���[�h�ɓ�����
        /// </summary>
        private bool changingLaneRotating = false;

        /// <summary>
        /// �Ԑ��ύX�I���t���O
        /// </summary>
        private bool changingLaneFinished = false;

        /// <summary>
        /// �Ԑ��ύX���̉~�ʋO��
        /// </summary>
        private CurveRoute curveChangingLane;

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

                case State.runningJoint:
                    RunJoint();
                    break;

                case State.changingLane:
                    ChangeLane();
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
            this.routes = GetRoute(spawnRoad.GetDiffrentEdge(spawnPoint), this.destination);

            //����n�߂�
            StartRunningRoad(spawnRoad, spawnLane, spawnPoint, true);
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
            //Navigator��胋�[�g���擾
            Road[] routeArray = Navigator.Instance.GetRoute(startingPoint, target);

            //�L���[�ɒ���
            Queue<Road> routeQueue = new Queue<Road>();
            foreach(Road road in routeArray)
            {
                routeQueue.Enqueue(road);
            }

            return routeQueue;
        }

        /// <summary>
        /// ���H�𑖂�n�߂�
        /// </summary>
        private void StartRunningRoad(Road road, uint laneID, RoadJoint startingJoint, bool first = false)
        {
            uint edgeID = road.GetEdgeID(startingJoint);

            //�L��
            currentRoad = road;
            currentLane = laneID;
            currentDistanceInRoad = 0f;
            currentAlongRoad = road.alongVectors[edgeID];
            nextRoadJoint = road.GetDiffrentEdge(startingJoint);

            //���݈ʒu�𓹘H�ɊJ�n�ʒu�ɒ���
            AdjustStartingPositionInRoad(road, laneID, edgeID, first);

            //���݂̍s����̍��W
            Vector2 destinationPoint;

            if(routes.Count > 0)
            {
                //>>����Joint������
                //����Joint��]���v�Z
                if (TryGetNextCurveRoute(road.GetDiffrentEdge(startingJoint)))
                {
                    nextIsParallel = false;

                    //����Joint�ړ�������ꍇ�A��]�J�n�ʒu�܂�runningRoad
                    destinationPoint = currentCurveRoute.startingPoint;
                }
                else
                {
                    //>>���s
                    nextIsParallel = true;

                    //Joint�܂ő���
                    destinationPoint = road.GetDiffrentEdge(startingJoint).transform.position;
                }
            }
            else
            {
                //>>�����I�_
                //����Joint�ړ����Ȃ��ꍇ�i�I�_�̏ꍇ�j�AJoint�܂ő���
                destinationPoint = road.GetDiffrentEdge(startingJoint).transform.position;
            }

            //�ڕW���s����
            targetDistanceInRoad = Vector2.Distance(road.GetStartingPoint(edgeID, laneID), destinationPoint);

            //�J�n�ʒu���_�ł̑��s����
            currentDistanceInRoad = targetDistanceInRoad - Vector2.Distance(transform.position, destinationPoint);

            //�X�e�[�g��ύX
            state = State.runningRoad;
        }

        /// <summary>
        /// RunningRoad�J�n���̈ʒu����
        /// </summary>
        private void AdjustStartingPositionInRoad(Road road, uint laneID, uint edgeID, bool first)
        {
            //���W
            if (!CheckInLine(transform.position, road.GetStartingPoint(edgeID, laneID), road.alongVectors[edgeID]))
            {
                //�����󖳂��ꍇ�͐����̑��֒���
                transform.position = GetFootOfPerpendicular(transform.position, road.GetStartingPoint(edgeID, laneID), road.alongVectors[edgeID]);
            }

            //����̏ꍇ�A���W�����킹��
            if (first)
            {
                transform.position = road.GetStartingPoint(edgeID, laneID);
            }

            //��]
            transform.rotation = GetRotatoinInRoad(road.alongVectors[edgeID]);
        }

        /// <summary>
        /// ����curveRoute���擾
        /// </summary>
        /// <returns>���s�̏ꍇ��false</returns>
        private bool TryGetNextCurveRoute(RoadJoint curvingJoint)
        {
            //����Road
            Road nextRoad = routes.Peek();

            //���ƕ��s
            if (IsParallel(currentRoad.alongVectors[0], nextRoad.alongVectors[0], roadsParallelThreshold))
            {
                return false;
            }

            //�擾
            currentCurveRoute = GetCurveRoute(
                curvingJoint,
                currentRoad,
                currentLane,
                nextRoad,
                GetNextLane()
                );

            return true;
        }

        /// <summary>
        /// ����Road�ő���Ԑ���I��
        /// </summary>
        private uint GetNextLane()
        {
            //TODO
            nextLaneID = 0;
            return nextLaneID;
        }

        /// <summary>
        /// runningJoint�X�e�[�g�ɓ���
        /// </summary>
        private void StartRunningJoint()
        {
            //�ʒu�𒲐�
            AdjustStartingPositionInJoint();

            //�p�x���̏�����
            currentAngle = currentCurveRoute.startingAngle;

            //�X�e�[�g��ύX
            state = State.runningJoint;
        }

        /// <summary>
        /// RunningJoint�J�n���̈ʒu����
        /// </summary>
        private void AdjustStartingPositionInJoint()
        {
            //���W
            transform.position = currentCurveRoute.startingPoint;

            //��]
            transform.rotation = GetRotationInJoint(currentCurveRoute.startingAngle, currentCurveRoute.clockwise);
        }

        /// <summary>
        /// runningRoad���̑��s����
        /// </summary>
        private void RunRoad()
        {
            //�O�i
            AdvanceRoad();
            
            //�I�[��ʂ�߂������m�F
            if (currentDistanceInRoad >= targetDistanceInRoad)
            {
                if(routes.Count > 0)
                {
                    //>>�܂��I�_�܂ŗ��Ă��Ȃ�
                    if (nextIsParallel)
                    {
                        //�Ԑ��ύX���[�h
                        StartChangingLane();
                    }
                    else
                    {
                        //Joint��]���[�h�ɓ���
                        StartRunningJoint();
                    }
                }
                else
                {
                    //>>�I�_�܂ŗ���
                    //����������
                    OnArrivedDestination();
                }
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
        /// Joint�����
        /// </summary>
        private void RunJoint()
        {
            //�ړ�
            TurnJoint();

            //�I�[��ʂ�߂������m�F
            if (CheckPassedJoint())
            {
                //�ʂ�߂���
                StartRunningRoad(routes.Dequeue(), nextLaneID, currentCurveRoute.curvingJoint);
            }
        }

        /// <summary>
        /// RoadJoint�����
        /// </summary>
        private void TurnJoint()
        {
            //���v�E�����v���Ő������]����
            int coef;
            if (currentCurveRoute.clockwise)
            {
                coef = -1;
            }
            else
            {
                coef = 1;
            }

            //��]
            currentAngle += GetAngularSpeedInJoint() * coef * Time.deltaTime;

            //���W
            transform.position = GetPositionFromPolar(currentCurveRoute.center, currentCurveRoute.radius, currentAngle);

            //��]
            transform.rotation = GetRotationInJoint(currentAngle, currentCurveRoute.clockwise);
        }

        /// <summary>
        /// RunningJoint���ɏI�[��ʂ�߂������m�F
        /// </summary>
        private bool CheckPassedJoint()
        {
            return CheckCircularFinished(currentAngle, currentCurveRoute);
        }

        /// <summary>
        /// �Ԑ��ύX���J�n
        /// </summary>
        private void StartChangingLane()
        {
            //CheckChangingLaneNecessary()���O�ɌĂԕK�v������
            GetNextLane();

            //�Ԑ��ύX�̕K�v���Ȃ����m�F
            if (CheckChangingLaneNecessary())
            {
                StartRunningRoad(routes.Dequeue(), nextLaneID, nextRoadJoint);
                return;
            }

            changingLaneRotating = false;
            curveChangingLane = new CurveRoute();

            state = State.changingLane;
        }

        /// <summary>
        /// �Ԑ��ύX
        /// </summary>
        private void ChangeLane()
        {
            Road nextRoad = routes.Peek();
            Vector2 nextVector = nextRoad.alongVectors[nextRoad.GetEdgeID(nextRoadJoint)];
            uint nextEdgeID = nextRoad.GetEdgeID(nextRoadJoint);
            Vector2 nextLaneStartingPoint = nextRoad.GetStartingPoint(nextEdgeID, nextLaneID);

            if(!changingLaneRotating)
            {
                TryMakeCurveChangingLane(nextLaneStartingPoint, nextVector);
            }

            if (changingLaneRotating)
            {
                //��]�ړ�
                ChangeLaneRotation(nextLaneStartingPoint, nextVector, curveChangingLane);
            }
            else
            {
                //�O�i���Ȃ���Ȃ���
                ChangeLaneForward(nextLaneStartingPoint, nextVector);
            }
        }

        /// <summary>
        /// �Ԑ��ύX���K�v���m�F����
        /// </summary>
        private bool CheckChangingLaneNecessary()
        {
            Road nextRoad = routes.Peek();
            uint edgeID = nextRoad.GetEdgeID(nextRoadJoint);

            return CheckInLine((Vector2)transform.position,
                (Vector2)nextRoad.GetStartingPoint(edgeID, nextLaneID),
                (Vector2)nextRoad.alongVectors[edgeID]);
        }

        /// <summary>
        /// �Ԑ��ύX���A�~�ʈړ������肵�āA��]�ړ����[�h�ɓ��邩���f
        /// </summary>
        private bool TryMakeCurveChangingLane(Vector2 nextLaneStartingPoint, Vector2 nextVector)
        {
            //���s�Ȃ�false
            if(IsParallel(nextVector, front, parallelThresholdChangingLane))
            {
                return false;
            }

            //>>��]�ړ�����Ɖ��肵���Ƃ��̔��a�E���S�����߂�
            //���݂̐i�s�����̖@���x�N�g��
            Vector2 perpendicularFromAhead = GetPerpendicular(front);

            //�i�s�����ƎԐ������̊p�̓񓙕����x�N�g��
            Vector2 bisector = GetBisector(-front, nextVector);

            //��]���S�̍��W
            Vector2 rotationCenter = GetIntersection(transform.position, perpendicularFromAhead, nextLaneStartingPoint, bisector);

            //��]���a
            float radius = Vector2.Distance(rotationCenter, transform.position);

            if (radius <= thresholdRadiusChangingLane)
            {
                //��]�ړ����J�n
                changingLaneRotating = true;

                //>>��]�O���̋�̉�
                //��]�������Z�o
                float angularDiference = Quaternion.FromToRotation(front, nextVector).eulerAngles.z;
                bool clockwise;
                if (angularDiference < 180f)
                {
                    //�����v���
                    clockwise = true;
                }
                else
                {
                    //���v���
                    clockwise = false;
                }

                //�J�[�u�̎n�_�E�I�_�����߂�
                Vector2 startingPoint = transform.position;
                Vector2 endingPoint = GetFootOfPerpendicular(rotationCenter, nextLaneStartingPoint, nextVector);

                //�p�x�����߂�
                float startingAngle = GetAngular(startingPoint - rotationCenter);
                float endingAngle = GetAngular(endingPoint - rotationCenter);

                //�O���̕ۑ�
                curveChangingLane.center = rotationCenter;
                curveChangingLane.radius = radius;
                curveChangingLane.clockwise = clockwise;
                curveChangingLane.startingAngle = startingAngle;
                curveChangingLane.endingAngle = endingAngle;

                //���݂̊p�x
                currentAngle = startingAngle;

                return true;
            }

            return false;
        }

        /// <summary>
        /// �Ԑ��ύX���́A�Ō�̉�]�ړ�
        /// </summary>
        private void ChangeLaneRotation(Vector2 targetLanePoint, Vector2 targetLaneVector, CurveRoute curve)
        {
            //�p�x���ړ�������
            if (curveChangingLane.clockwise)
            {
                currentAngle -= GetAngularSpeedInChangingLane() * Time.deltaTime;
            }
            else
            {
                currentAngle += GetAngularSpeedInChangingLane() * Time.deltaTime;
            }

            if(CheckCircularFinished(currentAngle, curve)){
                //>>�s���߂����̂ŎԐ������֖߂�

                //���W
                transform.position = GetPositionFromPolar(curve.center, curve.radius, curve.endingAngle);

                //��]
                transform.rotation = GetRotationInJoint(curve.endingAngle, curve.clockwise);

                //�Ԑ��ύX���I��
                StartRunningRoad(routes.Dequeue(), nextLaneID, nextRoadJoint);
            }
            else
            {
                //���W
                transform.position = GetPositionFromPolar(curve.center, curve.radius, currentAngle);

                //��]
                transform.rotation = GetRotationInJoint(currentAngle, curve.clockwise);
            }
        }

        /// <summary>
        /// �Ԑ��ύX���A�O�i���Ȃ���Ȃ���
        /// </summary>
        private void ChangeLaneForward(Vector2 targetLanePoint, Vector2 targetLaneVector)
        {
            //�Ȃ���������Z�o
            bool shouldTurnRight = !IsRightFromVector(transform.position, targetLanePoint, targetLaneVector);

            //�i�s�����ɑ΂���Ԑ��̊p�x
            float angularDifference = Vector2.Angle(front, targetLaneVector);

            //��]�p���Z�o
            float angularMovement = Mathf.Min(angleMaxChangingLane -angularDifference, GetAngularSpeedInChangingLane() * Time.deltaTime);

            if (shouldTurnRight)
            {
                //�E�ɋȂ���ꍇ�A�������]
                angularMovement = -angularMovement;
            }

            //��]
            transform.rotation = transform.rotation * Quaternion.Euler(0, 0, angularMovement);

            //��]��ɑO�i
            transform.position += (Vector3)(front.normalized * GetSpeedInRoad() * Time.deltaTime);
        }

        private float GetAngularSpeedInChangingLane()
        {
            return angularSpeedChangingLane;
        }

        /// <summary>
        /// �ړI�n�ɓ������āA������GameManager�ɕ�
        /// </summary>
        private void OnArrivedDestination()
        {
            //������
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Joint��]���̊p���x
        /// </summary>
        /// <returns></returns>
        private float GetAngularSpeedInJoint()
        {
            return angularSpeed;
        }

        /// <summary>
        /// RunRoad���̉�]��Ԃ�
        /// </summary>
        private static Quaternion GetRotatoinInRoad(Vector2 alongVector)
        {
            return Quaternion.FromToRotation(Vector3.right, alongVector);
        }

        /// <summary>
        /// ��̓�����J�[�u�O�����擾
        /// </summary>
        /// <param name="startingEdgeID">�����_����edgeID</param>
        /// <param name="endingRoad">�����_����edgeID</param>
        private CurveRoute GetCurveRoute(
            RoadJoint curvingJoint,
            Road startingRoad, 
            uint startingLaneID,
            Road endingRoad, 
            uint endingLaneID
            )
        {
            CurveRoute output = new CurveRoute();

            output.curvingJoint = curvingJoint;

            //EdgeID���擾
            uint startingEdgeID = startingRoad.GetEdgeID(curvingJoint);
            uint endingEdgeID = endingRoad.GetEdgeID(curvingJoint);

            //>>���v��肩���擾
            Vector2 startingAlongVector = startingRoad.alongVectors[Road.GetDifferentEdgeID(startingEdgeID)];
            Vector2 endingAlongVector = endingRoad.alongVectors[endingEdgeID];
            float angularDiference = Quaternion.FromToRotation(startingAlongVector, endingAlongVector).eulerAngles.z;
            if (angularDiference < 180f)
            {
                //�����v���
                output.clockwise = false;
            }
            else
            {
                //���v���
                output.clockwise = true;
            }

            //>>���S���擾
            //�Ԑ��̑��ʂ̌�_�����S�ɂȂ�

            //�l���������
            Vector2 startingRoadSideLinePoint;
            Vector2 startingRoadSideLineVector;
            Vector2 endingRoadSideLinePoint;
            Vector2 endingRoadSideLineVector;

            if (output.clockwise)
            {
                //>>���v���

                //starting: �Ԑ��̉E��
                startingRoadSideLinePoint = startingRoad.GetRightPoint(Road.GetDifferentEdgeID(startingEdgeID), startingLaneID);
                startingRoadSideLineVector = startingRoad.alongVectors[Road.GetDifferentEdgeID(startingEdgeID)];

                //ending: �Ԑ��̉E��
                endingRoadSideLinePoint = endingRoad.GetRightPoint(endingEdgeID, endingLaneID);
                endingRoadSideLineVector = endingRoad.alongVectors[endingEdgeID];
            }
            else
            {
                //>>�����v���
                
                //starting: �Ԑ��̍���
                startingRoadSideLinePoint = startingRoad.GetLeftPoint(Road.GetDifferentEdgeID(startingEdgeID), startingLaneID);
                startingRoadSideLineVector = startingRoad.alongVectors[Road.GetDifferentEdgeID(startingEdgeID)];

                //ending: �Ԑ��̍���
                endingRoadSideLinePoint = endingRoad.GetLeftPoint(endingEdgeID, endingLaneID);
                endingRoadSideLineVector = endingRoad.alongVectors[endingEdgeID];
            }

            //��_�����߂�
            output.center = GetIntersection(startingRoadSideLinePoint, startingRoadSideLineVector, endingRoadSideLinePoint, endingRoadSideLineVector);

            //�J�[�u�̎n�_�E�I�_�����߂�
            Vector2 startingPoint = GetFootOfPerpendicular(output.center, startingRoad.GetStartingPoint(Road.GetDifferentEdgeID(startingEdgeID), startingLaneID), startingRoad.alongVectors[Road.GetDifferentEdgeID(startingEdgeID)]);
            Vector2 endingPoint = GetFootOfPerpendicular(output.center, endingRoad.GetStartingPoint(endingEdgeID, endingLaneID), endingRoad.alongVectors[endingEdgeID]);

            //���a�����߂�
            output.radius = Vector2.Distance(startingPoint, output.center);

            //�p�x�����߂�
            output.startingAngle = GetAngular(startingPoint - output.center);
            output.endingAngle = GetAngular(endingPoint - output.center);

            return output;
        }

        /// <summary>
        /// ��̐����̌�_�����߂�
        /// </summary>
        private static Vector2 GetIntersection(Vector2 line0Point, Vector2 line0Vector, Vector2 line1Point, Vector2 line1Vector)
        {
            // �O�ς����߂�
            float cross = line0Vector.x * line1Vector.y - line0Vector.y * line1Vector.x;

            // ���������s�ł���ꍇ
            if (Mathf.Approximately(cross, 0f))
            {
                Debug.LogError("���s");
                return Vector2.zero;
            }

            // ��_�����߂�
            float t = ((line1Point.x - line0Point.x) * line1Vector.y - (line1Point.y - line0Point.y) * line1Vector.x) / cross;
            Vector2 intersectionPoint = line0Point + line0Vector * t;

            return intersectionPoint;
        }

        /// <summary>
        /// RunningJoint���̉�]���擾
        /// </summary>
        private Quaternion GetRotationInJoint(float angleInCurve, bool clockwise)
        {
            float addition;

            //90�x�����������邱�ƂŐi�s����������
            if (clockwise)
            {
                addition = -90f;
            }
            else
            {
                addition = 90f;
            }

            return Quaternion.Euler(0f, 0f, angleInCurve + addition);
        }

        /// <summary>
        /// �����̑������߂�
        /// </summary>
        private static Vector2 GetFootOfPerpendicular(Vector2 point, Vector2 linePoint, Vector2 lineVector)
        {
            Vector2 v = point - linePoint;
            float t = Vector2.Dot(v, lineVector) / lineVector.sqrMagnitude;
            Vector2 foot = linePoint + lineVector * t;

            return foot;
        }

        /// <summary>
        /// �_��������ɂ��邩���肷��
        /// </summary>
        private bool CheckInLine(Vector3 point, Vector3 linePoint, Vector3 lineVector)
        {
            Vector3 difference = point - linePoint;

            //�O�ς̑傫�������߂�
            float outer = Vector3.Cross(lineVector, difference).magnitude;

            //0�Ȃ璼����
            return IsSame(outer, 0f, onSameLineThreshold);
        }

        /// <summary>
        /// �ɍ��W���畽�ʍ��W�ɕϊ�
        /// </summary>
        private static Vector2 GetPositionFromPolar(Vector2 pole, float radius, float angular)
        {
            return pole + (Vector2)(Quaternion.Euler(0f, 0f, angular) * Vector2.right) * radius;
        }

        /// <summary>
        /// ��̃x�N�g�������s���i臒l�ȉ��j���Ԃ�
        /// </summary>
        private bool IsParallel(Vector2 vec0, Vector2 vec1, float threshold)
        {
            
            float angle = Vector2.Angle(vec0, vec1);

            if((angle <= threshold)
                ||(Mathf.Abs(angle -180f) <= threshold)
                ||(Mathf.Abs(angle -360f) <= threshold)){
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// �덷�������ē���l��Ԃ�
        /// </summary>
        private bool IsSame(float v0, float v1, float threshold)
        {
            return (Mathf.Abs(v0 - v1) <= threshold);
        }

        /// <summary>
        /// �_�ƒ����̋��������߂�
        /// </summary>
        private static float GetDistance(Vector2 point, Vector2 linePoint, Vector2 lineVector)
        {
            //point�̑��΍��W
            Vector2 pointToLineStart = point - linePoint;

            //point����̐��ˉe�_
            float dotProduct = Vector2.Dot(pointToLineStart, lineVector);
            Vector2 projection = linePoint + lineVector * dotProduct;

            //���΍��W�Ɛ��ˉe�_�̋��������߂�Ηǂ�
            return Vector2.Distance(point, projection);
        }

        /// <summary>
        /// �@���x�N�g�������߂�
        /// </summary>
        private static Vector2 GetPerpendicular(Vector2 vec)
        {
            return new Vector2(vec.y, -vec.x);
        }

        private static Vector2 GetBisector(Vector2 vec0, Vector2 vec1)
        {
            Vector2 u0 = vec0.normalized;
            Vector2 u1 = vec1.normalized;

            return (u0 + u1).normalized;
        }

        /// <summary>
        /// �^����ꂽ�x�N�g���ɑ΂��A�^����ꂽ�_���E�ɂ��邩��Ԃ�
        /// </summary>
        private static bool IsRightFromVector(Vector2 point, Vector2 linePoint, Vector2 lineVector)
        {
            float angularDiference = Quaternion.FromToRotation(lineVector, point-linePoint).eulerAngles.z;

            if (angularDiference < 180f)
            {
                //���ɂ���
                return false;
            }
            else
            {
                //�E�ɂ���
                return true;
            }
        }

        /// <summary>
        /// ��]�^�����I���������m�F
        /// </summary>
        private static bool CheckCircularFinished(float currentAngle, CurveRoute curveRoute)
        {
            if (curveRoute.clockwise)
            {
                //���v���
                if (curveRoute.startingAngle > curveRoute.endingAngle)
                {
                    //x�������o�߂��Ȃ�
                    return (currentAngle <= curveRoute.endingAngle);
                }
                else
                {
                    //x�������o��
                    return (currentAngle <= curveRoute.endingAngle - 360);
                }
            }
            else
            {
                //�����v���
                if (curveRoute.startingAngle < curveRoute.endingAngle)
                {
                    //x�������o�߂��Ȃ�
                    return (currentAngle >= curveRoute.endingAngle);
                }
                else
                {
                    //x�������o��
                    return (currentAngle >= curveRoute.endingAngle + 360);
                }
            }
        }

        /// <summary>
        /// �x�N�g���̂���������̔����v���̊p�x(0-360)���Z�o
        /// </summary>
        /// <returns></returns>
        private float GetAngular(Vector2 vector)
        {
            Quaternion q = Quaternion.FromToRotation(Vector2.right, vector);

            float z = q.eulerAngles.z;

            if (Mathf.Approximately(z, 0f))
            {
                //180�x��]�̏ꍇ�Ax, y��]�Ƃ݂Ȃ����z���O�ɂȂ��Ă���\��������
                if ((q.eulerAngles.x > 90f) || (q.eulerAngles.y > 90f))
                {
                    z = 180f;
                }
            }

            return z;
        }

        /// <summary>
        /// Joint���Ȃ���Ƃ��A�~�ʂ�`���B���̉~�ʂ̎n�_�A�I�_�A���S�B
        /// </summary>
        private struct CurveRoute
        {
            /// <summary>
            /// ����Ă���RoadJoint
            /// </summary>
            public RoadJoint curvingJoint;

            /// <summary>
            /// �~�ʂ̒��S
            /// </summary>
            public Vector2 center;

            /// <summary>
            /// �~�ʔ��a
            /// </summary>
            public float radius;

            /// <summary>
            /// �n�_�̊p�x�i�������甽���v���Ɂj
            /// </summary>
            public float startingAngle;

            /// <summary>
            /// �I�_�̊p�x�i�������甽���v���Ɂj
            /// </summary>
            public float endingAngle;

            /// <summary>
            /// ���v��肩
            /// </summary>
            public bool clockwise;

            public Vector2 startingPoint
            {
                get
                {
                    return GetPositionFromPolar(center, radius, startingAngle);
                }
            }

            public Vector2 endingPoint
            {
                get
                {
                    return GetPositionFromPolar(center, radius, endingAngle);
                }
            }
        }
    }
}