using ALM.Entities;

using System;

using Xunit.Abstractions;
using Xunit;

namespace ALM.Tests
{
    public class PrimitiveTests
    {
        private readonly ITestOutputHelper _output;
        public PrimitiveTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void PrimitiveCtor_ParseJsonString_CreateObject()
        {
            var primitive = new Primitive(
                "POINT((20 20))",
                "{\"DrawType\": \"TMMTTFSignDrawParams\", \"Symbol\": 91, \"Color\": 11337728, \"FontName\": \"D:\\MapWin\\Fonts\\pnt!.chr\", \"FontScaleX\": \"1\", \"FontScaleY\": \"1\"}",
                "{}",
                "Test",
                "12",
                "",
                "",
                "12",
                Guid.NewGuid().ToString()
            );

            Assert.Equal("pnt!.chr", primitive.DrawSettings.Value<string>("FontName"));
        }
    }
}
