using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    /// <summary>
    /// Car������o�H�����肷��
    /// </summary>
    public class Navigator : SingletonMonoBehaviour<Navigator>
    {
        /// <summary>
        /// A*�p�̃m�[�h
        /// </summary>
        private class Node
        {
            public RoadJoint roadJoint;
            public int parent;

            /// <summary>
            /// ��ԃR�X�g
            /// </summary>
            public float g;

            /// <summary>
            /// �����R�X�g
            /// </summary>
            public float h;

            /// <summary>
            /// �����R�X�g
            /// </summary>
            public float f
            {
                get
                {
                    return CalculateF(g, h);
                }
            }

            //�R���X�g���N�^
            public Node(RoadJoint roadJoint)
            {
                this.roadJoint = roadJoint;
            }

            /// <summary>
            /// �p�����[�^�[��������
            /// </summary>
            public void Initialize()
            {
                g = 1000000f;
                h = 1000000f;
                parent = -1;
            }

            /// <summary>
            /// 2��Node����v���邩�m�F
            /// </summary>
            public bool Equals(Node other)
            {
                if (this.roadJoint == other.roadJoint)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void SetAll(float g = -1, float h = -1)
            {
                if (g >= -0.001f){
                    this.g = g;
                }
                
                if (h >= -0.001f)
                {
                    this.h = h;
                }
            }
        }

        /// <summary>
        /// �אڍs��
        /// </summary>
        private Road[,] adjacency;

        /// <summary>
        /// �אڍs��͂��̏�
        /// </summary>
        private Node[] nodes;

        /// <summary>
        /// �{�N���X�̃Z�b�g�A�b�v�������s���B
        /// GetRoute()�O�ɍs����K�v������A���H�ڑ��̌�ɌĂ΂��K�v������B
        /// </summary>
        public void SetUp()
        {
            //�SRoadJoint
            RoadJoint[] roadJoints = FindObjectsOfType<RoadJoint>();

            //node�ɕϊ�
            nodes = new Node[roadJoints.Length];
            for(int cnt = 0; cnt < roadJoints.Length; cnt++)
            {
                nodes[cnt] = new Node(roadJoints[cnt]);
            }

            //�אڍs����쐬
            Road[] roads = FindObjectsOfType<Road>();
            adjacency = MakeAdjacencyMatrix(nodes, roads);
        }

        /// <summary>
        /// ���B���[�g��Ԃ�
        /// </summary>
        public Road[] GetRoute(RoadJoint start, RoadJoint destination)
        {
            //�S�m�[�h������
            InitializeNodes(nodes);

            //�אڍs���̃C���f�b�N�X
            int startIndex = FindIndex(start, nodes);
            int destinationIndex = FindIndex(destination, nodes);

            nodes[startIndex].SetAll(0, 0);

            Vector2 destinationPosition = nodes[destinationIndex].roadJoint.transform.position;

            HashSet<int> openHash = new HashSet<int>();
            HashSet<int> closedHash = new HashSet<int>();

            openHash.Add(startIndex);

            while (openHash.Count > 0)
            {
                //�Ȃ�ł�����
                int currentIndex = GetMinOfHash(openHash);

                foreach (int index in openHash)
                {
                    if (nodes[index].f < nodes[currentIndex].f)
                    {
                        currentIndex = index;
                    }
                }

                openHash.Remove(currentIndex);
                closedHash.Add(currentIndex);

                //����
                if (currentIndex == destinationIndex)
                {
                    List<int> answerRouteNodes = new List<int>();

                    int now = currentIndex;

                    while(now != -1)
                    {
                        answerRouteNodes.Add(now);
                        now = nodes[now].parent;
                    }

                    //���]�i�n�_���I�_�̏��ɂ���j
                    answerRouteNodes.Reverse();

                    return ConvertToRoadRoute(answerRouteNodes).ToArray();
                }

                //�q�m�[�h�̎w��
                List<int> children = new List<int>();

                for(int cnt = 0; cnt < nodes.Length; cnt++)
                {
                    //�R�X�g�����i�אڂ��Ă��Ȃ��j�Ȃ疳��
                    if (adjacency[currentIndex, cnt] == null){
                        //>>�אڂ��Ă��Ȃ�
                        //��������
                        continue;
                    }
                    else
                    {
                        //>>�אڂ��Ă���
                        //�q�m�[�h�̔ԍ���ۑ�
                        children.Add(cnt);
                    }   
                }

                //�R�X�g�̌v�Z
                foreach(int child in children)
                {
                    //���݂̃R�X�g�Ɉړ��R�X�g�𑫂�
                    float g = nodes[currentIndex].g + GetCostG(currentIndex, child);
                    Vector2 childPosition = nodes[child].roadJoint.transform.position;

                    //�ړ���m�[�h�i�q�m�[�h�j�ƖړI�n�̃��[�N���b�h���������߂�
                    float h = Vector2.Distance(destinationPosition, childPosition);

                    float f = CalculateF(g, h);
                    if (nodes[child].f > f)
                    {
                        nodes[child].parent = currentIndex;
                        nodes[child].SetAll(g, h);
                    }
                    else
                    {
                        continue;
                    }

                    //���łɃI�[�v���\��łȂ���΃n�b�V���ɉ�����
                    if (openHash.Contains(child))
                    {
                        continue;
                    }
                    else
                    {
                        openHash.Add(child);
                    }
                }
            }

            //�����܂ŗ�����A�S�[���ł��郋�[�g������
            Debug.LogError("���B�s�\�ȃ��[�g");

            return null;
        }

        /// <summary>
        /// �אڍs������
        /// </summary>
        private Road[,] MakeAdjacencyMatrix(Node[] nodes, Road[] roads)
        {
            //������
            Road[,] adjacency = new Road[nodes.Length, nodes.Length];

            //�S���H��o�^
            foreach(Road road in roads)
            {
                //�ڑ�����RoadJoint
                RoadJoint[] connected = road.connectedJoints;

                //����Index
                int index0 = FindIndex(connected[0], nodes);
                int index1 = FindIndex(connected[1], nodes);

                //�o�^
                adjacency[index0, index1] = road;
                adjacency[index1, index0] = road;
            }

            return adjacency;
        }

        /// <summary>
        /// Road�̎�ԃR�X�g�l�����߂�B
        /// </summary>
        private float GetCostG(int start, int end)
        {
            Road road = adjacency[start, end];

            return 1f;
        }

        /// <summary>
        /// �z��̒��̃C���f�b�N�X�����߂�
        /// </summary>
        private int FindIndex(Node node, Node[] array)
        {
            //���`�T��
            for(int cnt = 0; cnt < array.Length; cnt++)
            {
                if (node == array[cnt])
                {
                    return cnt;
                }
            }

            //������Ȃ�����
            return -1;
        }

        /// <summary>
        /// �z��̒��̃C���f�b�N�X�����߂�
        /// </summary>
        private int FindIndex(RoadJoint node, Node[] array)
        {
            //���`�T��
            for (int cnt = 0; cnt < array.Length; cnt++)
            {
                if (node == array[cnt].roadJoint)
                {
                    return cnt;
                }
            }

            //������Ȃ�����
            return -1;
        }

        /// <summary>
        /// HashSet����ŏ��̗v�f��Ԃ�
        /// </summary>
        private int GetMinOfHash(HashSet<int> hash)
        {
            int min = int.MaxValue;

            foreach(int column in hash)
            {
                if (min > column)
                {
                    min = column;
                }
            }

            return min;
        }

        /// <summary>
        /// ���H��Node���X�g��Road�̃��X�g�ɕϊ�
        /// </summary>
        /// <param name="nodeRoute"></param>
        /// <returns></returns>
        private List<Road> ConvertToRoadRoute(List<int> nodeRoute)
        {
            List<Road> output = new List<Road>();

            //�X�^�b�N���Ȃ��Ȃ�܂�
            //�Ō�̈�͏I�_�Ȃ̂ŏ����ɗv��Ȃ�
            for(int cnt = 0; cnt < nodeRoute.Count-1; cnt++)
            {
                //�n�_
                int startIndex = nodeRoute[cnt];
                //�I�_
                int endIndex = nodeRoute[cnt + 1];

                //�q���铹
                Road road = adjacency[startIndex, endIndex];

                //�o�^
                output.Add(road);
            }

            return output;
        }

        /// <summary>
        /// �����R�X�g���v�Z
        /// </summary>
        /// <param name="g">��ԃR�X�g</param>
        /// <param name="h">�����R�X�g</param>
        private static float CalculateF(float g, float h)
        {
            return g + h;
        }

        /// <summary>
        /// node������������
        /// </summary>
        private void InitializeNodes(Node[] nodes)
        {
            foreach(Node node in nodes)
            {
                node.Initialize();
            }
        }
    }
}