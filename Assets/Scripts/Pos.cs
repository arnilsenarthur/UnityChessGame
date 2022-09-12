using System;

namespace Game
{   
    [Serializable]
    public struct Pos
    {
        public static Pos EMPTY => new Pos{rank = '?', file = '?'};

        public char file;
        public char rank;

        public int x => file - 97;
        public int z => rank - 49;

        public Pos(int file, int rank)
        {
            this.file = (char) (file + 97);
            this.rank = (char) (rank + 49);
        }

        public override string ToString()
        {
            if(file == '?' && rank == '?')
                return String.Empty;

            if(file == '?')
                return $"{rank}";

            if(rank == '?')
                return $"{file}";

            return $"{file}{rank}";
        }

        public bool Matches(Pos filter)
        {
            return (filter.rank == '?' || filter.rank == rank) && (filter.file == '?' || filter.file == file);
        }

        public bool isEmpty => (file == rank) && file == '?';
    }
}