using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    public class CarGenerator : SingletonMonoBehaviour<CarGenerator>
    {
        [Tooltip("�Ԃ̃v���n�u")]
        [SerializeField] private GameObject carPrefab;
        [Tooltip("���������Car�I�u�W�F�N�g�̐e�I�u�W�F�N�g")]
        [SerializeField] private Transform carParent;

        [Tooltip("1�b�ɏo������Ԃ̊��Ґ�")]
        [SerializeField] private float expectedCars = 1f;
        [Tooltip("1�b�ɏo������Ԃ̊��Ґ��͈̔́i�Б��͈̔͂̒����j")]
        [SerializeField] private float expectedCarsRange = 0.2f;

        [Tooltip("�X�|�[���|�C���g�i�eOutsideConnection�ƌq����[�̎Ԑ����Ɓj�ŗL�̐����C���^�[�o��")]
        [SerializeField] private float spawnIntervalInPoint = 1f;

        [Tooltip("�Ō���̎Ԃ����̋����������Ɛ������Ȃ�")]
        [SerializeField] private float notSpawningDistance = 1f;

        private SpawnPoint[] spawnPoints;

        private bool initialized = false;

        private void Update()
        {
            if (initialized)
            {
                AdvanceSpawnPointsTimers();
                SpawnCars();
            }
        }

        /// <summary>
        /// �����������B���H�ڑ���ɍs����K�v������
        /// </summary>
        public static void Initialize()
        {
            //�X�|�[���n�_���擾
            Instance.spawnPoints = GetSpawnPoints();

            //�������ς݂�
            Instance.initialized = true;
        }

        /// <summary>
        /// �eSpawnPoint�̃^�C�}�[��i�߂�
        /// </summary>
        private void AdvanceSpawnPointsTimers()
        {
            foreach(SpawnPoint spawnPoint in spawnPoints)
            {
                spawnPoint.AdvanceTimer();
            }
        }

        /// <summary>
        /// �X�|�[���n�_���擾
        /// </summary>
        private static SpawnPoint[] GetSpawnPoints()
        {
            List<SpawnPoint> pointsList = new List<SpawnPoint>();
            
            //�SOutsideConnection����
            foreach(OutsideConnection outsideConnection in FindObjectsOfType<OutsideConnection>())
            {
                //�e���H�ɂ���
                foreach(Road road in outsideConnection.connectedRoads)
                {
                    //����OutsideConnection��edge�ԍ�
                    uint edge = road.GetEdgeID(outsideConnection);

                    //�e�Ԑ��ɂ���
                    for (uint lane = 0; lane < road.lanes; lane++)
                    {
                        //�ʒu���擾
                        Vector2 spawnPosition = road.GetStartingPoint(edge, lane);

                        //SpawnPoint��o�^
                        SpawnPoint newPoint = new SpawnPoint(outsideConnection, road, lane, spawnPosition, Instance.spawnIntervalInPoint);
                        pointsList.Add(newPoint);
                    }
                }
            }

            return pointsList.ToArray();
        }

        /// <summary>
        /// �Ԃ𐶐�����BUpdate�֐�����ĂԂ���
        /// </summary>
        private void SpawnCars()
        {
            //���ꂩ�琶������Ԑ�
            int spawningCars = GetSpawningCarsN();

            //�g�p����SpawnPoint�̏��Ԃ����߂�
            Queue<SpawnPoint> spawnPointsOrder = new Queue<SpawnPoint>(ShuffleSpawnPoints(spawnPoints));

            //�w�肳�ꂽ�Ԃ̐���������
            for(int carNum = 0; carNum < spawningCars; carNum++)
            {
                //�g�p�\��SpawnPoint
                SpawnPoint spawnPoint = GetAvailableSpawnPoint(spawnPointsOrder);
                
                //�g�p�\��spawnPoint���Ȃ��Ȃ����璆�~
                if (spawnPoint == null)
                {
                    break;
                }

                //�I�u�W�F�N�g����
                InstantiateCar(spawnPoint);
            }
        }

        /// <summary>
        /// �X�|�[�������Z�o
        /// </summary>
        private int GetSpawningCarsN()
        {
            //���Ғl�͈̔�
            float expectedMin = expectedCars - expectedCarsRange;
            float expectedMax = expectedCars + expectedCarsRange;

            //���̃t���[���ɏo���ׂ��Ԃ̗ʂ��Z�o
            float generatingCarsF = Random.Range(expectedMin, expectedMax) * Time.deltaTime;

            //>>������
            int generatingCarsI = (int)generatingCarsF;

            //�[���̏���
            if (Random.value <= generatingCarsF - generatingCarsI)
            {
                generatingCarsI++;
            }

            return generatingCarsI;
        }

        /// <summary>
        /// �g�p����SpawnPoint�̏��Ԃ����߂�
        /// </summary>
        private SpawnPoint[] ShuffleSpawnPoints(SpawnPoint[] spawnPoints)
        {
            SpawnPoint[] _spawnPoints = (SpawnPoint[])spawnPoints.Clone();
            
            //�V���b�t������
            for(int cnt = 0; cnt < _spawnPoints.Length; cnt++)
            {
                SpawnPoint temp = _spawnPoints[cnt];
                int randomIndex = Random.Range(0, _spawnPoints.Length);
                _spawnPoints[cnt] = _spawnPoints[randomIndex];
                _spawnPoints[randomIndex] = temp;
            }

            //Queue�ɂ���
            return _spawnPoints;
        }

        /// <summary>
        /// �g�p�\��SpawnPoint��Ԃ�
        /// </summary>
        /// <return>�g�p�\��SpawnPoint���Ȃ��Ȃ�����null</return>
        private SpawnPoint GetAvailableSpawnPoint(Queue<SpawnPoint> spawnPoints)
        {
            //�L���[���Ȃ��Ȃ�܂�
            while(spawnPoints.Count > 0)
            {
                //�L���[�̐擪�����o��
                SpawnPoint checking = spawnPoints.Dequeue();

                //�g�p�\���m�F
                if (CheckSpawnPointAvailable(checking))
                {
                    //>>�g�p�\
                    
                    //�^�C�}�[���Z�b�g
                    checking.ResetTimer();

                    //�Ԃ�
                    return checking;
                }

                //>>�g�p�\�łȂ��ˎ���
            }

            //�����܂ŗ�����A�g�p�\��SpawnPoint���Ȃ�
            return null;
        }

        /// <summary>
        /// �X�|�[���|�C���g�����p�\���m�F����
        /// </summary>
        private bool CheckSpawnPointAvailable(SpawnPoint spawnPoint)
        {
            //�^�C�}�[�ɂ���
            if(spawnPoint.timer < spawnIntervalInPoint)
            {
                //>>�^�C�}�[���܂��������Ă��Ȃ�
                return false;
            }
            //>>�^�C�}�[���������Ă���

            //�Ō���Ƃ̋����ɂ���
            Car tailCar = GetTailCar(spawnPoint);
            if(tailCar != null)
            {
                //>>�Ō��������
                float distance = Vector2.Distance(tailCar.transform.position, spawnPoint.position);
                if (distance <= notSpawningDistance)
                {
                    //�߂�����
                    return false;
                }
            }

            //�\
            return true;
        }

        /// <summary>
        /// �Ō���̎Ԃ��擾����
        /// </summary>
        /// <returns>���݂��Ȃ��ꍇ��null</returns>
        private Car GetTailCar(SpawnPoint spawnPoint)
        {
            //���o�r�[������
            Road road = spawnPoint.road;
            Vector2 alongVector = road.alongVectors[road.GetEdgeID(spawnPoint.roadJoint)];
            RaycastHit2D[] hitteds = Physics2D.RaycastAll(spawnPoint.position, alongVector, alongVector.magnitude);

            //�ł��߂����̂�T��
            float nearestDistance = float.MaxValue;
            Car nearestCar = null;
            foreach(RaycastHit2D hitted in hitteds)
            {
                GameObject opponent = hitted.collider.gameObject;

                //Car�I�u�W�F�N�g�̂�
                Car car = opponent.GetComponent<Car>();
                if (car == null)
                {
                    //>>Car�łȂ�
                    //��΂�
                    continue;
                }
                //>>Car�ł���

                //���l���Ă��铹�H�ɑ��݂��Ă��邩
                if(car.currentRoad != spawnPoint.road)
                {
                    //>>�Ⴄ���H
                    //��΂�
                    continue;
                }

                //���������߂�
                float distance = Vector2.Distance(spawnPoint.position, car.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCar = car;
                }
            }

            return nearestCar;
        }

        /// <summary>
        /// �ԃI�u�W�F�N�g�𐶐�
        /// </summary>
        private void InstantiateCar(SpawnPoint spawnPoint)
        {
            //�I�u�W�F�N�g����
            GameObject carObject = Instantiate(carPrefab, carParent);

            //�������i�ʒu���킹��Car�N���X���S���j
            carObject.GetComponent<Car>().Initialize(spawnPoint.roadJoint, spawnPoint.road, spawnPoint.laneID, null);
        }

        /// <summary>
        /// �X�|�[���n�_�̊Ǘ�
        /// </summary>
        private class SpawnPoint
        {
            public RoadJoint roadJoint { get; private set; }
            public Road road { get; private set; }
            public uint laneID { get; private set; }
            public Vector2 position { get; private set; }

            public float timer { get; private set; }

            //�R���X�g���N�^
            public SpawnPoint(RoadJoint roadJoint, Road road, uint laneID, Vector2 position, float timerMax)
            {
                this.roadJoint = roadJoint;
                this.road = road;
                this.laneID = laneID;
                this.position = position;
                this.timer = timerMax;      //�ŏ�����g�p�\��
            }

            /// <returns>���݂̃^�C�}�[</returns>
            public float AdvanceTimer()
            {
                timer += Time.deltaTime;
                return timer;
            }

            public void ResetTimer()
            {
                timer = 0f;
            }
        }
    }
}