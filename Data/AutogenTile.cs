/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using Bendyline.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class AutogenTile
    {
        private GenericBuildingDistribution gbd;
        private List<GenericBuilding> genericBuildings = null;
        private List<VegetationPolygon> vegetationPolygons = null;
        private List<PolygonBuilding> polygonBuildings = null;
        private List<LibraryObject> libraryObjects= null;
        private List<RectangularVegetationArea> rectangularVegetationAreas = null;

        private RiffRecord rootRecord;

        private double northWestLatitude;
        private double northWestLongitude;

        public const double LongitudeDegreesPerTile = 0.0146484375;  
        public const double LatitudeDegreesPerTile = 0.010986328125;

        public const double LatitudeTileOffset = 0;//(AutogenTile.LatitudeDegreesPerTile / 2);

        public const int TilesLongitude = 24576;
        public const int TilesLatitude = 16384;

        public const double PolygonClosenessThreshold = 0.01;
        private bool isLoaded = false;

        private String path;

        public String Path
        {
            get
            {
                return path;
            }

            set
            {
                this.path = value;

                if (!String.IsNullOrEmpty(this.path))
                {
                    FileInfo fi = new FileInfo(this.path);

                    this.LoadFromFileName(fi.Name);
                }
            }
        }

        public GenericBuildingDistribution GenericBuildingDistribution
        {
            get
            {
                if (this.gbd == null)
                {
                    this.gbd = new GenericBuildingDistribution();
                }

                return this.gbd;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return this.isLoaded;
            }
        }

        public int V
        {
            get
            {
                return Convert.ToInt32(Math.Floor(   ((90 -  (this.northWestLatitude +LatitudeTileOffset)  ) *   TilesLatitude) / 180) );
            }

            set
            {
                this.NorthWestLatitude = (90 - (value * LatitudeDegreesPerTile)) - LatitudeTileOffset;
            }
        }

        public int U
        {
            get
            {
                return Convert.ToInt32(Math.Floor( ((this.northWestLongitude + 180) * TilesLongitude) / 360 ));
            }

            set
            {
                this.NorthWestLongitude = (value * LongitudeDegreesPerTile) - 180;
            }
        }

        public List<LibraryObject> LibraryObjects
        {
            get
            {
                return this.libraryObjects;
            }
        }

        public List<VegetationPolygon> VegetationPolygons
        {
            get
            {
                return this.vegetationPolygons;
            }
        }

        public List<PolygonBuilding> PolygonBuildings
        {
            get
            {
                return this.polygonBuildings;
            }
        }

        public List<GenericBuilding> GenericBuildings
        {
            get
            {
                return this.genericBuildings;
            }
        }

        public List<RectangularVegetationArea> RectangularVegetationAreas
        {
            get
            {
                return this.rectangularVegetationAreas;
            }
        }

        public double WidthInKm
        {
            get
            {
                return GeoUtilities.GetDistance(this.NorthWestLatitude, this.NorthWestLongitude, this.NorthWestLatitude, this.SouthEastLongitude);
            }
        }

        public double HeightInKm
        {
            get
            {
                return GeoUtilities.GetDistance(this.NorthWestLatitude, this.NorthWestLongitude, this.SouthEastLatitude, this.NorthWestLongitude);
            }
        }

        public double SouthEastLatitude
        {
            get
            {
                return this.northWestLatitude - AutogenTile.LatitudeDegreesPerTile;
            }
        }

        public double SouthEastLongitude
        {
            get
            {
                return this.northWestLongitude + AutogenTile.LongitudeDegreesPerTile;
            }
        }

        public double NorthWestLatitude
        {
            get
            {
                return this.northWestLatitude;
            }

            set
            {
                this.northWestLatitude = value;
            }
        }

        public double NorthWestLongitude
        {
            get
            {
                return this.northWestLongitude;
            }

            set
            {
                this.northWestLongitude = value;
            }
        }

        public AutogenTile()
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.vegetationPolygons = new List<VegetationPolygon>();
            this.polygonBuildings = new List<PolygonBuilding>();
            this.libraryObjects = new List<LibraryObject>();
            this.rectangularVegetationAreas = new List<RectangularVegetationArea>();
            this.genericBuildings = new List<GenericBuilding>();
        }

        public void SnapPolygonsToNorthwestSide()
        {            
            for (int j = 0; j < this.VegetationPolygons.Count; j++)
            {
                VegetationPolygon primary = this.VegetationPolygons[j];

                List<Vertex> westernVertices= this.GetAllWesternVertices(primary.Vertices);
                List<Vertex> easternVertices = this.GetEasternVertices(primary.Vertices);
                List<Vertex> northernVertices = this.GetNorthernVertices(westernVertices, easternVertices);

                foreach (Vertex v in westernVertices)
                {
                    if (v.X < (PolygonClosenessThreshold * 2))
                    {
                        v.X = 0;
                    }
                }

                foreach (Vertex v in northernVertices)
                {
                    if (v.Y < (PolygonClosenessThreshold * 2))
                    {
                        v.Y = 0;
                    }
                }

                Debug.Assert(primary.Vertices[1].Y != primary.Vertices[2].Y);
            }
        }

        public List<Vertex> GetAllWesternVertices(List<Vertex> vertices)
        {
            List<Vertex> westernVertices = new List<Vertex>();

            double x = 1;
            
            foreach (Vertex v in vertices)
            {
                if (v.X < x)
                {
                    x = v.X;
                }
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];

                if (Math.Abs(v.X - x) < PolygonClosenessThreshold)
                {
                    westernVertices.Add(v);
                }
            }

            return westernVertices;
        }


        public List<Vertex> GetWesternVertices(List<Vertex> vertices)
        {
            List<Vertex> westernVertices = new List<Vertex>();

            double x = 1;

            int maxStreak = 0;
            int curStreak = 0;

            foreach (Vertex v in vertices)
            {
                if (v.X < x)
                {
                    x = v.X;
                }
            }

            for (int i = 1; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];

                if (v.X == x)
                {
                    westernVertices.Add(v);

                    curStreak++;

                    if (curStreak > maxStreak)
                    {
                        maxStreak = curStreak;
                    }
                }
                else
                {
                    curStreak = 0;
                }
            }

            if (maxStreak != westernVertices.Count)
            {
                westernVertices.Clear();
            }


            return westernVertices;
        }


        public List<Vertex> GetEasternVertices(List<Vertex> vertices)
        {
            List<Vertex> easternVertices = new List<Vertex>();

            double x = 0;

            int maxStreak = 0;
            int curStreak = 0;

            foreach (Vertex v in vertices)
            {
                if (v.X > x)
                {
                    x = v.X;
                }
            }

            for (int i=1; i<vertices.Count; i++)
            {
                Vertex v = vertices[i];

                if (v.X == x)
                {
                    easternVertices.Add(v);

                    curStreak++;

                    if (curStreak > maxStreak)
                    {
                        maxStreak = curStreak;
                    }
                }
                else
                {
                    curStreak = 0;
                }
            }

            if (maxStreak != easternVertices.Count)
            {
                easternVertices.Clear();
            }

            return easternVertices;
        }

        public List<Vertex> GetSouthernVertices(List<Vertex> westVertices, List<Vertex> eastVertices)
        {
            List<Vertex> southernVertices = new List<Vertex>();

            if (eastVertices.Count == 2 && westVertices.Count == 2)
            {
                if (westVertices[1].Y < westVertices[0].Y)
                {
                    southernVertices.Add(westVertices[1]);
                }
                else
                {
                    southernVertices.Add(westVertices[0]);
                }

                if (eastVertices[1].Y < eastVertices[0].Y)
                {
                    southernVertices.Add(eastVertices[1]);
                }
                else
                {
                    southernVertices.Add(eastVertices[0]);
                }
            }

            return southernVertices;
        }

        public List<Vertex> GetNorthernVertices(List<Vertex> westVertices, List<Vertex> eastVertices)
        {
            List<Vertex> northernVertices = new List<Vertex>();

            if (eastVertices.Count == 2 && westVertices.Count == 2)
            {
                if (westVertices[1].Y > westVertices[0].Y)
                {
                    northernVertices.Add(westVertices[1]);
                }
                else
                {
                    northernVertices.Add(westVertices[0]);
                }

                if (eastVertices[1].Y > eastVertices[0].Y)
                {
                    northernVertices.Add(eastVertices[1]);
                }
                else
                {
                    northernVertices.Add(eastVertices[0]);
                }
            }

            return northernVertices;
        }

        public void CoalesceRectangularPolygons()
        {
            for (int j = 0; j < this.VegetationPolygons.Count; j++)
            {
                VegetationPolygon primary = this.VegetationPolygons[j];

                for (int k = j + 1; k < this.VegetationPolygons.Count; k++)
                {
                    VegetationPolygon secondary = this.VegetationPolygons[k];

                    List<Vertex> primaryEast = GetEasternVertices(primary.Vertices);
                    List<Vertex> secondaryWest = GetWesternVertices(secondary.Vertices);
                    List<Vertex> secondaryEast = GetEasternVertices(secondary.Vertices);

                    if (primaryEast.Count == 2 && secondaryWest.Count == 2 && secondary.VegetationType == primary.VegetationType && secondary != primary)
                    {
                        if (primaryEast[0].IsClose(secondaryWest[1], PolygonClosenessThreshold) && primaryEast[1].IsClose(secondaryWest[0], PolygonClosenessThreshold))
                        {

                            if (secondaryEast.Count == 2)
                            {
                                if (Math.Abs(primaryEast[0].Y - secondaryEast[0].Y) < PolygonClosenessThreshold && Math.Abs(primaryEast[1].Y - secondaryEast[1].Y) <= PolygonClosenessThreshold)
                                {
                                    primaryEast[0].X = secondaryEast[0].X;
                                    primaryEast[0].Y = secondaryEast[0].Y;

                                    primaryEast[1].X = secondaryEast[1].X;
                                    primaryEast[1].Y = secondaryEast[1].Y;

                                    this.VegetationPolygons.Remove(secondary);
                                    k--;
                                }
                            }
                        }
                    }
                }
            }

            for (int j = this.VegetationPolygons.Count - 1; j > 0; j--)
            {
                VegetationPolygon primary = this.VegetationPolygons[j];

                for (int k = j - 1; k > 0; k--)
                {
                    VegetationPolygon secondary = this.VegetationPolygons[k];

                    List<Vertex> primaryEast = GetEasternVertices(primary.Vertices);
                    List<Vertex> secondaryWest = GetWesternVertices(secondary.Vertices);
                    List<Vertex> secondaryEast = GetEasternVertices(secondary.Vertices);

                    List<Vertex> primaryWest = GetWesternVertices(primary.Vertices);
                    List<Vertex> primarySouth = GetSouthernVertices(primaryWest, primaryEast);
                    List<Vertex> secondaryNorth = GetNorthernVertices(secondaryWest, secondaryEast);

                    if (primarySouth.Count == 2 && secondaryNorth.Count == 2 && secondary.VegetationType == primary.VegetationType && secondary != primary)
                    {
                        if (primarySouth[0].IsClose(secondaryNorth[0], PolygonClosenessThreshold * 2) && 
                            primarySouth[1].IsClose(secondaryNorth[1], PolygonClosenessThreshold * 2))
                        {
                            List<Vertex> secondarySouth = GetSouthernVertices(secondaryWest, secondaryEast);

                            if (secondarySouth.Count == 2)
                            {
                                if (Math.Abs(primarySouth[0].X - secondarySouth[0].X) < PolygonClosenessThreshold * 5 && 
                                    Math.Abs(primarySouth[1].X - secondarySouth[1].X) <= PolygonClosenessThreshold * 5 &&
                                    secondarySouth[0].Y < primarySouth[0].Y &&
                                    secondarySouth[1].Y < primarySouth[1].Y)
                                {
                                    primarySouth[0].X = secondarySouth[0].X;
                                    primarySouth[0].Y = secondarySouth[0].Y;

                                    primarySouth[1].X = secondarySouth[1].X;
                                    primarySouth[1].Y = secondarySouth[1].Y;

                                    this.VegetationPolygons.Remove(secondary);
                                    k--;
                                }
                            }
                        }
                    }

                }
            }
        }

        private void JoinPolygonsAcross(List<Vertex> primaryVertices, List<Vertex> secondaryVertices)
        {
            primaryVertices[1] = secondaryVertices[1];
            primaryVertices[2] = secondaryVertices[2];
        }

        private void JoinPolygonsDown(List<Vertex> primaryVertices, List<Vertex> secondaryVertices)
        {
            primaryVertices[2] = secondaryVertices[2];
            primaryVertices[3] = secondaryVertices[3];
        }


        public void SnapToUpperLeftCorner()
        {
            this.U = this.U;
            this.V = this.V;
        }

        public void Save(String path)
        {
            this.rootRecord = new RiffRecord();

            VersionRecord vr = new VersionRecord();
            this.rootRecord.ChildRecords.Add(vr);

            if (gbd == null)
            {
                this.gbd = new GenericBuildingDistribution();
            }

            this.rootRecord.ChildRecords.Add(this.gbd);

            if (this.vegetationPolygons.Count > 0)
            {
                ContainerRecord cr = new ContainerRecord();
                cr.RecordType = RecordType.VegetationPolygonsPREG;
                this.rootRecord.ChildRecords.Add(cr);

                foreach (VegetationPolygon vp in this.vegetationPolygons)
                {
                    cr.ChildRecords.Add(vp);
                }
            }

            if (this.polygonBuildings.Count > 0)
            {
                ContainerRecord cr = new ContainerRecord();
                cr.RecordType = RecordType.PolygonBuildingsPBLD;

                this.rootRecord.ChildRecords.Add(cr);

                foreach (PolygonBuilding vp in this.polygonBuildings)
                {
                    cr.ChildRecords.Add(vp);
                }
            }

            if (this.libraryObjects.Count > 0)
            {
                ContainerRecord cr = new ContainerRecord();
                cr.RecordType = RecordType.LibraryObjectsAGN2;

                this.rootRecord.ChildRecords.Add(cr);

                foreach (LibraryObject lo in this.libraryObjects)
                {
                    cr.ChildRecords.Add(lo);
                }
            }

            if (this.genericBuildings.Count > 0)
            {
                ContainerRecord cr = new ContainerRecord();
                cr.RecordType = RecordType.GenericBuildingSetGBLD;

                this.rootRecord.ChildRecords.Add(cr);

                foreach (GenericBuilding lo in this.genericBuildings)
                {
                    cr.ChildRecords.Add(lo);
                }
            }

            if (this.rectangularVegetationAreas.Count > 0)
            {
                ContainerRecord cr = new ContainerRecord();
                cr.RecordType = RecordType.RectangularVegetationAreasVGRG;

                this.rootRecord.ChildRecords.Add(cr);

                foreach (RectangularVegetationArea rva in this.rectangularVegetationAreas)
                {
                    cr.ChildRecords.Add(rva);
                }
            }


            FileInfo fi = new FileInfo(path);

            using (FileStream fs = fi.Open(FileMode.Create, FileAccess.Write))
            {
                this.rootRecord.Save(fs);
            }
        }

        public void Load()
        {
            if (String.IsNullOrEmpty(this.path))
            {
                return;
            }


            FileInfo fi = new FileInfo(this.path);

            this.Initialize();
        
            List<Record> recordStack = new List<Record>();

            using (FileStream fs = fi.OpenRead())
            {
                while (fs.CanRead)
                {
                    byte[] record = new byte[4];

                    RecordType? art = null;

                    if (fs.Read(record, 0, 4) != 4)
                    {
                        break;
                    }

                    int i = 0;

                    foreach (String tag in Record.RecordTypeTags)
                    {
                        if (record[0] == tag[0] && record[1] == tag[1] && record[2] == tag[2] && record[3] == tag[3])
                        {
                            art = (RecordType)i;
                            break;
                        }

                        i++;
                    }

                    if (art != null)
                    {
                        int sectionSize = 0;

                        sectionSize = ReadInt(fs);

                        if (recordStack.Count > 1)
                        {
                            RecordType[] allowedChildren = recordStack[recordStack.Count - 1].AllowedChildTypes;

                            if (allowedChildren != null && allowedChildren.Length > 0)
                            {
                                bool foundType = false;

                                foreach (RecordType allowedChildType in allowedChildren)
                                {
                                    if (allowedChildType == art)
                                    {
                                        foundType = true;
                                    }
                                }

                                if (!foundType)
                                {
                                    recordStack.RemoveAt(recordStack.Count-1);
                                }
                            }
                        }

                        Record childRecord = null;

                        if (art == RecordType.RiffFileRIFF)
                        {
                            this.rootRecord = new RiffRecord();

                            this.rootRecord.Load(fs, sectionSize);

                            recordStack.Add(this.rootRecord);
                        }
                        else if (art == RecordType.VegetationPolygonsPREG)
                        {
                            ContainerRecord cr = new ContainerRecord();

                            cr.RecordType = RecordType.VegetationPolygonsPREG;
                            cr.AllowedChildTypes = new RecordType[] { RecordType.VegetationPolygonPRDE };

                            recordStack.Add(cr);

                            childRecord = cr;

                        }
                        else if (art == RecordType.PolygonBuildingsPBLD)
                        {
                            ContainerRecord cr = new ContainerRecord();

                            cr.RecordType = RecordType.PolygonBuildingsPBLD;
                            cr.AllowedChildTypes = new RecordType[] { RecordType.PolygonBuildingPBDE};

                            recordStack.Add(cr);

                            childRecord = cr;

                        }
                        else if (art == RecordType.GenericBuildingSetGBLD)
                        {
                            ContainerRecord cr = new ContainerRecord();

                            cr.RecordType = RecordType.GenericBuildingSetGBLD;
                            cr.AllowedChildTypes = new RecordType[] { RecordType.GenericBuildingGBLR };

                            recordStack.Add(cr);

                            childRecord = cr;

                        }
                        else if (art == RecordType.LibraryObjectsAGN2)
                        {
                            ContainerRecord cr = new ContainerRecord();

                            cr.RecordType = RecordType.LibraryObjectsAGN2;
                            cr.AllowedChildTypes = new RecordType[] { RecordType.LibraryObjectA2GE};

                            recordStack.Add(cr);

                            childRecord = cr;

                        }
                        else if (art == RecordType.VersionVERS)
                        {
                            VersionRecord vr = new VersionRecord();

                            vr.Load(fs, sectionSize);

                            childRecord = vr;
                        }
                        else if (art == RecordType.GenericBuildingDistributionGBDD)
                        {
                            this.gbd = new GenericBuildingDistribution();

                            gbd.Load(fs, sectionSize);

                            childRecord = gbd;
                        }
                        else if (art == RecordType.GenericBuildingGBLR)
                        {
                            GenericBuilding vp = new GenericBuilding();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;
                            this.genericBuildings.Add(vp);
                        }
                        else if (art == RecordType.VegetationPolygonPRDE)
                        {
                            VegetationPolygon vp = new VegetationPolygon();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;

                            this.vegetationPolygons.Add(vp);
                        }
                        else if (art == RecordType.PolygonBuildingPBDE)
                        {
                            PolygonBuilding vp = new PolygonBuilding();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;
                            this.polygonBuildings.Add(vp);
                        }
                        else if (art == RecordType.LibraryObjectA2GE)
                        {
                            LibraryObject vp = new LibraryObject();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;
                            this.libraryObjects.Add(vp);
                        }
                        else if (art == RecordType.RowHouseROWH)
                        {
                            RowHouse vp = new RowHouse();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;
                            
                        }
                        else
                        {
                            for (int filler=0; filler < sectionSize / 4; filler++)
                            {
                                ReadInt(fs);
                            }
                        }

                        if (childRecord != null)
                        {
                            recordStack[recordStack.Count - 1].ChildRecords.Add(childRecord);
                        }
                    }
                }

            }

            this.isLoaded = true;
        }

        private void WriteRecordHeader(Stream stream, char a, char b, char c, char d, int fileSize)
        {
            stream.WriteByte((byte)a);
            stream.WriteByte((byte)b);
            stream.WriteByte((byte)c);
            stream.WriteByte((byte)d);

            byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);

            stream.WriteByte(fileSizeBytes[0]);
            stream.WriteByte(fileSizeBytes[1]);
            stream.WriteByte(fileSizeBytes[2]);
            stream.WriteByte(fileSizeBytes[3]);

        }

        protected Int32 ReadInt(Stream fs)
        {
            byte[] int32bytes = new byte[4];

            fs.Read(int32bytes, 0, 4);

            return BitConverter.ToInt32(int32bytes, 0);
        }

        public String GetFileNameToken()
        {
            int uVal = this.U;
            int vVal = this.V;

            byte[] bytes = new byte[15];
            int count = 0;

            while (uVal > 0)
            {
                if ((uVal % 2) == 1)
                {
                    bytes[count] = 1;
                }

                uVal >>= 1;

                count++;
            }

            count = 0;

            while (vVal > 0)
            {
                if ( (vVal %2) == 1)
                {
                    bytes[count] += 2;
                }

                vVal >>= 1;

                count++;
            }

            String token = String.Empty;

            for (int i=14; i>=0; i--)
            {
                if (bytes[i] == 0)
                {
                    token += '0';
                }
                else if (bytes[i] == 1)
                {
                    token += '1';
                }
                else if (bytes[i] == 2)
                {
                    token += '2';
                }
                else if (bytes[i] == 3)
                {
                    token += '3';
                }
            }

            return token;
        }

        public void LoadFromFileName(String fileName)
        {
            String subsection = fileName;

            int lastPeriod = subsection.LastIndexOf(".");

            if (lastPeriod >= 0)
            {
                subsection = subsection.Substring(0, lastPeriod);
            }

            int lastSlash = subsection.LastIndexOf("\\");

            if (lastSlash >= 0)
            {
                subsection = subsection.Substring(lastSlash + 1);
            }

            subsection = subsection.ToLower();

            if (subsection.EndsWith("an"))
            {
                subsection = subsection.Substring(0, subsection.Length - 2);
            }

            List<byte> bytes = new List<byte>();

            foreach (char ch in subsection)
            {
                if (Char.IsNumber(ch))
                {
                    if (ch == '0')
                    {
                        bytes.Add(0);
                    }
                    if (ch == '1')
                    {
                        bytes.Add(1);
                    }
                    if (ch == '2')
                    {
                        bytes.Add(2);
                    }
                    if (ch == '3')
                    {
                        bytes.Add(3);
                    }
                }
            }

            int uVal = 0;
            int vVal = 0;

            for (int i=0; i<bytes.Count; i++)
            {
                uVal *= 2;
                vVal *= 2;

                if (bytes[i] == 1 || bytes[i] == 3)
                {
                    uVal += 1;
                }

                if (bytes[i] == 2 || bytes[i] == 3)
                {
                    vVal += 1;
                }
            }

            this.U = uVal;
            this.V = vVal;

            String altToken = GetFileNameToken();
        }
    }
}
