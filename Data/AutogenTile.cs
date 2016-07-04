/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using Bendyline.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
        private object tag;

        private RiffRecord rootRecord;

        private double northWestLatitude;
        private double northWestLongitude;

        public const double LongitudeDegreesPerTile = 0.0146484375;  
        public const double LatitudeDegreesPerTile = 0.010986328125;

        public const double LatitudeTileOffset = 0;//(AutogenTile.LatitudeDegreesPerTile / 2);

        public const int TilesLongitude = 24576;
        public const int TilesLatitude = 16384;

        public const double PolygonClosenessThreshold = 0.017;
        private bool isLoaded = false;

        private String path;

        public object Tag
        {
            get
            {
                return this.tag;
            }

            set
            {
                this.tag = value;
            }
        }

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

                List<Vertex> westernVertices= primary.AllWesternVertices;
                List<Vertex> easternVertices = primary.EasternVertices;
                List<Vertex> northernVertices = primary.AllNorthernVertices;

                foreach (Vertex v in westernVertices)
                {
                    if (v.X < (PolygonClosenessThreshold * 2.5))
                    {
                        v.X = 0;
                    }
                }

                foreach (Vertex v in northernVertices)
                {
                    if (v.Y < (PolygonClosenessThreshold * 2.0))
                    {
                        v.Y = 0;
                    }
                }

                Debug.Assert(primary.Vertices[1].Y != primary.Vertices[2].Y);
            }
        }

        public void CoalesceRectangularPolygons()
        {
            CoalesceRectangularPolygons(null, null);
        }

        public void CoalesceRectangularPolygons(Nullable<bool> joinNorthward, Nullable<bool> joinEasterly)
        {
            for (int j = 0; j < this.VegetationPolygons.Count; j++)
            {
                VegetationPolygon primary = null;

                if (j < this.VegetationPolygons.Count - 1)
                {
                    primary = this.VegetationPolygons[j];
                }


                double closestX = Double.MaxValue, closestY = double.MaxValue;
                VegetationPolygon closestPolygon = null;

                for (int k =0; k < this.VegetationPolygons.Count; k++)
                {
                    VegetationPolygon secondary = null;

                    if (k >= j+1 && k < this.VegetationPolygons.Count - 1)
                    {
                        secondary = this.VegetationPolygons[k];
                    }


                    if (secondary != primary && secondary != null && primary != null)
                    { 
                        List<Vertex> primaryEast = primary.EasternVertices;
                        List<Vertex> secondaryWest = secondary.WesternVertices;

                        if (primaryEast.Count == 2 && secondaryWest.Count == 2 && secondary != primary)
                        {
                            if (
                                (joinNorthward == true &&
                                (primaryEast[0].IsCloseAndToNorth(secondaryWest[1], PolygonClosenessThreshold * 3, PolygonClosenessThreshold * 3) &&
                                  primaryEast[1].IsCloseAndToNorth(secondaryWest[0], PolygonClosenessThreshold * 3, PolygonClosenessThreshold * 3))) ||

                                  (joinNorthward == false &&
                                (primaryEast[0].IsCloseAndToSouth(secondaryWest[1], PolygonClosenessThreshold * 3, PolygonClosenessThreshold * 3) &&
                                  primaryEast[1].IsCloseAndToSouth(secondaryWest[0], PolygonClosenessThreshold * 3, PolygonClosenessThreshold * 3))) ||

                                (joinNorthward == null &&
                                (primaryEast[0].IsClose(secondaryWest[1], PolygonClosenessThreshold , PolygonClosenessThreshold) && 
                                  primaryEast[1].IsClose(secondaryWest[0], PolygonClosenessThreshold, PolygonClosenessThreshold)))
                                  
                               )
                            {
                                double closeX = Math.Abs(primaryEast[0].X - secondaryWest[1].X);
                                double closeY = Math.Abs(primaryEast[0].Y - secondaryWest[1].Y);

                                if (closeX < closestX && closeY < closestY)
                                {
                                    closestPolygon = secondary;
                                    closestX = closeX;
                                    closestY = closeY;
                                }
                            }
                        }
                    }
                }


                if (closestPolygon != null && closestPolygon.VegetationType == primary.VegetationType)
                {
                    List<Vertex> primaryEast = primary.EasternVertices;
                    List<Vertex> closestEast = closestPolygon.EasternVertices;

                    if (closestEast.Count == 2)
                    {
                        if (Math.Abs(primaryEast[0].Y - closestEast[0].Y) < PolygonClosenessThreshold &&
                            Math.Abs(primaryEast[1].Y - closestEast[1].Y) <= PolygonClosenessThreshold)
                        {
                            primaryEast[0].X = closestEast[0].X;
                            primaryEast[0].Y = closestEast[0].Y;

                            primaryEast[1].X = closestEast[1].X;
                            primaryEast[1].Y = closestEast[1].Y;

                            this.VegetationPolygons.Remove(closestPolygon);
                            j--;
                        }
                    }
                }
            }

            for (int j = this.VegetationPolygons.Count - 1; j > 0; j--)
            {
                VegetationPolygon primary = null;

                if (j < this.VegetationPolygons.Count - 1)
                {
                    primary = this.VegetationPolygons[j];
                }

                for (int k = j - 1; k > 0; k--)
                {
                    VegetationPolygon secondary = null;

                    if (k >= 0 && k < j - 1 && k < this.VegetationPolygons.Count - 1)
                    {
                        secondary = this.VegetationPolygons[k];
                    }

                    if (primary != secondary && primary != null && secondary != null)
                    {
                        List<Vertex> primaryEast = primary.EasternVertices;
                        List<Vertex> secondaryWest = secondary.WesternVertices;
                        List<Vertex> secondaryEast = secondary.EasternVertices;

                        List<Vertex> primaryWest = primary.WesternVertices;
                        List<Vertex> primarySouth = primary.SouthernVertices;
                        List<Vertex> secondaryNorth = secondary.NorthernVertices;

                        if (primarySouth.Count == 2 && secondaryNorth.Count == 2 && secondary.VegetationType == primary.VegetationType && secondary != primary)
                        {
                            if (primarySouth[0].IsClose(secondaryNorth[0], PolygonClosenessThreshold ) &&
                                primarySouth[1].IsClose(secondaryNorth[1], PolygonClosenessThreshold))
                            {
                                List<Vertex> secondarySouth = secondary.SouthernVertices;

                                if (secondarySouth.Count == 2)
                                {
                                    if (Math.Abs(primarySouth[0].X - secondarySouth[0].X) < PolygonClosenessThreshold *3 &&
                                        Math.Abs(primarySouth[1].X - secondarySouth[1].X) <= PolygonClosenessThreshold * 3 &&
                                        secondarySouth[0].Y < primarySouth[0].Y &&
                                        secondarySouth[1].Y < primarySouth[1].Y)
                                    {
                                        primarySouth[0].X = secondarySouth[0].X;
                                        primarySouth[0].Y = secondarySouth[0].Y;

                                        primarySouth[1].X = secondarySouth[1].X;
                                        primarySouth[1].Y = secondarySouth[1].Y;

                                        this.VegetationPolygons.Remove(secondary);

                                        if (k < j )
                                        {
                                            k++;
                                        }
                                    }
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
            bool tryAgain = true;
            int tryCount = 5;

            while (tryAgain == true)
            {
                try
                {
                    using (FileStream fs = fi.Open(FileMode.Create, FileAccess.Write))
                    {
                        this.rootRecord.Save(fs);
                    }

                    tryAgain = false;
                }
                catch (IOException)
                {
                    tryCount--;
                    tryAgain = (tryCount > 0);

                    Log.Error("File '" + path + "' could not be written; retrying.");

                    Thread.Sleep(500);
                }
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
