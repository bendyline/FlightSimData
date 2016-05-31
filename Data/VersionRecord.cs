/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class VersionRecord : Record
    {
        private Int32 version = 1091777331;

        public override RecordType RecordType
        {
            get
            {
                return RecordType.VersionVERS;
            }

            set
            {
                base.RecordType = value;
            }
        }

        public Int32 Version
        {
            get
            {
                return this.version;
            }
        }

        public override int NodeSize
        {
            get
            {
                return 4;
            }
        }

        public VersionRecord()
        {

        }
        

        public override void Load(Stream fs, int sectionSize)
        {
            this.version = ReadInt(fs);
        }

        public override void Save(Stream s)
        {
            base.Save(s);

            this.WriteInt(s, this.version);
        }
    }
}
