using System.Collections.Generic;
using UnityEngine;

namespace Game.Providers
{
    public class AttackLinesProviderKing : AttackLinesProvider
    {
        public void GetAttackLines(int f, int r, int tf, int tr, BoardState state, List<AttackLine> list)
        {
            AttackLine line = new AttackLine();
            line.to = new Vector2Int(tf, tr);
            line.from = new Vector2Int(f, r);

            Vector2Int dir = line.to - line.from;

            if(Mathf.Abs(dir.x) <= 1 && Mathf.Abs(dir.y) <= 1)
                list.Add(line);
        }
    }
}