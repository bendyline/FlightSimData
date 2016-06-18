/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using System;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class GenericBuildingDistribution : Record
    {
        private Int32 oneAndTwoFloorBuildingPercent = 0;
        private Int32 threeToFiveFloorBuildingPercent = 0;
        private Int32 sixToEightFloorBuildingPercent = 0;
        private Int32 nineToTwelveFloorBuildingPercent = 0;

        public Int32 OneAndTwoFloorBuildingPercent
        {
            get
            {
                return this.oneAndTwoFloorBuildingPercent;
            }

            set
            {
                this.oneAndTwoFloorBuildingPercent = value;
            }
        }

        public Int32 SixToEightFloorBuildingPercent
        {
            get
            {
                return this.sixToEightFloorBuildingPercent;
            }

            set
            {
                this.sixToEightFloorBuildingPercent = value;
            }
        }

        public Int32 ThreeToFiveFloorBuildingPercent
        {
            get
            {
                return this.threeToFiveFloorBuildingPercent;
            }

            set
            {
                this.threeToFiveFloorBuildingPercent = value;
            }
        }

        public Int32 NineToTwelveFloorBuildingPercent
        {
            get
            {
                return this.nineToTwelveFloorBuildingPercent;
            }

            set
            {
                this.sixToEightFloorBuildingPercent = value;
            }
        }
        public override RecordType RecordType
        {
            get
            {
                return RecordType.GenericBuildingDistributionGBDD;
            }

            set
            {
                base.RecordType = value;
            }
        }

        public override int NodeSize
        {
            get
            {
                return 16;
            }
        }

        public override void Load(Stream stream, int sectionSize)
        {
            this.oneAndTwoFloorBuildingPercent = ReadInt(stream);
            this.threeToFiveFloorBuildingPercent = ReadInt(stream);
            this.sixToEightFloorBuildingPercent = ReadInt(stream);
            this.nineToTwelveFloorBuildingPercent = ReadInt(stream);
        }

        public override void Save(Stream s)
        {
            base.Save(s);

            this.WriteInt(s, this.oneAndTwoFloorBuildingPercent);
            this.WriteInt(s, this.threeToFiveFloorBuildingPercent);
            this.WriteInt(s, this.sixToEightFloorBuildingPercent);
            this.WriteInt(s, this.nineToTwelveFloorBuildingPercent);
        }
    }
}
