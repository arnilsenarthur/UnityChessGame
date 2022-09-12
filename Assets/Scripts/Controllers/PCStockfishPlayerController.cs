using UnityEngine;

namespace Game.Controllers
{
    public class PCStockfishPlayerController : PlayerController
    {
        public void PlayerTurn(bool isWhite)
        {
            if(GameController.instance.waiting || GameController.instance.stockFishSearching)
                return;

            BoardStateBuffer buffer = GameController.instance.buffer;

            if(buffer.moveCount > 0)
            {
                GameController.instance.StartCoroutine(GameController.instance.StockFishSearch((s) => {
                    Debug.Log("Do: " + s);
                    GameController.instance.DoMove(s, null, true);
                }));
            }
        }
    }
}