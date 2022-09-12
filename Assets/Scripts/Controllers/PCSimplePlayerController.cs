using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Controllers
{
    public class MoveValuation
    {
        public int value;
        public Move move;
    }

    public class PCSimplePlayerController : PlayerController
    {
        public int GetPieceValue(Piece piece)
        {
            if(piece.type == 'q' || piece.type == 'Q')
                return 1000;

            if(piece.type == 'r' || piece.type == 'R')
                return 500;

            if(piece.type == 'b' || piece.type == 'B')
                return 400;

            if(piece.type == 'n' || piece.type == 'N')
                return 500;

            if(piece.type == 'p')
                return 700 - piece.position.y * 100;

            if(piece.type == 'P')
                return piece.position.y * 100;

            return 1;
        }

        public Move FindBestMove(BoardStateBuffer buffer)
        {
            List<MoveValuation> moves = new List<MoveValuation>();
            BoardState state = GameController.instance.state;

            foreach(var v in buffer.moves)
            {
                foreach(var m in v.Value)
                {
                    int value = 0;
                    
                    if(m.isCapture)
                        value += GetPieceValue(state[m.to.x, m.to.z]);

                    if(m.isCheck)
                        value += 100;

                    if(m.isCheckMate)
                        value += 10_000;

                    if(m.isPromotion)
                        value += 200; 

                    Piece piece = state[m.from.x, m.from.z];

                    if(piece.type == 'p' || piece.type == 'P')
                    {
                        if(piece.isWhite)
                        {
                            if(m.to.z > m.from.z)
                                value += 10;
                        }
                        else
                            if(m.to.z < m.from.z)
                                value += 10;
                    }

                    moves.Add(new MoveValuation{move = m, value = value});
                }
            }

            moves.Sort((a,b) => -a.value.CompareTo(b.value));

            int max = moves[0].value;
            MoveValuation[] allMax = moves.Where((a) => a.value == max).ToArray();
            return moves[Random.Range(0, allMax.Length)].move;
        }

        public void PlayerTurn(bool isWhite)
        {
            if(GameController.instance.waiting || GameController.instance.stockFishSearching)
                return;

            BoardStateBuffer buffer = GameController.instance.buffer;

            if(buffer.moveCount > 0)
            {
                Move move = FindBestMove(buffer);

                if(move.isPromotion)
                        move.promoteTo = new char[]{'Q','B','R','N'}[Random.Range(0, 3)];

                GameController.instance.DoMove(move);
            }
        }
    }
}