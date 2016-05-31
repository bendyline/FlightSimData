/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.Collections.Generic;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class LibraryObject : Record
    {
        private Vertex firstPoint;
        private Vertex secondPoint;
        private Vertex thirdPoint;

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

        public Vertex ThirdPoint
        {
            get
            {
                return this.thirdPoint;
            }

            set
            {
                this.thirdPoint = value;
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

                if (this.vertices.Count > 0)
                {
                    return 16 + (this.vertices.Count * 8);
                }

                // GUID = 16 bytes, two vertices where each vertex is 8 bytes.
                int size = 32;

                if (this.thirdPoint != null)
                {
                    size += 8;
                }

                return size;
            }
        }

        public LibraryObject()
        {
            this.firstPoint = new Vertex(-0.1f, -0.1f);
            this.secondPoint = new Vertex(0.1f, -0.1f);

            this.vertices = new List<Vertex>();
        }


        public override void Load(Stream fs, int sectionSize)
        {
            this.libraryObjectId = ReadGuid(fs);

            if (sectionSize == 32 || sectionSize == 40)
            {
                this.firstPoint = new Vertex();

                this.firstPoint.X = ReadFloat(fs);
                this.firstPoint.Y = ReadFloat(fs);

                this.secondPoint = new Vertex();

                this.secondPoint.X = ReadFloat(fs);
                this.secondPoint.Y = ReadFloat(fs);

                if (sectionSize == 40)
                {

                    this.thirdPoint = new Vertex();

                    this.thirdPoint.X = ReadFloat(fs);
                    this.thirdPoint.Y = ReadFloat(fs);

                }
            }
            else
            {
                int numVertices = (sectionSize - 16) / 8;

                for (int i=0; i< numVertices; i++)
                {
                    Vertex v = new Vertex();

                    v.X = ReadFloat(fs);
                    v.Y = ReadFloat(fs);

                    this.vertices.Add(v);
                }
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
            else
            {
                this.WriteFloat(s, this.firstPoint.X);
                this.WriteFloat(s, this.firstPoint.Y);

                this.WriteFloat(s, this.secondPoint.X);
                this.WriteFloat(s, this.secondPoint.Y);

                if (this.thirdPoint != null)
                {
                    this.WriteFloat(s, this.thirdPoint.X);
                    this.WriteFloat(s, this.thirdPoint.Y);
                }
            }
        }
    }
}
