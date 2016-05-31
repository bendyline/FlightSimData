/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using System;
using System.Collections.Generic;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class AutogenTile
    {
        private GenericBuildingDistribution gbd;
        private List<VegetationPolygon> vegetationPolygons = null;
        private List<PolygonBuilding> polygonBuildings = null;
        private List<LibraryObject> libraryObjects= null;
        private List<RectangularVegetationArea> rectangularVegetationAreas = null;

        private RiffRecord rootRecord;

        private double latitude;
        private double longitude;

        public const double LongitudeDegreesPerTile = 0.0146484375;  
        public const double LatitudeDegreesPerTile = 0.010986328125;

        public const double LatitudeTileOffset = (AutogenTile.LatitudeDegreesPerTile);

        public const int TilesLongitude = 24576;
        public const int TilesLatitude = 16384;

        public int V
        {
            get
            {
                return Convert.ToInt32(Math.Floor(   ((90 -  (this.latitude +LatitudeTileOffset)  ) *   TilesLatitude) / 180) );
            }

            set
            {
                this.Latitude = (90 - (value * LatitudeDegreesPerTile)) - LatitudeTileOffset;
            }
        }

        public int U
        {
            get
            {
                return Convert.ToInt32(Math.Floor( ((this.longitude + 180) * TilesLongitude) / 360 ));
            }

            set
            {
                this.Longitude = (value * LongitudeDegreesPerTile) - 180;
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

        public List<RectangularVegetationArea> RectangularVegetationAreas
        {
            get
            {
                return this.rectangularVegetationAreas;
            }
        }

        public double Latitude
        {
            get
            {
                return this.latitude;
            }

            set
            {
                this.latitude = value;
            }
        }

        public double Longitude
        {
            get
            {
                return this.longitude;
            }

            set
            {
                this.longitude = value;
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

        public void Load(String path)
        {
            FileInfo fi = new FileInfo(path);

            this.Initialize();

            this.LoadFromFileName(fi.Name);
        
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
                        else if (art == RecordType.VegetationPolygonPRDE)
                        {
                            VegetationPolygon vp = new VegetationPolygon();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;
                        }
                        else if (art == RecordType.PolygonBuildingPBDE)
                        {
                            PolygonBuilding vp = new PolygonBuilding();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;
                        }
                        else if (art == RecordType.LibraryObjectA2GE)
                        {
                            LibraryObject vp = new LibraryObject();

                            vp.Load(fs, sectionSize);

                            childRecord = vp;
                        }

                        if (childRecord != null)
                        {
                            recordStack[recordStack.Count - 1].ChildRecords.Add(childRecord);
                        }
                    }
                }
            }
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
