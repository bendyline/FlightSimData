/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class RiffRecord : Record
    {
        private String type = "AGNX";

        public String Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public override int NodeSize
        {
            get
            {
                return 4;
            }
        }

        public override RecordType RecordType
        {
            get
            {
                return RecordType.RiffFileRIFF;
            }

            set
            {
                base.RecordType = value;
            }
        }

        public override void Load(Stream fs, int sectionSize)
        {
            this.type = String.Empty;

            this.type += ReadChar(fs);
            this.type += ReadChar(fs);
            this.type += ReadChar(fs);
            this.type += ReadChar(fs);
        }

        public override void Save(Stream s)
        {
            base.Save(s);

            string typeToWrite = this.type;

            if (typeToWrite == null || typeToWrite.Length != 4)
            {
                typeToWrite = "AGNX";
            }

            foreach (char c in typeToWrite)
            {
                this.WriteChar(s, c);
            }

            base.SaveChildren(s);
        }
    }
}
