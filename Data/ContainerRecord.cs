/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class ContainerRecord : Record
    {


        public override int NodeSize
        {
            get
            {
                return 0;
            }
        }

        public override void Load(Stream stream, int sectionSize)
        {

        }

        public override void Save(Stream s)
        {
            base.Save(s);
            base.SaveChildren(s);
        }
    }
}
