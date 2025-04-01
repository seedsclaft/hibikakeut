
using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class HexRoute
    {
        private HexMode _hexMode = HexMode.Reach;
        private MoveType _moveType = MoveType.None;
        private List<HexField> _fields = new();
        private HexField GetField(int x,int y) => _fields.Find(a => a.X == x && a.Y == y);
        private List<HexField> _openList = new();
        private List<HexField> _closedList = new();
        private int width;
        private int height;
        private List<HexUnitInfo> _hexUnitInfos;
        private int _goalColX = 0;
        private int _goalRowY = 0;
        private int _moveCost = 0;

        public HexRoute(int mapX,int mapY,List<HexUnitInfo> hexUnitInfos)
        {
            width = mapX;
            height = mapY;
            MakeFields(mapX,mapY);
            _hexUnitInfos = hexUnitInfos;
            UpdateIsUnit();
        }

        public void MakeFields(int mapX,int mapY)
        {
            _fields.Clear();
            for (int i = 0;i < mapX;i++)
            {   
                for (int j = 0;j < mapY;j++)
                {
                    var field = new HexField
                    {
                        X = i,
                        Y = j
                    };
                    _fields.Add(field);
                }
            }
        }

        public void ResetAll()
        {
            _openList.Clear();
            _closedList.Clear();
        }

        public void UpdateIsUnit()
        {
            foreach (var field in _fields)
            {
                var find = _hexUnitInfos.Find(a => a.HexField.X == field.X && a.HexField.Y == field.Y);
                field.IsUnit = find != null && find.IsUnit;
            }
        }

        public int CalcHeuristic(int colX, int rowY) 
        {
            // モードは到着探索
            if (_hexMode == HexMode.Reach) 
            {
                return 0; // ヒューリスティック計算は常に０を返す
            }
            var distX = Math.Abs(_goalColX - colX);
            var distY = Math.Abs(_goalRowY - rowY);
            if (distX > distY) 
            {
                return distX;
            }
            return distY;
        }

        public bool CheckIsNodeValid(int colX,int rowY,HexField parent) 
        {
            // フィールド外チェック
            if (colX < 0 || rowY < 0) 
            {
                return false;
            }
            if (colX >= width || rowY >= height) 
            {
                return false;
            }

            // 未オープンチェック（同時に壁チェックもしている）
            var field = GetField(colX,rowY);
            if (field.Stat != HexStat.None) 
            {
                return false;
            }

                /*
                // ユニット位置チェック
                if (this.Field[rowY][colX].IsUnit === true) {
                return false;
                }
                */

            // 地形と移動タイプから実コストを計算
            var aCost = parent.ACost + 1/*mapData[colX][rowY].Terrain.cost[_moveType]*/;
            if (aCost > 1/*moveCost*/) 
            { 
                // 移動コストより大きい場合、オープンしない
                return false;
            }

            return true; // 有効。OK！
        }

        public void OpenNode(int colX,int rowY,HexField parent) 
        {
            var node = GetField(colX,rowY);

            node.Stat = HexStat.Open; // ステータス オープン
            // 地形と移動タイプから実コストを計算
            node.ACost = parent.ACost + 1/*mapData[colX][rowY].Terrain.cost[_moveType]*/;

            node.HCost = CalcHeuristic(colX, rowY); // 推定コストを設定
            node.Score = node.ACost + node.HCost;
            node.Parent = parent; // 親ノードを設定

            // オープンリストに追加
            _openList.Add(node);
        }

        public void OpenStartNode(int colX,int rowY) 
        {
            var node = GetField(colX,rowY);

            node.Stat = HexStat.Open; // ステータス オープン
            // 地形と移動タイプから実コストを計算
            node.ACost = 0;

            node.HCost = CalcHeuristic(colX, rowY); // 推定コストを設定
            node.Score = node.ACost + node.HCost;
            node.Parent = null; // 親ノードはnull

            // オープンリストに追加
            _openList.Add(node);
        }

        public HexField TakeMinScoreNodeFromOpenList()
        {
            // 最小スコアの初期化
            var minScore = 9999999;
            // 最小実コストの初期化
            var minACost = 9999999;
            // 最小ノードのID(loopの値)
            int? minNodeID = null;

            var listMax = _openList.Count;
            HexField node;

            for (int loop = 0; loop < listMax; loop++) 
            {
                node = _openList[loop];
                if (node.Score > minScore) 
                {
                    // スコアが大きい
                    continue;
                }
                if (node.Score == minScore && node.ACost >= minACost) 
                {
                    // スコアが同じときは実コストも比較する
                    continue;
                }

                // 最小値更新.
                minScore = node.Score;
                minACost = node.ACost;
                minNodeID = loop;
            }


            var minNode = _openList[(int)minNodeID]; // 該当要素の参照を保存
            minNode.Stat = HexStat.Cloased; // ステータスをクローズドに変更
            //_openList.RemoveAt((int)minNodeID, 1); // this.OpenListから、該当要素を１つ削除
            _openList.RemoveAt((int)minNodeID); // this.OpenListから、該当要素を１つ削除
            _closedList.Add(minNode); // クローズリストに追加
            return minNode;
        }

        public bool IsGoal(HexField node)
        {
            var colX = node.X;
            var rowY = node.Y;

            // ゴールチェック
            if (colX == _goalColX && rowY == _goalRowY) 
            {
                return true;
            }
            return false;
        }

        public void OpenSurroundingNodes(HexField currentNode)
        {
            var colX = currentNode.X;
            var rowY = currentNode.Y;
            if (CheckIsNodeValid(colX + 1, rowY, currentNode)) 
            {
                OpenNode(colX + 1, rowY, currentNode);
            }

            // 偶数のときは、X座標−１と隣接している
            if (rowY % 2 == 0) 
            {
                if (CheckIsNodeValid(colX + 1, rowY - 1, currentNode)) 
                {
                    OpenNode(colX + 1, rowY - 1, currentNode);
                }
            }
            else 
            {
                if (CheckIsNodeValid(colX - 1, rowY + 1, currentNode)) 
                {
                    OpenNode(colX - 1, rowY + 1, currentNode);
                }
            }

            // 下のライン
            if (CheckIsNodeValid(colX - 1, rowY, currentNode)) 
            {
                OpenNode(colX - 1, rowY, currentNode);
            }

            // 偶数のときは、X座標−１と隣接している
            if (rowY % 2 == 0) 
            {
                if (CheckIsNodeValid(colX - 1, rowY - 1, currentNode)) 
                {
                    OpenNode(colX - 1, rowY - 1, currentNode);
                }
            } else 
            {
                if (CheckIsNodeValid(colX - 1, rowY - 1, currentNode)) 
                {
                    OpenNode(colX - 1, rowY - 1, currentNode);
                }
            }

            // 縦のライン
            if (CheckIsNodeValid(colX, rowY - 1, currentNode)) 
            {
                OpenNode(colX, rowY - 1, currentNode);
            }

            if (CheckIsNodeValid(colX, rowY + 1, currentNode)) 
            {
                OpenNode(colX, rowY + 1, currentNode);
            }
        }
        
        public HexPath GetPath(HexField node)
        {
            var pathlist = new List<HexPath>();

            var path = new HexPath();
            path.X = node.X;
            path.Y = node.Y;
            path.Obj = null;
            pathlist.Add(path);
            while (true) 
            {
                if (node.Parent.Parent == null) 
                { 
                    // スタート地点は含まない
                    return path;
                }
                node = node.Parent;
                path = new HexPath();
                path.X = node.X;
                path.Y = node.Y;
                path.Obj = null;
                pathlist.Add(path);
            }
        }

        public void RefreshField(int mapX,int mapY) 
        {
            // 最初に壁の探索
            HexStat stat;
            for (int row = 0; row < mapY; row++) 
            {
                for (int col = 0; col < mapX; col++) 
                {
                    stat = GetField(col,row).Stat;
                    // 壁 か 空白 だったら探索領域から外す
                    if (stat == HexStat.Open || stat == HexStat.Cloased) 
                    {
                         // すでにOPENやCLOSEDだったら
                        GetField(col,row).Stat = HexStat.None; // ステータスをNULLにする
                    }
                }
            }
            _openList.Clear();
            _closedList.Clear();
        }

        public HexPath FindRoute(MoveType moveType,HexField startHex,HexField goalHex) 
        {
            _hexMode = HexMode.Route; // ルート探索モードをセット

            _moveType = moveType;

            RefreshField(width,height); // リフレッシュする

            // ゴールの登録
            _goalColX = goalHex.X;
            _goalRowY = goalHex.Y;

            // 移動コスト 十分大きければOK
            _moveCost = 1000;

            // スタートのノードを追加
            OpenStartNode(startHex.X, startHex.Y);

            // 経路探索のループ;
            HexField currentNode = null;
            while (true) 
            {
                // リストが空 = 経路が見つけられなかった
                if (_openList.Count == 0) 
                {
                    return null;
                }

                currentNode = TakeMinScoreNodeFromOpenList(); // スコア最小ノードの取り出し (そしてクローズ)

                // ゴールしたかどうかのチェック
                if (IsGoal(currentNode)) 
                {
                    return GetPath(GetField(currentNode.X,currentNode.Y));
                }

                // 周囲のノードをオープンする
                OpenSurroundingNodes(currentNode);
            }
        }

        public List<HexField> GetReachableArea(MoveType moveType,HexField startHex,int moveCost) 
        {
            _hexMode = HexMode.Reach; // 到着探索モードをセット

            _moveType = moveType; // 移動タイプ
            _moveCost = moveCost; // 移動コスト

            RefreshField(width,height); // リフレッシュする

            // スタートのノードを追加
            OpenStartNode(startHex.X, startHex.Y);

            // 経路探索のループ;
            HexField currentNode = null;
            while (true) 
            {

                // リストが空 = 到達可能エリアの探索終了
                if (_openList.Count == 0) 
                {
                    return _closedList;
                }
                currentNode = TakeMinScoreNodeFromOpenList(); // スコア最小ノードの取り出し (そしてクローズ)

                OpenSurroundingNodes(currentNode);
            }
        }
    }

    public class HexField
    {
        public int X = -1;
        public int Y = -1;
        public HexStat Stat = HexStat.None;
        public bool IsUnit = false;
        public int ACost = 0;
        public int HCost = 0;
        public int Score = 0;
        public HexField Parent = null;
    }

    public class HexPath
    {
        public int X = -1;
        public int Y = -1;
        public HexField Obj = null;
    }

    /// <summary>
    /// 検索タイプ
    /// </summary>
    public enum HexMode
    {
        Route = 0,
        Reach = 1
    }

    /// <summary>
    /// 検索ステータス
    /// </summary>
    public enum HexStat
    {
        None = 0, // 未探索
        Open = 1, // オープン
        Cloased = 2, // クローズド
        Wall = 3, // 壁
    }

    /// <summary>
    /// 検索ステータス
    /// </summary>
    public enum MoveType
    {
        None = 0,
        Normal = 1, 
    }
}
