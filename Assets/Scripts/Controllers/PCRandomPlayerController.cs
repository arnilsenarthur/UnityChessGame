using UnityEngine;

namespace Game.Controllers
{
    public class PCRandomPlayerController : PlayerController
    {
        public void PlayerTurn(bool isWhite)
        {
            if(GameController.instance.waiting || GameController.instance.stockFishSearching)
                return;

            BoardStateBuffer buffer = GameController.instance.buffer;

            if(buffer.moveCount > 0)
            {  
                int rnd = Random.Range(0, buffer.moveCount);

                int i = 0;
                foreach(var v in buffer.moves)
                {
                    if(rnd < i + v.Value.Count)
                    {
                        Move move = v.Value[rnd - i];

                        //Choose target promotion
                        if(move.isPromotion)
                            move.promoteTo = new char[]{'Q','B','R','N'}[Random.Range(0, 3)];

                        GameController.instance.DoMove(move);
                        break;
                    }
                    else
                        i += v.Value.Count;
                }
            }
        }
    }
}