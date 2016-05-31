/* Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. */

    using Bendyline.Base;
using System;
using System.IO;

namespace Bendyline.FlightSimulator.Data
{
    public class FlightSimulatorData
    {


        public void LoadAutogenFolder(String path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            if (!di.Exists)
            {
                Log.Error("Autogen folder '" + path + "' does not exist.");
                return;
            }

            FileInfo[] files = di.GetFiles();

            foreach (FileInfo fi in files)
            {
                if (fi.Extension.EndsWith(".agn"))
                {
                    AutogenTile at = new AutogenTile();
                    at.Load(fi.FullName);
                }
            }
        }
    }
}
