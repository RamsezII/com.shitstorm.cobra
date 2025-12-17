using System.Collections.Generic;

namespace _COBRA_
{
    public sealed class BoaList : List<BoaObject>
    {

        //----------------------------------------------------------------------------------------------------------

        BoaList()
        {
        }

        public BoaList(IEnumerable<BoaObject> values) : base(values)
        {
        }

        //----------------------------------------------------------------------------------------------------------

    }
}