using System.Collections.Generic;

namespace Game.Populators
{
    public class PieceMovePopulatorKing : PieceMovePopulator
    {
        public bool IsSquareSafe(BoardState state, int f, int r, bool white)
        {
            foreach(var line in state.GetAttackLines(f, r, !white))
                if(line.isDirect) return false;

            return true;
        }

        public virtual IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            if(state.IsMovable(piece, f + 1, r + 1, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f + 1, r + 1).Capturing(!state.IsEmpty(f + 1, r + 1, state)).Build();                
        
            if(state.IsMovable(piece, f - 1, r - 1, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f - 1, r - 1).Capturing(!state.IsEmpty(f - 1, r - 1, state)).Build(); 

            if(state.IsMovable(piece, f + 1, r - 1, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f + 1, r - 1).Capturing(!state.IsEmpty(f + 1, r - 1, state)).Build(); 

            if(state.IsMovable(piece, f - 1, r + 1, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f - 1, r + 1).Capturing(!state.IsEmpty(f - 1, r + 1, state)).Build(); 
        
            if(state.IsMovable(piece, f + 1, r, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f + 1, r).Capturing(!state.IsEmpty(f + 1, r, state)).Build(); 

            if(state.IsMovable(piece, f - 1, r, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f - 1, r).Capturing(!state.IsEmpty(f - 1, r, state)).Build(); 

            if(state.IsMovable(piece, f, r + 1, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f, r + 1).Capturing(!state.IsEmpty(f, r + 1, state)).Build(); 

            if(state.IsMovable(piece, f, r - 1, state))
                yield return new MoveBuilder().Piece('K').From(f, r).To(f, r - 1).Capturing(!state.IsEmpty(f, r - 1, state)).Build(); 
        }
    }
    
    public class PieceMovePopulatorKingCastler : PieceMovePopulatorKing
    {
        public override IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            #region Castling
            if(f == 4 && (piece.isWhite ? r == 0 : r == 7))
            {
                Piece rookL = state[0, r];
                Piece rookR = state[7, r];

                if(rookL != null && (rookL.populator is PieceMovePopulatorRookCastler) && state.IsMovable(piece, 1, r, state) && state.IsMovable(piece, 2, r, state) && state.IsMovable(piece, 3, r, state))
                {
                    if(IsSquareSafe(state, 2, r, piece.isWhite) && IsSquareSafe(state, 3, r, piece.isWhite) && IsSquareSafe(state, 4, r, piece.isWhite))
                        yield return new MoveBuilder().Piece('K').From(f, r).To(2, r).Castling(true, true).Build();                
                }

                if(rookR != null && (rookR.populator is PieceMovePopulatorRookCastler) && state.IsMovable(piece, 5, r, state) && state.IsMovable(piece, 6, r, state))
                {
                    if(IsSquareSafe(state, 4, r, piece.isWhite) && IsSquareSafe(state, 5, r, piece.isWhite) && IsSquareSafe(state, 6, r, piece.isWhite))
                        yield return new MoveBuilder().Piece('K').From(f, r).To(6, r).Castling(true, false).Build();                
                }
            }
            #endregion

            foreach(var v in base.PopulateMoves(piece, state))
                yield return v;
        }
    }
}