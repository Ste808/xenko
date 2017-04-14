﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class VisualTestCurves : GameTest
    {
        public VisualTestCurves() : base("VisualTestCurves")
        {
            IndividualTestVersion = 1;  //  Changed the default rotation for paritcles using curve rotation
        }

        [Test]
        public void RunVisualTests()
        {
            RunGameTest(new VisualTestCurves());
        }
    }
}
