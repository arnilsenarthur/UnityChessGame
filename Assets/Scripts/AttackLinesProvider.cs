using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Main attack line provider interface
    /// </summary>
    public interface AttackLinesProvider
    {
        void GetAttackLines(int f, int r, int tf, int tr, BoardState state, List<AttackLine> list) { }
    }   
}