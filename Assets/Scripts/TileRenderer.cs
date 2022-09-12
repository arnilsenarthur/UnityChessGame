using UnityEngine;
using System.Collections.Generic;

namespace Game
{
    public class TileRenderer : MonoBehaviour
    {
        public GameObject selectedTilePrefab;
        public GameObject moveTilePrefab;

        [SerializeField]
        private GameObject _selectedTile;

        [SerializeField]
        private List<GameObject> _possibleMoveTiles;

        public void ClearSelectedTile()
        {
            if(_selectedTile == null) return;

            Destroy(_selectedTile);
            _selectedTile = null;

            ClearPossibleMoves();
        }

        public void SetSelectedTile(Vector2Int tile)
        {
            if(_selectedTile == null)
            {
                _selectedTile = Instantiate(selectedTilePrefab);
            } 

            _selectedTile.transform.position = new Vector3(tile.x, 0.05f, tile.y);
        }

        public void ClearPossibleMoves()
        {
            foreach(var go in _possibleMoveTiles)
                Destroy(go);

            _possibleMoveTiles.Clear();
        }

        public void SetPossibleMoves(List<Move> moves)
        {
            ClearPossibleMoves();

            foreach(Move move in moves)
            {
                GameObject go = Instantiate(moveTilePrefab);
                go.transform.position = new Vector3(move.to.x, 0.05f, move.to.z);
                _possibleMoveTiles.Add(go);
            }    
        }
    }
}