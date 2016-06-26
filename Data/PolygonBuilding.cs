/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.Collections.Generic;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class PolygonBuilding : Record
    {
        private List<Vertex> vertices;
        private double extrusionWidth = -0.00945066f;
        private Guid buildingType;

        public double ExtrusionWidth
        {
            get
            {
                return this.extrusionWidth;
            }

            set
            {
                this.extrusionWidth = value;
            }
        }

        public override RecordType RecordType
        {
            get
            {
                return RecordType.PolygonBuildingPBDE;
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

        public Guid BuildingType
        {
            get
            {
                return this.buildingType;
            }

            set
            {
                this.buildingType = value;
            }
        }

        public override int NodeSize
        {
            get
            {
                // GUID = 16 bytes, FLOAT for height = 4 bytes, INT for vertext count = 4 bytes, each vertex is 8 bytes.
                return 28 + (this.vertices.Count * 8);
            }
        }

        public PolygonBuilding()
        {
            this.vertices = new List<Vertex>();
        }
        

        public override void Load(Stream fs, int sectionSize)
        {
            this.buildingType = ReadGuid(fs);
            int val = ReadInt(fs);
            this.extrusionWidth = ReadFloat(fs);
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

            this.WriteGuid(s, this.buildingType);
            this.WriteInt(s, 1);
            this.WriteFloat(s, this.extrusionWidth);
            this.WriteInt(s, this.vertices.Count);

            foreach (Vertex v in this.vertices)
            {
                this.WriteFloat(s, v.X);
                this.WriteFloat(s, v.Y);
            }
        }
    }
}
