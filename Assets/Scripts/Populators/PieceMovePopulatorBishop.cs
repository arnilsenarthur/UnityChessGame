using System.Collections.Generic;

namespace Game.Populators
{
    public class PieceMovePopulatorBishop : PieceMovePopulator
    {
        public IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            foreach(var m in state.GetMoves(piece, 1, 1, 'B', state)) yield return m;
            foreach(var m in state.GetMoves(piece, -1, 1, 'B', state)) yield return m;
            foreach(var m in state.GetMoves(piece, 1, -1, 'B', state)) yield return m;
            foreach(var m in state.GetMoves(piece, -1, -1, 'B', state)) yield return m;
        }
    }
}