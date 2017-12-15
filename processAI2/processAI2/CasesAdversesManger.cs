using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace processAI2
{
    class CasesAdversesManger
    {


        private int positionEnnemie;
        private int positionAlliee;
        private int poidsAlliee;

        public CasesAdversesManger(int positionAlliee, int positionEnnemie, int poidsAlliee)
        {
            this.positionEnnemie = positionEnnemie;
            this.positionAlliee = positionAlliee;
            this.poidsAlliee = poidsAlliee;
        }

        public int CoordEnnemies1 { get => positionEnnemie; set => positionEnnemie = value; }
        public int CoordAlliee1 { get => positionAlliee; set => positionAlliee = value; }
        public int PoidsAlliee { get => poidsAlliee; set => poidsAlliee = value; }
    }
    public static class TupleListExtensions
    {
        public static void Add<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> list,
                T1 item1, T2 item2, T3 item3)
        {
            list.Add(Tuple.Create(item1, item2, item3));
        }
        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list,
                T1 item1, T2 item2)
        {
            list.Add(Tuple.Create(item1, item2));
        }
    }
}
