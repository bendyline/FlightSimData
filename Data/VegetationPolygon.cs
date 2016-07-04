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

        private List<Vertex> allNorthernVertices;
        private List<Vertex> allWesternVertices;
        private List<Vertex> westernVertices;
        private List<Vertex> easternVertices;
        private List<Vertex> northernVertices;
        private List<Vertex> southernVertices;

        public List<Vertex> WesternVertices
        {
            get
            {
                if (this.westernVertices == null)
                {
                    this.westernVertices = Vertex.GetWesternVertices(this.Vertices);
                }

                return this.westernVertices;
            }
        }

        public List<Vertex> AllWesternVertices
        {
            get
            {
                if (this.allWesternVertices == null)
                {
                    this.allWesternVertices = Vertex.GetAllWesternVertices(this.Vertices);
                }

                return this.allWesternVertices;
            }
        }

        public List<Vertex> EasternVertices
        {
            get
            {
                if (this.easternVertices == null)
                {
                    this.easternVertices = Vertex.GetEasternVertices(this.Vertices);
                }

                return this.easternVertices;
            }
        }

        public List<Vertex> AllNorthernVertices
        {
            get
            {
                if (this.allNorthernVertices == null)
                {
                    this.allNorthernVertices = Vertex.GetNorthernVertices(this.AllWesternVertices, this.EasternVertices);
                }

                return this.allNorthernVertices;
            }
        }


        public List<Vertex> NorthernVertices
        {
            get
            {
                if (this.northernVertices == null)
                {
                    this.northernVertices = Vertex.GetNorthernVertices(this.WesternVertices, this.EasternVertices);
                }

                return this.northernVertices;
            }
        }

        public List<Vertex> SouthernVertices
        {
            get
            {
                if (this.southernVertices == null)
                {
                    this.southernVertices = Vertex.GetSouthernVertices(this.WesternVertices, this.EasternVertices);
                }

                return this.southernVertices;
            }
        }

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

            this.ClearVertexCaches();
        }

        public void ClearVertexCaches()
        {
            this.westernVertices = null;
            this.northernVertices = null;
            this.easternVertices = null;
            this.southernVertices = null;
            this.allNorthernVertices = null;
            this.allWesternVertices = null;
        }


        public override string ToString()
        {
            String result = this.vegetationType.ToString();

            foreach (Vertex v in this.vertices)
            {
                result += "|" + Convert.ToSingle(v.X).ToString() + "|" + Convert.ToString(v.Y).ToString();
            }

            return result;
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
