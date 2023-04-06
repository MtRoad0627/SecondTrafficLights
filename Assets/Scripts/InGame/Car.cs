using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class Car : MonoBehaviour
    {
        [Header("���x�֌W")]

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
        /// ���ݍl����ׂ��Ώۂ̐M���@
        /// </summary>
        private TrafficLight currentTrafficLight;

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

        [Header("�Ԑ��ύX")]

        [Tooltip("���H�̊p�x�i�x�j������ȉ��Ȃ畽�s�Ƃ݂Ȃ�")]
        [SerializeField] private float roadsParallelThreshold = 10f;

        [Tooltip("�Ԑ��ύX���A�ړI�̎Ԑ��܂ł̉�]���a������ȉ��ɂȂ������]�ړ��ɂ�钲�����J�n����B")]
        [SerializeField] private float thresholdRadiusChangingLane = 5f;

        [Tooltip("�Ԑ��ύX���A�ړI�̎Ԑ��Ƃ̍ő�p�x")]
        [SerializeField] private float angleMaxChangingLane = 10f;

        [Tooltip("�Ԑ��ύX���A���H�Ƃ̊p�x������ȉ��ɂȂ����瓹�H�ƕ��s�Ƃ݂Ȃ�")]
        [SerializeField] private float parallelThresholdChangingLane = 3f;

        [Header("�Z���V���O")]
        [Tooltip("���o�r�[���n�_")]
        [SerializeField] private Transform detectionRayStart;

        [Tooltip("���o�r�[���I�_�E�O")]
        [SerializeField] private Transform[] detectionRayDestinationsFront;

        [Tooltip("���o�r�[���I�_�E���O")]
        [SerializeField] private Transform[] detectionRayDestinationsFrontLeft;

        [Tooltip("���o�r�[���I�_�E�E�O")]
        [SerializeField] private Transform[] detectionRayDestinationsFrontRight;

        [Tooltip("���o�r�[���I�_�E��")]
        [SerializeField] private Transform[] detectionRayDestinationsLeft;

        [Tooltip("���o�r�[���I�_�E�E")]
        [SerializeField] private Transform[] detectionRayDestinationsRight;

        [Tooltip("���[�g������E�n�_")]
        [SerializeField] private Transform meterPrototypeStart;

        [Tooltip("���[�g������E�I�_")]
        [SerializeField] private Transform meterPrototypeEnd;

        [Header("���̑�")]
        [Tooltip("���꒼����Ɣ��f����O�ς�臒l")]
        [SerializeField] private float onSameLineThreshold = 0.05f;

        //���o���ꂽ��
        private List<Car> carsDetectedFront = new List<Car>();
        private List<Car> carsDetectedFrontLeft = new List<Car>();
        private List<Car> carsDetectedFrontRight = new List<Car>();
        private List<Car> carsDetectedLeft = new List<Car>();
        private List<Car> carsDetectedRight = new List<Car>();

        /// <summary>
        /// �Ԑ��ύX�ŃJ�[�u���[�h�ɓ�����
        /// </summary>
        private bool changingLaneRotating = false;

        /// <summary>
        /// �Ԑ��ύX���̉~�ʋO��
        /// </summary>
        private CurveRoute curveChangingLane;

        /// <summary>
        /// �O���[�o�����W�������烁�[�g���ɒ����W��
        /// </summary>
        private float globalToMeterCoef
        {
            get
            {
                return 1f / (meterPrototypeStart.position - meterPrototypeEnd.position).magnitude;
            }
        }

        private void Update()
        {
            Run();
            Detect();
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
        /// ���o�r�[���𔭎˂��āA���͂̕��̂����o����B
        /// </summary>
        private void Detect()
        {
            DetectCars();
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

            //�M���@�����m
            currentTrafficLight = DetectTrafficLight(currentRoad, edgeID);

            //�X�e�[�g��ύX
            state = State.runningRoad;
        }

        /// <summary>
        /// RunningRoad�J�n���̈ʒu����
        /// </summary>
        private void AdjustStartingPositionInRoad(Road road, uint laneID, uint edgeID, bool first)
        {
            //���W
            if (!MyMath.CheckOnLine(transform.position, road.GetStartingPoint(edgeID, laneID), road.alongVectors[edgeID], onSameLineThreshold))
            {
                //�����󖳂��ꍇ�͐����̑��֒���
                transform.position = MyMath.GetFootOfPerpendicular(transform.position, road.GetStartingPoint(edgeID, laneID), road.alongVectors[edgeID]);
            }

            //����̏ꍇ�A���W�����킹��
            if (first)
            {
                transform.position = road.GetStartingPoint(edgeID, laneID);
            }

            //��]
            transform.rotation = Quaternion.Euler(0,0,GetRotatoinInRoad(road.alongVectors[edgeID]));
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
            if (MyMath.IsParallel(currentRoad.alongVectors[0], nextRoad.alongVectors[0], roadsParallelThreshold))
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
            transform.position = MyMath.GetPositionFromPolar(currentCurveRoute.center, currentCurveRoute.radius, currentAngle);

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

            return MyMath.CheckOnLine((Vector2)transform.position,
                (Vector2)nextRoad.GetStartingPoint(edgeID, nextLaneID),
                (Vector2)nextRoad.alongVectors[edgeID],
                onSameLineThreshold);
        }

        /// <summary>
        /// �Ԑ��ύX���A�~�ʈړ������肵�āA��]�ړ����[�h�ɓ��邩���f
        /// </summary>
        private bool TryMakeCurveChangingLane(Vector2 nextLaneStartingPoint, Vector2 nextVector)
        {
            //���s�Ȃ�false
            if(MyMath.IsParallel(nextVector, front, parallelThresholdChangingLane))
            {
                return false;
            }

            //>>��]�ړ�����Ɖ��肵���Ƃ��̔��a�E���S�����߂�
            //���݂̐i�s�����̖@���x�N�g��
            Vector2 perpendicularFromAhead = MyMath.GetPerpendicular(front);

            //�i�s�����ƎԐ������̊p�̓񓙕����x�N�g��
            Vector2 bisector = MyMath.GetBisector(-front, nextVector);

            //�i�s�����ƎԐ��̌�_
            Vector2 intersection = MyMath.GetIntersection(transform.position, front, nextLaneStartingPoint, nextVector);

            //��]���S�̍��W
            Vector2 rotationCenter = MyMath.GetIntersection(transform.position, perpendicularFromAhead, intersection, bisector);

            //��]���a
            float radius = Vector2.Distance(rotationCenter, transform.position);

            if (radius <= thresholdRadiusChangingLane)
            {
                //��]�ړ����J�n
                changingLaneRotating = true;

                //>>��]�O���̋�̉�
                //��]�������Z�o
                float angularDiference = MyMath.GetAngularDifference(front, nextVector);
                bool clockwise;
                if (angularDiference < 180f)
                {
                    //�����v���
                    clockwise = false;
                }
                else
                {
                    //���v���
                    clockwise = true;
                }

                //�J�[�u�̎n�_�E�I�_�����߂�
                Vector2 startingPoint = transform.position;
                Vector2 endingPoint = MyMath.GetFootOfPerpendicular(rotationCenter, nextLaneStartingPoint, nextVector);

                //�p�x�����߂�
                float startingAngle = MyMath.GetAngular(startingPoint - rotationCenter);
                float endingAngle = MyMath.GetAngular(endingPoint - rotationCenter);

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
                transform.position = MyMath.GetPositionFromPolar(curve.center, curve.radius, curve.endingAngle);

                //��]
                transform.rotation = GetRotationInJoint(curve.endingAngle, curve.clockwise);

                //�Ԑ��ύX���I��
                StartRunningRoad(routes.Dequeue(), nextLaneID, nextRoadJoint);
            }
            else
            {
                //���W
                transform.position = MyMath.GetPositionFromPolar(curve.center, curve.radius, currentAngle);

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
            bool shouldTurnRight = !MyMath.IsRightFromVector(transform.position, targetLanePoint, targetLaneVector);

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
        private float GetRotatoinInRoad(Vector2 alongVector)
        {
            return MyMath.GetAngular(alongVector);
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
            float angularDiference = MyMath.GetAngularDifference(startingAlongVector, endingAlongVector);
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
            output.center = MyMath.GetIntersection(startingRoadSideLinePoint, startingRoadSideLineVector, endingRoadSideLinePoint, endingRoadSideLineVector);

            //�J�[�u�̎n�_�E�I�_�����߂�
            Vector2 startingPoint = MyMath.GetFootOfPerpendicular(output.center, startingRoad.GetStartingPoint(Road.GetDifferentEdgeID(startingEdgeID), startingLaneID), startingRoad.alongVectors[Road.GetDifferentEdgeID(startingEdgeID)]);
            Vector2 endingPoint = MyMath.GetFootOfPerpendicular(output.center, endingRoad.GetStartingPoint(endingEdgeID, endingLaneID), endingRoad.alongVectors[endingEdgeID]);

            //���a�����߂�
            output.radius = Vector2.Distance(startingPoint, output.center);

            //�p�x�����߂�
            output.startingAngle = MyMath.GetAngular(startingPoint - output.center);
            output.endingAngle = MyMath.GetAngular(endingPoint - output.center);

            return output;
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
        /// �Ԃ����o
        /// </summary>
        private void DetectCars()
        {
            carsDetectedFront = LunchDetectionRayForCars(detectionRayStart, detectionRayDestinationsFront);
            carsDetectedFrontLeft = LunchDetectionRayForCars(detectionRayStart, detectionRayDestinationsFrontLeft);
            carsDetectedFrontRight = LunchDetectionRayForCars(detectionRayStart, detectionRayDestinationsFrontRight);
            carsDetectedLeft = LunchDetectionRayForCars(detectionRayStart, detectionRayDestinationsLeft);
            carsDetectedRight = LunchDetectionRayForCars(detectionRayStart, detectionRayDestinationsRight);
        }

        /// <summary>
        /// �M���@�����o
        /// </summary>
        private TrafficLight DetectTrafficLight(Road road, uint startingEdgeID)
        {
            //�n�_�Ɣ��Α��̐M���@�����o
            TrafficLight trafficLight = road.trafficLights[Road.GetDifferentEdgeID(startingEdgeID)];

            if (trafficLight.enabled)
            {
                //�M���@���N�����Ă���
                return trafficLight;
            }
            else{
                //�M���@���N������Ă��Ȃ�
                return null;
            }
        }

        /// <summary>
        /// ���o�r�[���𔭎˂��āA���o���ꂽ�Ԃ̔z���Ԃ�
        /// </summary>
        private List<Car> LunchDetectionRayForCars(Transform rayStartTransform, Transform[] rayEndTransforms)
        {
            List<Car> output = new List<Car>();

            Vector2 rayStart = rayStartTransform.position;

            foreach(Transform rayEndTransform in rayEndTransforms)
            {
                Vector2 rayEnd = rayEndTransform.position;

                //���o�r�[���𔭎�
                RaycastHit2D[] hitteds = Physics2D.RaycastAll(rayStart, rayEnd - rayStart, (rayEnd - rayStart).magnitude);

                //�Փ˂����I�u�W�F�N�g����Ԃ��
                foreach (RaycastHit2D hitted in hitteds)
                {
                    //Car�R���|�[�l���g�����邩+�������g�ł͂Ȃ������m�F
                    Car car = hitted.collider.gameObject.GetComponent<Car>();
                    if ((car != null)
                        &&(car != this))
                    {
                        //�Ԃł���
                        output.Add(car);

                        Debug.Log("Detected");
                    }
                }
            }

            return output;
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
                    return MyMath.GetPositionFromPolar(center, radius, startingAngle);
                }
            }

            public Vector2 endingPoint
            {
                get
                {
                    return MyMath.GetPositionFromPolar(center, radius, endingAngle);
                }
            }
        }
    }
}