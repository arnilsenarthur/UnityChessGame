using System.Collections.Generic;

namespace Game
{
    public interface PieceMovePopulator
    {
        IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            yield break;
        }    
    }
}