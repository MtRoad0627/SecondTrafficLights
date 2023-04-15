using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class Car : MonoBehaviour
    {
        private enum State
        {
            runningRoad,
            runningJoint,
            changingLane
        }

        private State state;


        [Header("runningRoad�̑��x���f��")]

        [Tooltip("�����x�ꎞ��T�i�b�j")]
        [SerializeField] private float runningRoadT = 0.74f;

        [Tooltip("��]���xv0�i�O���[�o�����W�j")]
        [SerializeField] private float runningRoadV0 = 5f;

        [Tooltip("�ɘa����t_1�i�b�j")]
        [SerializeField] private float runningRoadT1 = 2.45f;

        [Tooltip("�ɘa����t_2�i�b�j")]
        [SerializeField] private float runningRoadT2 = 0.77f;

        [Tooltip("���ݍ�p����R�i�O���[�o�����W�j")]
        [SerializeField] private float runningRoadR = 1f;

        [Tooltip("���ݍ�p����R'�i�O���[�o�����W�j")]
        [SerializeField] private float runningRoadRp = 20f;

        [Tooltip("��Ԏ��̎Ԋԋ���d")]
        [SerializeField] private float runningRoadD = 0.5f;

        [Tooltip("�X�|�[�����̑��x�̌W���iv0�Ɋ|���Z����j")]
        [SerializeField] private float spawnedSpeedCoef = 0.75f;

        [Tooltip("���̊p�x�ȓ��Ȃ瑼�̎Ԃ����������𑖂��Ă���Ɣ��f")]
        [SerializeField] private float runningRoadSameDirectionThreshold = 60f;

        [Tooltip("�Ό��Ԃ����̋����ȓ��ɗ������Ԃ���B")]
        [SerializeField] private float runningRoadStopDistanceThreshold = 0.5f;

        [Header("runningJoint�̑��x���f��")]

        [Tooltip("�����x�ꎞ��T�i�b�j")]
        [SerializeField] private float runningJointT = 0.74f;

        [Tooltip("��]���xv0�i�O���[�o�����W�j")]
        [SerializeField] private float runningJointV0 = 2f;

        [Tooltip("�ɘa����t_1�i�b�j")]
        [SerializeField] private float runningJointT1 = 2.45f;

        [Tooltip("�ɘa����t_2�i�b�j")]
        [SerializeField] private float runningJointT2 = 0.77f;

        [Tooltip("���ݍ�p����R�i�O���[�o�����W�j")]
        [SerializeField] private float runningJointR = 1f;

        [Tooltip("���ݍ�p����R'�i�O���[�o�����W�j")]
        [SerializeField] private float runningJointRp = 20f;

        [Tooltip("��Ԏ��̎Ԋԋ���d")]
        [SerializeField] private float runningJointD = 0.5f;

        [Tooltip("���̊p�x�ȓ��Ȃ瑼�̎Ԃ����������𑖂��Ă���Ɣ��f")]
        [SerializeField] private float runningJointSameDirectionThreshold = 60f;

        [Tooltip("�Ό��Ԃ����̋����ȓ��ɗ������Ԃ���B")]
        [SerializeField] private float runningJointStopDistance = 0.5f;

        [Header("changingLane�̑��x���f��")]

        [Tooltip("�O�̎ԂƂ̋��������ꖢ���Ȃ���")]
        [SerializeField] private float changingLaneStopDistance = 0.5f;

        [Header("���x�֌W")]

        [Tooltip("�Ԑ��ύX���񓪂̉�]���x")]
        [SerializeField] private float changingLaneRotationSpeed = 10f;

        [Tooltip("�Ԑ��ύX����]�ړ��̉�]���x")]
        [SerializeField] private float changingLaneAngularSpeed = 10f;

        [Header("�X�R�A�E�����x�֌W")]

        [Tooltip("���̊Ԋu�ő��x���L�^����i�b�j")]
        [SerializeField] private float saveSpeedInterval = 0.5f;

        [Tooltip("�����x�𑝌�������Ԋu�i�b�j")]
        [SerializeField] private float happinessCalculationInterval = 1f;

        [Tooltip("�����x������臒l�i��]���x�ɑ΂��銄���j")]
        [SerializeField] private float[] happinessChangeThresholds;

        [Tooltip("�����x�̕ω���")]
        [SerializeField] private int[] happinessChangements;

        [Tooltip("�����x�̏����l")]
        [SerializeField] private int happiness = 80;

        private const int happinessMin = 0;
        private const int happinessMax = 100;

        private float saveSpeedTimer = 0f;
        private List<float> savedSpeeds = new List<float>();
        private float happinessCalculationTimer = 0f;

        public float happinessRatio
        {
            get
            {
                return (float)(happiness - happinessMin) / (float)happinessMax;
            }
        }

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
        /// �O�t���[���܂ł̑��x
        /// </summary>
        public float currentSpeed { get; private set; }

        /// <summary> 
        /// ���ݎg���Ă铹�����x�N�g��
        /// </summary>
        public Vector2 currentAlongRoad { get; private set; }

        /// <summary>
        /// �����Ă�������EdgeID�BrunningRoad�J�n���ɍX�V
        /// </summary>
        public uint currentEdgeIDFrom { get; private set; }

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
        /// ����RunningRoad��laneID�BrunningRoad�J�n���ɍX�V
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

        [Header("���̑�")]
        [Tooltip("���꒼����Ɣ��f����O�ς�臒l")]
        [SerializeField] private float onSameLineThreshold = 0.05f;

        [Tooltip("�J���[�����O")]
        [SerializeField] private CarColor colorObject;

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

        private void Start()
        {
            InitializeSpeed();
            colorObject.UpdateColor(happinessRatio);
        }

        private void Update()
        {
            Run();
            Detect();
            ManageHappiness();
        }

        /// <summary>
        /// �X�|�[�����̏����x��ݒ�
        /// </summary>
        private void InitializeSpeed()
        {
            currentSpeed = runningRoadV0 * spawnedSpeedCoef;
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
            currentEdgeIDFrom = edgeID;

            //���݈ʒu�𓹘H�ɊJ�n�ʒu�ɒ���
            AdjustStartingPositionInRoad(road, laneID, edgeID, first);

            //���݂̍s����̍��W
            Vector2 destinationPoint;

            if(routes.Count > 0)
            {
                //���ɑ���Ԑ����擾
                Road[] nextRoads = routes.ToArray();
                if (routes.Count >= 2)
                {
                    //�u���̎��v������
                    nextLaneID = GetNextLane(nextRoads[0], nextRoads[1]);
                }
                else
                {
                    //���̎��͏I�_
                    nextLaneID = GetNextLane(nextRoads[0], null);
                }

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
                nextLaneID
                );

            return true;
        }

        /// <summary>
        /// ����Road�ő���Ԑ���I��
        /// </summary>
        private uint GetNextLane(Road roadSelecting, Road roadNext)
        {
            uint output = 0;

            switch (roadSelecting.lanes)
            {
                case 1:
                    output = 0;
                    break;

                case 2:
                    output = ChooseLaneFrom2(roadSelecting, roadNext);
                    break;

                default:
                    Debug.LogError("�������G���[");
                    output = 0;
                    break;
            }

            nextLaneID = output;
            return nextLaneID;
        }

        /// <summary>
        /// 2�Ԑ�����Ԑ���I��
        /// </summary>
        private uint ChooseLaneFrom2(Road selectingRoad, Road nextRoad)
        {
            //���ʂ���RoadJoint��T��
            RoadJoint commonJoint = RoadJoint.FindCommonJoint(selectingRoad, nextRoad);

            if(nextRoad == null)
            {
                //>>�����I�_�̂Ƃ�
                return (uint)Random.Range(0, 2);
            }

            int fromIndex = commonJoint.connectedRoads.IndexOf(selectingRoad);
            int toIndex = commonJoint.connectedRoads.IndexOf(nextRoad);

            int leftHandIndex = (int)fromIndex - 1;

            switch (commonJoint.connectedRoads.Count)
            {
                case 2:
                    return currentLane;

                case 3:
                    if(leftHandIndex < 0)
                    {
                        leftHandIndex += 3;
                    }

                    if (leftHandIndex == toIndex)
                    {
                        //���葤�ɍs���\��
                        //�����Ԑ���
                        return 0;
                    }
                    else
                    {
                        //�E�葤�ɍs���\��
                        //�E���Ԑ���
                        return 1;
                    }

                case 4:
                    if (leftHandIndex < 0)
                    {
                        leftHandIndex += 4;
                    }

                    if (leftHandIndex == toIndex)
                    {

                        //���葤�ɍs���\��
                        //�����Ԑ���
                        return 0;
                    }
                    else
                    {
                        //�^�񒆁A�E�葤�ɍs���\��
                        //�E���Ԑ���
                        return 1;
                    }

                default:
                    Debug.LogError("�������G���[");
                    return 0;
            }
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
            Road nextRoad;
            uint nextEdgeID = 0;
            if (routes.Count > 0)
            {
                nextRoad = routes.Peek();
                nextEdgeID = nextRoad.GetEdgeID(nextRoadJoint);
            }
            else
            {
                nextRoad = null;
            }
            
            float advancedDistance = GetSpeedInRoad(currentRoad, currentEdgeIDFrom, nextRoad, nextEdgeID) * Time.deltaTime;
            //�O���i���[���h�j�I
            transform.position += (Vector3)(currentAlongRoad.normalized * advancedDistance);
            //�����I
            currentDistanceInRoad += advancedDistance;
        }

        /// <summary>
        /// runningRoad���̏󋵂ɑΉ����鑬�x�����߂�
        /// </summary>
        private float GetSpeedInRoad(Road currentRoad, uint currentEdgeIDFrom, Road nextRoad, uint nextEdgeIDFrom)
        {
            //�O�𑖂��Ă���Ԃ��擾
            Car frontCar = GetFrontCar();

            //�O�̎Ԃ����݂��Ȃ��Ƃ��̃p�����[�^�[
            float frontSpeed = runningRoadV0;
            float s = float.MaxValue;

            if (frontCar != null)
            {
                float distance = Vector2.Distance(frontCar.transform.position, this.transform.position);

                bool isRelatedRoad = ((frontCar.currentRoad == currentRoad) && (frontCar.currentEdgeIDFrom == currentEdgeIDFrom)) 
                    || ((frontCar.currentRoad == nextRoad) && (frontCar.currentEdgeIDFrom == nextEdgeIDFrom));

                if ((Mathf.Abs(MyMath.GetAngularDifference(frontCar.front, this.front)) > runningRoadSameDirectionThreshold)
                    && (frontCar.state != State.runningRoad)
                    && (distance <= runningRoadStopDistanceThreshold))
                {
                    //>>��ԏ����F�Ό��Ԃ����Ă���+臒l���߂�+

                    //>>臒l���߂�
                    //��Ԃ���
                    currentSpeed = 0f;
                    return currentSpeed;
                }
                else if (!((frontCar.state == State.runningRoad) && !isRelatedRoad))
                {
                    //>>�ʏ펞�F�֌W�Ȃ����H��runningRoad�����O

                    //�O�𑖂��Ă���Ԃ����݂���
                    frontSpeed = frontCar.currentSpeed;
                    s = distance;
                }
            }

            //�M���@
            if ((currentTrafficLight != null)
                && (currentTrafficLight.color != TrafficLight.Color.green))
            {
                //�Ώۂ̐M���@�����݂��Ă��āA���F���ԐF

                //����
                float distanceFromLight = Vector2.Distance(this.transform.position, currentTrafficLight.transform.position);

                if (s >= distanceFromLight)
                {
                    //>>�ΏۂƂ̋������߂�
                    //�O�ɒ�~�ԗ�������Ƃ��Ă���ւ�
                    s = distanceFromLight;
                    frontSpeed = 0f;
                }
            }

            //���x�v�Z
            currentSpeed = CalculateGFM(
                    currentSpeed,
                    s,
                    frontSpeed,
                    runningRoadT,
                    runningRoadV0,
                    runningRoadT1,
                    runningRoadT2,
                    runningRoadR,
                    runningRoadRp,
                    runningRoadD
                );

            return currentSpeed;
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
        /// �Ԑ��ύX���A�O�i���Ȃ���Ȃ���
        /// </summary>
        private void ChangeLaneForward(Vector2 targetLanePoint, Vector2 targetLaneVector)
        {
            //�Ȃ���������Z�o
            bool shouldTurnRight = !MyMath.IsRightFromVector(transform.position, targetLanePoint, targetLaneVector);

            //�i�s�����ɑ΂���Ԑ��̊p�x
            float angularDifference = Vector2.Angle(front, targetLaneVector);

            //��]�p���Z�o
            float angularMovement = Mathf.Min(angleMaxChangingLane - angularDifference, changingLaneRotationSpeed* Time.deltaTime);

            if (shouldTurnRight)
            {
                //�E�ɋȂ���ꍇ�A�������]
                angularMovement = -angularMovement;
            }

            //��]
            transform.rotation = transform.rotation * Quaternion.Euler(0, 0, angularMovement);

            //��]��ɑO�i
            Road[] roads = routes.ToArray();
            Road currentRoad = roads[0];
            uint currentRoadEdgeIDFrom = currentRoad.GetEdgeID(nextRoadJoint);
            Road nextRoad;
            uint nextRoadEdgeIDFrom = 0;
            if (roads.Length > 1)
            {
                nextRoad = roads[1];
                nextRoadEdgeIDFrom = nextRoad.GetEdgeID(RoadJoint.FindCommonJoint(currentRoad, nextRoad));
            }
            else
            {
                nextRoad = null;
            }
            
            transform.position += (Vector3)(front.normalized * GetSpeedInRoad(currentRoad, currentRoadEdgeIDFrom, nextRoad, nextRoadEdgeIDFrom) * Time.deltaTime);
        }

        /// <summary>
        /// �Ԑ��ύX���́A�Ō�̉�]�ړ�
        /// </summary>
        private void ChangeLaneRotation(Vector2 targetLanePoint, Vector2 targetLaneVector, CurveRoute curve)
        {
            //�p�x���ړ�������
            if (curveChangingLane.clockwise)
            {
                currentAngle -= GetAngularSpeedInChangingLane(curve.radius) * Time.deltaTime;
            }
            else
            {
                currentAngle += GetAngularSpeedInChangingLane(curve.radius) * Time.deltaTime;
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

        private float GetAngularSpeedInChangingLane(float radius)
        {
            Car frontCar = GetFrontCar();

            if(frontCar != null)
            {
                //>>�O�̎Ԃ�����

                float distance = Vector2.Distance(frontCar.transform.position, this.transform.position);

                if (distance <= changingLaneStopDistance)
                {
                    //>>�߂�����
                    //���
                    currentSpeed = 0f;
                    return currentSpeed;
                }
            }
           
            return changingLaneAngularSpeed;
        }

        /// <summary>�Ƃ�
        /// �ړI�n�ɓ������āA������GameManager�ɕ�
        /// </summary>
        private void OnArrivedDestination()
        {
            float speedAverage = CalculateAverageSpeed();

            //GameManager�ɒʒB
            GameManager.Instance.OnCarArrived(speedAverage, runningRoadV0);

            //������
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Joint��]���̊p���x
        /// </summary>
        /// <returns></returns>
        private float GetAngularSpeedInJoint()
        {
            //�O�𑖂��Ă���Ԃ��擾
            Car frontCar = DetectFrontCarInJoint(currentCurveRoute);

            //�O�̎Ԃ����݂��Ȃ��Ƃ��̃p�����[�^�[
            float frontSpeed = runningJointV0;
            float s = float.MaxValue;

            if (frontCar != null)
            {
                float distance = Vector2.Distance(frontCar.transform.position, this.transform.position);

                if ((Mathf.Abs(MyMath.GetAngularDifference(frontCar.front, this.front)) > runningJointSameDirectionThreshold)
                    && (s <= runningJointStopDistance)
                    && !((frontCar.state == State.runningRoad) && (frontCar.currentRoad != this.routes.Peek())))
                {
                    //>>��ԏ����F�Ό��Ԃ����Ă���+臒l���߂�+

                    //>>臒l���߂�
                    //��Ԃ���
                    currentSpeed = 0f;
                    return currentSpeed;
                }
                else if(!((frontCar.state == State.runningRoad) && (frontCar.currentRoad != this.routes.Peek())))
                {
                    //>>�ʏ펞�F���̓��H��runningRoad���Ă���Car�͏��O

                    //�O�𑖂��Ă���Ԃ����݂���
                    frontSpeed = frontCar.currentSpeed;
                    s = distance;
                }
            }

            //���x�v�Z
            currentSpeed = CalculateGFM(
                    currentSpeed,
                    s,
                    frontSpeed,
                    runningJointT,
                    runningJointV0,
                    runningJointT1,
                    runningJointT2,
                    runningJointR,
                    runningJointRp,
                    runningJointD
                );

            //�p���x�ɕϊ����ĕԂ�
            return MyMath.GetAngularSpeed(currentSpeed, currentCurveRoute.radius);
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
        /// Generalized Force Model���v�Z
        /// </summary>
        private float CalculateGFM(
            float formerSpeed,
            float s,
            float frontSpeed,
            float T,
            float v0,
            float t1,
            float t2,
            float r,
            float rp,
            float d
            )
        {

            float sFunc = d + T * formerSpeed;

            float dv = formerSpeed - frontSpeed;

            float V = v0 * (1f - Mathf.Exp(-(s - sFunc) / r));

            float theta;
            if (dv <= 0)
            {
                theta = 0f;
            }
            else
            {
                theta = 1f;
            }

            float xpp = (V - formerSpeed) / t1 - ((dv * theta) / t2) * Mathf.Exp(-(s - sFunc) / rp);

            float outputSpeed = formerSpeed + xpp * Time.deltaTime;

            if (outputSpeed < 0f)
            {
                outputSpeed = 0f;
            }

            return outputSpeed;
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
            Vector2 rayStart = rayStartTransform.position;
            Vector2[] rayEnds = new Vector2[rayEndTransforms.Length];
            for(int cnt = 0; cnt < rayEndTransforms.Length; cnt++)
            {
                rayEnds[cnt] = rayEndTransforms[cnt].position;
            }

            LunchDetectionRayForCars(rayStart, rayEnds);

            return LunchDetectionRayForCars(rayStart, rayEnds);
        }

        /// <summary>
        /// ���o�r�[���𔭎˂��āA���o���ꂽ�Ԃ̔z���Ԃ�
        /// </summary>
        private List<Car> LunchDetectionRayForCars(Vector2 rayStart, Vector2[] rayEnds)
        {
            List<Car> output = new List<Car>();

            foreach (Vector2 rayEnd in rayEnds)
            {
                //���o�r�[���𔭎�
                output.AddRange(LunchDetectionRayForCars(rayStart, rayEnd));
            }

            return output;
        }

        /// <summary>
        /// ���o�r�[���𔭎˂��āA���o���ꂽ�Ԃ̔z���Ԃ�
        /// </summary>
        private List<Car> LunchDetectionRayForCars(Vector2 rayStart, Vector2 rayEnd)
        {
            List<Car> output = new List<Car>();
            
            //���o�r�[���𔭎�
            RaycastHit2D[] hitteds = Physics2D.RaycastAll(rayStart, rayEnd - rayStart, (rayEnd - rayStart).magnitude);

            //�Փ˂����I�u�W�F�N�g����Ԃ��
            foreach (RaycastHit2D hitted in hitteds)
            {
                //Car�R���|�[�l���g�����邩+�������g�ł͂Ȃ������m�F
                Car car = hitted.collider.gameObject.GetComponent<Car>();
                if ((car != null)
                    && (car != this))
                {
                    //�Ԃł���
                    output.Add(car);
                }
            }

            return output;
        }

        /// <summary>
        /// �������H��runningRoad���Ă�����Car���擾
        /// </summary>
        private Car GetFrontCar()
        {
            //�ł��߂����̂���`�T��
            float nearestDistance = float.MaxValue;
            Car nearestCar = null;
            foreach (Car car in carsDetectedFront)
            {
                //�j��ς݂Ȃ��΂�
                if (car == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(car.transform.position, this.transform.position);

                if (distance < nearestDistance)
                {
                    nearestCar = car;
                    nearestDistance = distance;
                }
            }

            return nearestCar;
        }

        /// <summary>
        /// runningJoint���̈�O��Car���擾
        /// </summary>
        private Car GetFrontCarRunningJoint()
        {
            //�T���Ώۂ��
            List<Car> targets = new List<Car>();
            targets.AddRange(carsDetectedFront);
            if (currentCurveRoute.clockwise)
            {
                //���v���Ȃ�E������
                targets.AddRange(carsDetectedFrontRight);
                targets.AddRange(carsDetectedRight);
            }
            else
            {
                //�����v���Ȃ獶������
                targets.AddRange(carsDetectedFrontLeft);
                targets.AddRange(carsDetectedLeft);
            }

            //�ł��߂����̂���`�T��
            float nearestDistance = float.MaxValue;
            Car nearestCar = null;
            foreach (Car car in targets)
            {
                //�j��ς݂Ȃ��΂�
                if (car == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(car.transform.position, this.transform.position);

                if (distance < nearestDistance)
                {
                    nearestCar = car;
                    nearestDistance = distance;
                }
            }

            return nearestCar;
        }

        private Car DetectFrontCarInJoint(CurveRoute curveRoute)
        {
            const float angleUnit = 5f;

            //>>�~�ʏ�Ɍ��o�r�[�����o��
            float angle = currentAngle;
            float angleEnd = curveRoute.endingAngle;
            Car target = null;
            Vector2 ending = Vector2.zero;
            while(CheckCircularFinished(angle, curveRoute))
            {
                float rayStartAngle = angle;

                if (curveRoute.clockwise)
                {
                    //���v���
                    angle -= angleUnit;
                }
                else
                {
                    //�����v���
                    angle += angleUnit;
                }

                float rayEndAngle = angle;

                Vector2 rayStart = MyMath.GetPositionFromPolar(curveRoute.center, curveRoute.radius, rayStartAngle);
                Vector2 rayEnd = MyMath.GetPositionFromPolar(curveRoute.center, curveRoute.radius, rayEndAngle);

                ending = rayEnd;

                List<Car> hittedCars = LunchDetectionRayForCars(rayStart, rayEnd);
                if (hittedCars.Count > 0)
                {
                    //>>����
                    //rayStart�ɍł��߂����̂�T��
                    Car nearestCar = null;
                    float nearestDistance = float.MaxValue;
                    foreach(Car car in hittedCars)
                    {
                        float distance = Vector2.Distance(car.transform.position, rayStart);
                        if ((car.state == State.runningJoint)
                            &&(distance < nearestDistance))
                        {
                            nearestDistance = distance;
                            nearestCar = car;
                        }
                    }

                    if (nearestCar != null)
                    {
                        target = nearestCar;
                        break;
                    }
                }
            }

            //�����o�̏ꍇ�A���̓��H�̔���������
            if(target == null)
            {
                Road nextRoad = routes.Peek();

                Vector2 rayStart = ending;
                Vector2 direction = nextRoad.alongVectors[nextRoad.GetEdgeID(curveRoute.curvingJoint)] / 2f;

                List<Car> cars = LunchDetectionRayForCars(rayStart, rayStart + direction);

                //�ł��߂����̂�T��
                Car nearestCar = null;
                float nearestDistance = float.MaxValue;
                foreach(Car car in cars)
                {
                    float distance = Vector2.Distance(car.transform.position, rayStart);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestCar = car;
                    }
                }

                target = nearestCar;
            }

            return target;
        }

        /// <summary>
        /// �����x�̊Ǘ�������
        /// </summary>
        private void ManageHappiness()
        {
            //���x�̕ۑ�
            SaveSpeed();

            //�����x���v�Z
            CalculateHappiness();

            //�F���X�V
            colorObject.UpdateColor(happinessRatio);
        }

        /// <summary>
        /// ���x��ۑ�����
        /// </summary>
        private void SaveSpeed()
        {
            //�^�C�}�[�i�߂�
            saveSpeedTimer += Time.deltaTime;

            if(saveSpeedTimer < saveSpeedInterval)
            {
                //�܂��^�C�}�[���؂�Ă��Ȃ�
                return;
            }
            //>>�^�C�}�[���؂ꂽ

            //���x��ۑ�
            savedSpeeds.Add(currentSpeed);

            //�^�C�}�[���Z�b�g
            saveSpeedTimer = saveSpeedTimer % saveSpeedInterval;
        }

        /// <summary>
        /// �����x�̌v�Z
        /// </summary>
        private void CalculateHappiness()
        {
            //�^�C�}�[�i�߂�
            happinessCalculationTimer += Time.deltaTime;

            if(happinessCalculationTimer < happinessCalculationInterval)
            {
                //�܂��^�C�}�[���؂�Ă��Ȃ�
                return;
            }
            //>>�^�C�}�[���؂ꂽ

            //�����x�𑝌�
            int changement = GetHappinessChangement();
            happiness += changement;

            //�͈͊Ǘ�
            if (happiness < happinessMin)
            {
                happiness = happinessMin;
            }
            else if (happiness > happinessMax)
            {
                happiness = happinessMax;
            }

            //�^�C�}�[���Z�b�g
            happinessCalculationTimer = happinessCalculationTimer % happinessCalculationInterval;
        }

        /// <summary>
        /// �K���x�̕ω��ʂ��v�Z
        /// </summary>
        private int GetHappinessChangement()
        {
            //�ō����x�ɑ΂��銄��
            float speedRatio = currentSpeed / runningJointV0;

            //�Ή�����ω��ʂ�T��
            int output = happinessChangements[happinessChangeThresholds.Length - 1];
            for(int cnt = 0; cnt < happinessChangeThresholds.Length; cnt++)
            {
                if(speedRatio <= happinessChangeThresholds[cnt])
                {
                    //cnt���Y������C���f�b�N�X
                    output = happinessChangements[cnt];
                    
                    break;
                }
            }

            return output;
        }

        /// <summary>
        /// ���ϑ��x���v�Z
        /// </summary>
        private float CalculateAverageSpeed()
        {
            float sum = 0f;
            foreach(float speed in savedSpeeds)
            {
                sum += speed;
            }

            return sum / savedSpeeds.Count;
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