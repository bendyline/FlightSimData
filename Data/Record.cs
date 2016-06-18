/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.Collections.Generic;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public abstract class Record
    {
        private List<Record> childRecords;
        private RecordType[] allowedChildren;
        private RecordType recordType;

        public static String[] RecordTypeTags =
        new String[] {
            "VERS",
            "GBDD",
            "GBTE",
            "RHTE",
            "GBLD",
            "GBLR",
            "PREG",
            "RIFF",
            "PRDE",
            "VGRD",
            "VGRO",
            "VGRE",
            "VGRG",
            "AGN2",
            "A2GE",
            "AGNX",
            "PBLD",
            "PBDE",
            "ROWH"
        };

        public virtual RecordType RecordType
        {
            get
            {
                return this.recordType;
            }

            set
            {
                this.recordType = value;
            }
        }

        public String RecordTypeTag
        {
            get
            {
                return RecordTypeTags[(int)this.RecordType];
            }
        }

        public RecordType[] AllowedChildTypes
        {
            get
            {
                return this.allowedChildren;
            }

            set
            {
                this.allowedChildren = value;
            }
        }

        public List<Record> ChildRecords
        {
            get
            {
                if (this.childRecords == null)
                {
                    this.childRecords = new List<Record>();
                }

                return this.childRecords;
            }
        }

        public int TotalSize
        {
            get
            {
                int size = this.NodeSize;

                foreach (Record record in this.ChildRecords)
                {
                    size += record.TotalSize + 8;  // 8 = size of child record tag type + size of child record
                }

                return size;
            }

        }

        public abstract int NodeSize { get;  }

        public virtual void Save(Stream s)
        {
            String tag = this.RecordTypeTag;

            foreach (char c in tag)
            {
                s.WriteByte((byte)c);
            }

            this.WriteInt(s, this.TotalSize);
        }

        public virtual void SaveChildren(Stream s)
        {
            foreach (Record r in this.ChildRecords)
            {
                r.Save(s);
            }
        }

        public abstract void Load(Stream stream, int sectionSize);

        protected Guid ReadGuid(Stream fs)
        {
            byte[] guidbytes = new byte[16];

            fs.Read(guidbytes, 0, 16);

            Guid g = new Guid(guidbytes);

            return g;
        }

        protected void WriteGuid(Stream fs, Guid g)
        {
            byte[] guidBytes = g.ToByteArray();


            fs.Write(guidBytes, 0, 16);
        }

        protected void WriteInt(Stream fs, Int32 intToWrite)
        {
            byte[] int32bytes = BitConverter.GetBytes(intToWrite);
            
            fs.Write(int32bytes, 0, 4);
        }

        protected void WriteFloat(Stream fs, float floatToWrite)
        {
            byte[] floatbytes = BitConverter.GetBytes(floatToWrite);

            fs.Write(floatbytes, 0, 4);
        }

        protected void WriteChar(Stream fs, char c)
        {
            fs.WriteByte((byte)c);
        }

        protected Int32 ReadInt(Stream fs)
        {
            byte[] int32bytes = new byte[4];

            fs.Read(int32bytes, 0, 4);

            return BitConverter.ToInt32(int32bytes, 0);
        }

        protected float ReadFloat(Stream fs)
        {
            byte[] floatBytes = new byte[4];

            fs.Read(floatBytes, 0, 4);

            return BitConverter.ToSingle(floatBytes, 0);
        }

        protected char ReadChar(Stream fs)
        {
            return (char)fs.ReadByte();
        }

    }
}
