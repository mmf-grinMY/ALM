using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;

using Xunit.Sdk;

namespace ALM.Tests
{
    public class DatFileDataAttribute : DataAttribute
    {
        private readonly string _filePath;
        private readonly string _horizon;
        public DatFileDataAttribute(string filePath, string horizon)
        {
            _filePath = filePath;
            _horizon = horizon;
        }
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) throw new ArgumentNullException(nameof(testMethod));

            using (var reader = new StreamReader(_filePath))
            {
                yield return new object[] { reader.ReadLine(), _horizon };
            }
        }
    }
}
