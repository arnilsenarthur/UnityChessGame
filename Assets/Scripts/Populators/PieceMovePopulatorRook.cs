using System.Collections.Generic;

namespace Game.Populators
{
    public class PieceMovePopulatorRook : PieceMovePopulator
    {
        public IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            foreach(var m in state.GetMoves(piece, 1, 0, 'R', state)) yield return m;
            foreach(var m in state.GetMoves(piece, -1, 0, 'R', state)) yield return m;
            foreach(var m in state.GetMoves(piece, 0, 1, 'R', state)) yield return m;
            foreach(var m in state.GetMoves(piece, 0, -1, 'R', state)) yield return m;
        }
    }
    
    public class PieceMovePopulatorRookCastler : PieceMovePopulator
    {
        public IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            foreach(var m in state.GetMoves(piece, 1, 0, 'R', state)) yield return m;
            foreach(var m in state.GetMoves(piece, -1, 0, 'R', state)) yield return m;
            foreach(var m in state.GetMoves(piece, 0, 1, 'R', state)) yield return m;
            foreach(var m in state.GetMoves(piece, 0, -1, 'R', state)) yield return m;
        }
    }  
}