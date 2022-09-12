using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{   
    [Serializable]
    public class Piece
    {
        public Vector2Int position;
        public PieceMovePopulator populator;
        public Action<int, int, int, int, BoardState, List<AttackLine>> attackLineProviders;

        public char type = '?';

        public bool isWhite => type < 96;
        public bool isBlack => type > 96;

        public GameObject gameObject;
        
        public void Load()
        {
            populator = GameController.instance.GetPopulator(this);
            attackLineProviders = null;
            GameController.instance.LoadProviders(this);
        }

        public void SetType(char type)
        {
            this.type = type;

            GameObject go = GameController.Instantiate(GameController.instance.GetPrefab(type));
            go.transform.position = gameObject.transform.position;
            GameController.Destroy(gameObject);
            gameObject = go;

            this.type = type;
            Load();
        }
    }
}