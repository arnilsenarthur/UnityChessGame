using System.Collections.Generic;
using UnityEngine;

namespace Game.Providers
{
    public class AttackLinesProviderPawn : AttackLinesProvider
    {
        public void GetAttackLines(int f, int r, int tf, int tr, BoardState state, List<AttackLine> list)
        {
            Piece attacker = state[f, r];

            int dir = attacker.isWhite ? 1 : -1;

            if(tr == r + dir)
            {
                if(tf == f - 1 || tf == f + 1)
                {
                    AttackLine line = new AttackLine();
                    line.to = new Vector2Int(tf, tr);
                    line.from = new Vector2Int(f, r);
                    list.Add(line);
                }
            }
        }
    }
}