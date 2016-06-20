/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using System;

namespace Bendyline.FlightSimulator.Data
{
    public class Vertex
    {
        private float x;
        private float y;

        public Vertex()
        {

        }

        public Vertex(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float X
        {
            get
            {
                return this.x;
            }

            set
            {
                this.x = value;
            }
        }

        public float Y
        {
            get
            {
                return this.y;
            }

            set
            {
                this.y = value;
            }
        }

        public bool IsClose(object obj, double tolerance)
        {
            if (obj is Vertex)
            {
                return (  Math.Abs(this.X - ((Vertex)obj).X) < tolerance &&  Math.Abs( this.Y - ((Vertex)obj).Y) < tolerance);
            }

            return base.Equals(obj);
        }
    }
}
