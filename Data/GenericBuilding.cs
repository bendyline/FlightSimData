/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class GenericBuilding : Record
    {
        private List<Vertex> vertices;
        private Guid roofType;

        public override RecordType RecordType
        {
            get
            {
                return RecordType.GenericBuildingGBLR;
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

        public Guid RoofType
        {
            get
            {
                return this.roofType;
            }

            set
            {
                this.roofType = value;
            }
        }

        public override int NodeSize
        {
            get
            {
                // GUID = 16 bytes, FLOAT for height = 4 bytes, INT for vertext count = 4 bytes, each vertex is 8 bytes.
                return 16 + (this.vertices.Count * 8);
            }
        }

        public GenericBuilding()
        {
            this.vertices = new List<Vertex>();
        }
        

        public override void Load(Stream fs, int sectionSize)
        {
            this.roofType = ReadGuid(fs);
            int vertexCount = (sectionSize - 16) / 8;

            Debug.Assert(vertexCount * 8 + 16 == sectionSize);

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

            this.WriteGuid(s, this.roofType);
            
            foreach (Vertex v in this.vertices)
            {
                this.WriteFloat(s, v.X);
                this.WriteFloat(s, v.Y);
            }
        }
    }
}
