using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame
{
    /// <summary>
    /// RoadとRoadをつなぐJoint
    /// 曲がり角にはこのRoadJoint
    /// 交差点、外界への接続口は子クラスを使用
    /// </summary>
    public class RoadJoint : MonoBehaviour
    {
        /// <summary>
        /// このJointに接続しているRoad
        /// </summary>
        public List<Road> connectedRoads { get; private set; } = new List<Road>();

        /// <summary>
        /// 各connectedRoadのどちらの端が繋がっているか
        /// </summary>
        public Dictionary<Road, int> edges { get; private set; } = new Dictionary<Road, int>();

        /// <summary>
        /// 繋がった道を登録
        /// </summary>
        /// <param name="edge">道路のどちらの端か（Road.Edgeの番号に対応）</param>
        public void RegisterRoad(Road road, int edge)
        {
            //道を登録
            connectedRoads.Add(road);

            //端を登録
            edges[road] = edge;
        }
    }
}