using UnityEngine;
using System.Collections.Generic;

namespace Game.Controllers
{
    public class DefaultPlayerController : PlayerController
    {
        public void PlayerTurn(bool isWhite)
        {
            if(Input.GetMouseButtonDown(0) && !GameController.instance.waiting)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if(Physics.Raycast(ray, out RaycastHit hit, 20f))
                {
                    Vector3 position = hit.point;
                    position.y = 0;

                    Vector2Int tile = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));

                    if(GameController.instance.selectedPiece.y != -1)
                    {
                        //Move
                        foreach(Move move in GameController.instance.selectedPieceMoves)
                        {
                            if(move.to.x == tile.x && move.to.z == tile.y)
                            {
                                //Do move
                                GameController.instance.DoMove(move);

                                break;
                            }
                        }

                        GameController.instance.PlaySound(GameController.instance.soundOnClick);
                        GameController.instance.selectedPiece = new Vector2Int(-1, -1);
                        GameController.instance.tileRenderer.ClearSelectedTile();
                        return;
                    }

                    if(!GameController.instance.state.IsOnBounds(tile.x, tile.y))
                    {
                        GameController.instance.selectedPiece = new Vector2Int(-1, -1);
                        GameController.instance.tileRenderer.ClearSelectedTile();
                        return;
                    }
                        
                    Piece piece = GameController.instance.state[tile.x, tile.y];

                    if(piece == null || piece.isWhite != (GameController.instance.state.currentPlayer == 'w'))
                        return;

                    GameController.instance.selectedPiece = tile;
                    GameController.instance.tileRenderer.SetSelectedTile(tile);
                    GameController.instance.PlaySound(GameController.instance.soundOnClick);

                    List<Move> moves = new List<Move>();
                    moves.AddRange(GameController.instance.state.GetMoves(tile.x, tile.y));
                    GameController.instance.tileRenderer.SetPossibleMoves(moves);
                    GameController.instance.selectedPieceMoves = moves;
                }
            }
        }
    }
}