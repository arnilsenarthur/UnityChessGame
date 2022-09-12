using UnityEngine;
using System;
namespace Game
{
    [System.Serializable]
    public class StockfishSearch
    {
        public string fen = "";
        public int time = 1000;
        public Action<string> callback;
    }
}