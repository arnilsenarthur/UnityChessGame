namespace Game
{
    public class MoveBuilder
    {
        private Move _move;

        public MoveBuilder()
        {
            _move = new Move();
        }

        public MoveBuilder Piece(char type)
        {
            _move.piece = type;
            return this;
        }

        public MoveBuilder From(Pos pos)
        {
            _move.from = pos;
            return this;
        }

        public MoveBuilder From(int f,int r)
        {
            _move.from = new Pos(f, r);
            return this;
        }

        public MoveBuilder To(Pos pos)
        {
            _move.to = pos;
            return this;
        }

        public MoveBuilder To(int f,int r)
        {
            _move.to = new Pos(f, r);
            return this;
        }

        public MoveBuilder Capturing(bool value = true)
        {
            _move.isCapture = value;
            return this;
        }

        public MoveBuilder Promoting(bool value = true)
        {
            _move.isPromotion = value;
            return this;
        }

        public MoveBuilder Castling(bool value = true, bool queenSide = false)
        {
            _move.isCastling = value;
            _move.isCastlingQueenside = queenSide;
            return this;
        }

        public Move Build()
        {
            return _move;
        }
    }   
}