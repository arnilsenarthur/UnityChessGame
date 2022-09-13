using System.Collections.Generic;

namespace Game.Populators
{
    public class PieceMovePopulatorPawn : PieceMovePopulator
    {
        public IEnumerable<Move> PopulateMoves(Piece piece, BoardState state)
        {
            int f = piece.position.x;
            int r = piece.position.y;

            if(piece.isWhite)
            {                
                if(state.IsOnBounds(f, r + 1) && state.IsEmpty(f, r + 1, state))
                {
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f, r + 1).Promoting(r == 6).Build();

                    if(r == 1)
                    {
                        if(state.IsOnBounds(f, r + 2) && state.IsEmpty(f, r + 2, state))
                        {
                            yield return new MoveBuilder().Piece('P').From(f, r).To(f, r + 2).Build();
                        }
                    }
                }

                if(state.IsMovable(piece,f + 1, r + 1, state) && state.enPassantPosition.x == f + 1 && state.enPassantPosition.y == r + 1)
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f + 1, r + 1).Capturing().Promoting(r == 6).Build();

                if(state.IsMovable(piece,f - 1, r + 1, state) && state.enPassantPosition.x == f - 1 && state.enPassantPosition.y == r + 1)
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f - 1, r + 1).Capturing().Promoting(r == 6).Build();

                if(state.IsMovable(piece,f + 1, r + 1, state) && !state.IsEmpty(f + 1, r + 1, state))
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f + 1, r + 1).Promoting(r == 6).Capturing().Build();

                if(state.IsMovable(piece,f - 1, r + 1, state) && !state.IsEmpty(f - 1, r + 1, state))
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f - 1, r + 1).Promoting(r == 6).Capturing().Build();

            }
            else
            {
                if(state.IsOnBounds(f, r - 1) && state.IsEmpty(f, r - 1, state))
                {
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f, r - 1).Promoting(r == 1).Build();

                    if(r == 6)
                    {
                        if(state.IsOnBounds(f, r - 2) && state.IsEmpty(f, r - 2, state))
                        {
                            yield return new MoveBuilder().Piece('P').From(f, r).To(f, r - 2).Build();
                        }
                    }
                } 

                if(state.IsMovable(piece,f + 1, r - 1, state) && state.enPassantPosition.x == f + 1 && state.enPassantPosition.y == r - 1)
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f + 1, r - 1).Capturing().Promoting(r == 1).Build();

                if(state.IsMovable(piece,f - 1, r - 1, state) && state.enPassantPosition.x == f - 1 && state.enPassantPosition.y == r - 1)
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f - 1, r - 1).Capturing().Promoting(r == 1).Build();   

                if(state.IsMovable(piece,f + 1, r - 1, state) && !state.IsEmpty(f + 1, r - 1, state))
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f + 1, r - 1).Promoting(r == 1).Capturing().Build();

                if(state.IsMovable(piece,f - 1, r - 1, state) && !state.IsEmpty(f - 1, r - 1, state))
                    yield return new MoveBuilder().Piece('P').From(f, r).To(f - 1, r - 1).Promoting(r == 1).Capturing().Build();   
            }

            yield break;
        }
    }
}