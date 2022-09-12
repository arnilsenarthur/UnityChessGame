using System.Collections.Generic;

namespace Game.Populators
{
    public class PieceMovePopulatorKnight : PieceMovePopulator
    {
        public IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            if(state.IsMovable(piece, f + 2, r + 1, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f + 2, r + 1).Capturing(!state.IsEmpty(f + 2, r + 1, state)).Build();
        
            if(state.IsMovable(piece, f + 2, r - 1, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f + 2, r - 1).Capturing(!state.IsEmpty(f + 2, r - 1, state)).Build();

            if(state.IsMovable(piece, f - 2, r + 1, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f - 2, r + 1).Capturing(!state.IsEmpty(f - 2, r + 1, state)).Build();

            if(state.IsMovable(piece, f - 2, r - 1, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f - 2, r - 1).Capturing(!state.IsEmpty(f - 2, r - 1, state)).Build();

            if(state.IsMovable(piece, f + 1, r + 2, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f + 1, r + 2).Capturing(!state.IsEmpty(f + 1, r + 2, state)).Build();

            if(state.IsMovable(piece, f + 1, r - 2, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f + 1, r - 2).Capturing(!state.IsEmpty(f + 1, r - 2, state)).Build();

            if(state.IsMovable(piece, f - 1, r + 2, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f - 1, r + 2).Capturing(!state.IsEmpty(f - 1, r + 2, state)).Build();

            if(state.IsMovable(piece, f - 1, r - 2, state))
                yield return new MoveBuilder().Piece('N').From(f, r).To(f - 1, r - 2).Capturing(!state.IsEmpty(f - 1, r - 2, state)).Build();
        }
    }
}