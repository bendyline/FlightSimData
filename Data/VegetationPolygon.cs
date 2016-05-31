/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.Collections.Generic;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class VegetationPolygon : Record
    {
        private List<Vertex> vertices;
        private Guid vegetationType;

        public override RecordType RecordType
        {
            get
            {
                return RecordType.VegetationPolygonPRDE;
            }

            set
            {
                base.RecordType = value;
            }
        }

        public List<Vertex> Vertices
        {
            get
            {
                return this.vertices;
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
                // GUID = 16 bytes, INT = 4 bytes, each vertex is 8 bytes.
                return 20 + this.vertices.Count * 8;
            }
        }

        public VegetationPolygon()
        {
            this.vertices = new List<Vertex>();
        }
        

        public override void Load(Stream fs, int sectionSize)
        {
            this.vegetationType = ReadGuid(fs);
            int vertexCount = ReadInt(fs);

            for (int i=0; i<vertexCount; i++)
            {
                Vertex vt = new Vertex();

                vt.X = ReadFloat(fs);
                vt.Y = ReadFloat(fs);

                this.vertices.Add(vt);
            }
        }


        public override void Save(Stream s)
        {
            base.Save(s);

            this.WriteGuid(s, this.vegetationType);
            this.WriteInt(s, this.vertices.Count);

            foreach (Vertex v in this.vertices)
            {
                this.WriteFloat(s, v.X);
                this.WriteFloat(s, v.Y);

            }
        }
    }
}
