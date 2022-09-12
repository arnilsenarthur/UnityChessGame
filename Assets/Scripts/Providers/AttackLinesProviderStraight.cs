using System.Collections.Generic;
using UnityEngine;

namespace Game.Providers
{
    public class AttackLinesProviderStraight : AttackLinesProvider
    {
        public void GetAttackLines(int f, int r, int tf, int tr, BoardState state, List<AttackLine> list)
        {
            if(f == tf || r == tr) //On the same line
            {
                Piece attacker = state[f, r];

                AttackLine line = new AttackLine();
                line.to = new Vector2Int(tf, tr);
                line.from = new Vector2Int(f, r);
  
                Vector2Int dir = (line.to - line.from);
                
                if(dir.x == 0 && dir.y == 0)
                    return;

                dir /= Mathf.Abs(dir.x + dir.y);
               
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