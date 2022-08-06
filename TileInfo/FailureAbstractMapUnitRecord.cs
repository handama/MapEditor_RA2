using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapEditor.TileInfo
{
    public class FailureAbstractMapUnitRecord
    {
        public int X;
        public int Y;
        public List<string> Name = new List<string>();

        public void AddFailureRecord(int x, int y, string name)
        {
            X = x;
            Y = y;
            Name.Add(name);
        }
        public bool IsTargetFailureRecord(int x, int y)
        {
            if (x == X && y == Y)
                return true;
            else
                return false;
        }
        public bool IsInFailureRecord(string name)
        {
            foreach(var n in Name)
            {
                if (n == name)
                    return true;
            }
            return false;
        }
    }
}
