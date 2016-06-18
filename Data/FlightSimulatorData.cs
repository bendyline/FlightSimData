/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

using Bendyline.Base;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class FlightSimulatorData
    {
        private List<AutogenTile> tiles;
        private double northWestLatitude;
        private double northWestLongitude;
        private double southEastLatitude;
        private double southEastLongitude;

        public double NorthWestLatitude
        {
            get
            {
                return northWestLatitude;
            }
        }

        public double NorthWestLongitude
        {
            get
            {
                return northWestLongitude;
            }
        }
        public double SouthEastLatitude
        {
            get
            {
                return southEastLatitude;
            }
        }

        public double SouthEastLongitude
        {
            get
            {
                return southEastLongitude;
            }
        }

        public List<AutogenTile> GetTilesInBoundingBox(double nwLatitude, double nwLongitude, double seLatitude, double seLongitude)
        { 
            List<AutogenTile> results = new List<AutogenTile>();

            foreach (AutogenTile autogenTile in tiles)
            {
                if (    (autogenTile.NorthWestLatitude <= nwLatitude && 
                        autogenTile.NorthWestLatitude > seLatitude - AutogenTile.LatitudeDegreesPerTile &&
                        autogenTile.NorthWestLongitude >= nwLongitude &&
                        autogenTile.NorthWestLongitude <= seLongitude  + AutogenTile.LongitudeDegreesPerTile ) ||

                         (autogenTile.SouthEastLatitude <= nwLatitude &&
                        autogenTile.SouthEastLatitude > seLatitude - AutogenTile.LatitudeDegreesPerTile &&
                        autogenTile.SouthEastLongitude >= nwLongitude &&
                        autogenTile.SouthEastLongitude <= seLongitude + AutogenTile.LongitudeDegreesPerTile
                        )
                    )
                {
                    results.Add(autogenTile);
                }
            }

            return results;
        }

        public void LoadAutogenFolder(String path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            this.tiles = new List<AutogenTile>();

            if (!di.Exists)
            {
                Log.Error("Autogen folder '" + path + "' does not exist.");
                return;
            }

            FileInfo[] files = di.GetFiles();

            this.northWestLatitude = -90;
            this.northWestLongitude = 180;
            this.southEastLatitude = 90;
            this.southEastLongitude = -180;

            foreach (FileInfo fi in files)
            {
                if (fi.Extension.EndsWith(".agn"))
                {
                    AutogenTile at = new AutogenTile();

                    at.Path = fi.FullName;

                    if (at.NorthWestLatitude > this.northWestLatitude)
                    {
                        this.northWestLatitude = at.NorthWestLatitude;
                    }

                    if (at.NorthWestLongitude < this.northWestLongitude)
                    {
                        this.northWestLongitude = at.NorthWestLongitude;
                    }

                    if (at.NorthWestLatitude - AutogenTile.LatitudeDegreesPerTile < this.southEastLatitude)
                    {
                        this.southEastLatitude = at.NorthWestLatitude - AutogenTile.LatitudeDegreesPerTile;
                    }

                    if (at.NorthWestLongitude + AutogenTile.LongitudeDegreesPerTile > this.southEastLongitude)
                    {
                        this.southEastLongitude = at.NorthWestLongitude + AutogenTile.LongitudeDegreesPerTile;
                    }

                    this.tiles.Add(at);
                }
            }
        }
        public void LoadAllAutogenTiles()
        {
            foreach (AutogenTile tile in this.tiles)
            {
                if (!tile.IsLoaded)
                {
                    tile.Load();
                }
            }
        }
    }
}
