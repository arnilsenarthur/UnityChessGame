using UnityEngine;

namespace Game.Controllers
{
    public class AutoPlayerController : PlayerController
    {
        public void PlayerTurn(bool isWhite)
        {
            if(GameController.instance.waiting)
                return;

            if(GameController.instance.autoPlay || (
                GameController.instance.moveAnimation ? Input.GetKey(KeyCode.Space) : Input.GetKeyDown(KeyCode.Space)))
            {
                //Do next auto move
                if(GameController.instance.autoPlayerNextMoves.Count > 0)
                {
                    GameController.instance.DoMove(GameController.instance.autoPlayerNextMoves[0]);
                    GameController.instance.autoPlayerNextMoves.RemoveAt(0);
                }
            }
        }
    }
}