/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class RowHouse : Record
    {
        private List<Vertex> vertices;

        public List<Vertex> Vertices
        {
            get
            {
                return this.vertices;
            }
        }

        public override RecordType RecordType
        {
            get
            {
                return RecordType.RowHouseROWH;
            }

            set
            {
                base.RecordType = value;
            }
        }


        public override int NodeSize
        {
            get
            {
                return this.vertices.Count * 8;
            }
        }

        public RowHouse()
        {
            this.vertices = new List<Vertex>();
        }

        public override void Load(Stream fs, int sectionSize)
        {
            int vertexCount = (sectionSize) / 8;

            Debug.Assert(vertexCount * 8 == sectionSize);

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
            
            foreach (Vertex v in this.vertices)
            {
                this.WriteFloat(s, v.X);
                this.WriteFloat(s, v.Y);
            }
        }
    }
}
