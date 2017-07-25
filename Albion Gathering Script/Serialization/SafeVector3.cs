using Ennui.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ennui.Script.Official
{
    public class SafeVector3
    {
        public float X;
        public float Y;
        public float Z;

        public SafeVector3() { }

        public SafeVector3(Vector3<float> vec)
        {
            this.X = vec.X;
            this.Y = vec.Y;
            this.Z = vec.Z;
        }

        public Vector3f RealVector3()
        {
            return new Vector3f(X, Y, Z);
        }
    }
}
