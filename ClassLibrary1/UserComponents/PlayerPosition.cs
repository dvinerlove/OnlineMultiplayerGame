using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerLibrary.UserComponents
{
    public class PlayerPosition
    {
        public float x;
        public float y;
        public Vector2 GetVector()
        {
            return new Vector2(x, y);
        }
    }
}
