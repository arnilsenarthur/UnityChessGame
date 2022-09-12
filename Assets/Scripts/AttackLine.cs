using UnityEngine;

namespace Game
{
    /// <summary>
    /// Represents a piece attack line (used to calculate if a square is safe)
    /// </summary>
    public class AttackLine
    {
        public Vector2Int from;
        public Vector2Int to;

        public bool isDirect => defender == null;
        public Piece defender;

        public bool CanDefend(int f,int r)
        {
            Vector2Int point = new Vector2Int(f, r);

            //Capturing the source
            if(point == from)
                return true;

            //On the middle
            Vector2 dirF = point - from;
            Vector2 dirT = point - to;

            return dirF.normalized == -dirT.normalized; 
        }
    }
}