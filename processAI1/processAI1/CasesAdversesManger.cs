using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace processAI1
{
    class CasesAdversesManger
    {
       

            private int positionEnnemie;
            private int positionAlliee;
            private double poidsAlliee;
            private double poidsEnnemie;

        public CasesAdversesManger(int positionAlliee, int positionEnnemie)
        {
            this.positionEnnemie = positionEnnemie;
            this.positionAlliee = positionAlliee;
           // this.poidsAlliee = poidsAlliee;
            //this.poidsEnnemie = poidsEnnemie;
        }

        public int CoordEnnemies1 { get => positionEnnemie; set => positionEnnemie= value; }
        public int CoordAlliee1 { get => positionAlliee; set => positionAlliee = value; }
        public double PoidsAlliee { get => poidsAlliee; set => poidsAlliee = value; }
        public double PoidsEnnemie { get => poidsEnnemie; set => poidsEnnemie = value; }
    }
    public static class TupleListExtensions
    {
        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list,
                T1 item1, T2 item2)
        {
            list.Add(Tuple.Create(item1, item2));
        }
    }
}
