using System;
/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class RectangularVegetationArea : Record
    {
        private Vertex firstPoint;
        private Vertex secondPoint;

        private Guid vegetationType;

        public override RecordType RecordType
        {
            get
            {
                return RecordType.RectangularVegetationAreaVGRE;
            }

            set
            {
                base.RecordType = value;
            }
        }

        public Vertex FirstPoint
        {
            get
            {
                return this.firstPoint;
            }

            set
            {
                this.firstPoint = value;
            }
        }

        public Vertex SecondPoint
        {
            get
            {
                return this.secondPoint;
            }

            set
            {
                this.secondPoint = value;
            }
        }
        
        public Guid VegetationType
        {
            get
            {
                return this.vegetationType;
            }

            set
            {
                this.vegetationType = value;
            }
        }

        public override int NodeSize
        {
            get
            {
                return 32;
            }
        }

        public RectangularVegetationArea()
        {
            this.firstPoint = new Vertex(-0.1f, -0.1f);
            this.secondPoint = new Vertex(0.1f, -0.1f);
        }


        public override void Load(Stream fs, int sectionSize)
        {
            this.vegetationType = ReadGuid(fs);

            this.firstPoint = new Vertex();

            this.firstPoint.X = ReadFloat(fs);
            this.firstPoint.Y = ReadFloat(fs);

            this.secondPoint = new Vertex();

            this.secondPoint.X = ReadFloat(fs);
            this.secondPoint.Y = ReadFloat(fs);
        }


        public override void Save(Stream s)
        {
            base.Save(s);

            this.WriteGuid(s, this.vegetationType);

            this.WriteFloat(s, this.firstPoint.X);
            this.WriteFloat(s, this.firstPoint.Y);

            this.WriteFloat(s, this.secondPoint.X);
            this.WriteFloat(s, this.secondPoint.Y);
        }
    }
}
