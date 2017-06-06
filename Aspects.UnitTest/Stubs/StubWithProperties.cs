// <copyright file="StubWithProperties.cs" company="Infosoft AS">
// Copyright (c) 2017 All Rights Reserved
// </copyright>
// <author>esbbach</author>
// <email>esbbach@INFOSOFT</email>
// <date>6/6/2017 11:14:57 AM</date>
// <summary></summary>
using System;
using Aspects.Object;

namespace Aspects.UnitTest.Stubs
{
    /// <summary>
    ///
    /// </summary>
    [PropertyHashCodeAspect]
    public class StubWithProperties
    {
        public string MyFirstProperty { get; set; }

        public int MySecondProperty { get; set; }

        public bool MyThirdProperty { get; set; }

        public DateTime MyDateProperty { get; set; }
    }
}