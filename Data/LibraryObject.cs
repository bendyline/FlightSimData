/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.Collections.Generic;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class LibraryObject : Record
    {
        private List<Vertex> vertices;
        private Guid libraryObjectId;

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
                return RecordType.LibraryObjectA2GE;
            }

            set
            {
                base.RecordType = value;
            }
        }
        
        public Guid LibraryObjectId
        {
            get
            {
                return this.libraryObjectId;
            }

            set
            {
                this.libraryObjectId = value;
            }
        }

        public override int NodeSize
        {
            get
            {
                return 16 + (this.vertices.Count * 8);
            }
        }

        public LibraryObject()
        {
            this.vertices = new List<Vertex>();
        }


        public override void Load(Stream fs, int sectionSize)
        {
            this.libraryObjectId = ReadGuid(fs);

            int numVertices = (sectionSize - 16) / 8;

            for (int i=0; i< numVertices; i++)
            {
                Vertex v = new Vertex();

                v.X = ReadFloat(fs);
                v.Y = ReadFloat(fs);

                this.vertices.Add(v);
            }
        }


        public override void Save(Stream s)
        {
            base.Save(s);

            this.WriteGuid(s, this.libraryObjectId);

            if (this.vertices.Count > 0)
            {
                foreach (Vertex v in this.vertices)
                {
                    this.WriteFloat(s, v.X);
                    this.WriteFloat(s, v.Y);
                }

            }
        }
    }
}
