using System;
using System.Text;

namespace Game
{ 
    [Serializable]
    public struct Move
    {
        public char piece;
        public Pos from;
        public Pos to;

        public bool isCapture;

        public bool isPromotion;
        public char promoteTo;

        public bool isCheck;
        public bool isCheckMate;

        public bool isCastling;
        public bool isCastlingQueenside;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            if(isCastling)
            {
                builder.Append("O-O");

                if(isCastlingQueenside)
                    builder.Append("-O");
            }
            else
            {            
                if(piece != '?' && piece != 'P')
                    builder.Append(piece);
                
                if(!from.isEmpty)
                    builder.Append(from);

                if(isCapture)
                    builder.Append('x');

                if(!to.isEmpty)
                    builder.Append(to);

                if(isPromotion)
                {
                    builder.Append('=');
                    builder.Append(promoteTo);
                }
            }

            if(isCheck)
                builder.Append(isCheckMate ? '#' : '+');  

            return builder.ToString();
        }

        public bool Matches(Move filter)
        {
            if(from.Matches(filter.from) && to.Matches(filter.to))
            {
                if(filter.isCastling)
                {
                    if(!isCastling) return false;
                    if(filter.isCastlingQueenside != isCastlingQueenside) return false;
                }
                else
                {
                    if(filter.piece != '?' && filter.piece != piece) return false;

                    if(filter.isCapture && !isCapture) return false;

                    if(filter.isPromotion)
                    {
                        if(!isPromotion) return false;
                        //if(filter.promoteTo != promoteTo) return false;
                    }
                }

                /*
                if(filter.isCheck)
                {
                    if(!isCheck) return false;
                    if(filter.isCheckMate != isCheckMate) return false;
                }
                */
                
                return true;
            }

            return false;
        }
    }  
}