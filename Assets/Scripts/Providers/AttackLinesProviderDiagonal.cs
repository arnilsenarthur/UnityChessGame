using System.Collections.Generic;
using UnityEngine;

namespace Game.Providers
{
    public class AttackLinesProviderDiagonal : AttackLinesProvider
    {
        public void GetAttackLines(int f, int r, int tf, int tr, BoardState state, List<AttackLine> list)
        {
            AttackLine line = new AttackLine();
            line.to = new Vector2Int(tf, tr);
            line.from = new Vector2Int(f, r);

            Vector2Int dir = (line.to - line.from);

            if(Mathf.Abs(dir.x) == Mathf.Abs(dir.y) && dir.x != 0)
            {
                dir /= Mathf.Abs(dir.x);

                Piece attacker = state[f, r];

                int defenderCount = 0;
                for(Vector2Int current = line.from + dir; current != line.to; current += dir)
                {
                    Piece defender = state[current.x, current.y];

                    if(defender != null)
                    {
                        if(defender.isBlack == attacker.isBlack || defenderCount > 0) return;

                        //Defender here
                        defenderCount ++;
                        line.defender = defender;
                    }
                } 

                list.Add(line);
            }
        }
    }
}