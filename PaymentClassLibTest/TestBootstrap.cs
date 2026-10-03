using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace PaymentClassLibTest
{
    [TestClass]
    public static class TestBootstrap
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            try
            {
                // Инициализируем Windows App SDK для WinUI-типов
                Bootstrap.TryInitialize(0x00010006, out var hr);
            }
            catch
            {
                // Если не получилось — тесты WinUI упадут, но консольные пройдут
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Bootstrap.Shutdown();
        }
    }
}