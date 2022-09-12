using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Board situation buffer
    /// </summary>
    [Serializable]
    public class BoardStateBuffer
    {
        public List<AttackLine> currentAttackLines = new List<AttackLine>();
        public Dictionary<Vector2Int, List<Move>> moves = new Dictionary<Vector2Int, List<Move>>();
        public int moveCount = 0;

        public bool inCheck = false;
        public bool inCheckMate = false;
        public bool inStaleMate = false;

        public override string ToString()
        {
            return $"Moves: {moveCount} {(inCheck ? (inCheckMate ? "MATE" : "CHECK") : (inStaleMate ? "STALE" : ""))}";
        }
    }
}