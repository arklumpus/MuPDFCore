using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class MuPDFTests
    {
        [TestMethod]
        public async Task MuPDFOutputRedirection()
        {
            await MuPDF.RedirectOutput();

            MuPDF.ResetOutput();
        }

        [TestMethod]
        public async Task MuPDFOutputRedirectionStdout()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
            string receivedMessage = null;

            void eventHandler(object sender, MessageEventArgs e)
            {
                receivedMessage = e.Message;
                semaphore.Release();
            }

            MuPDF.StandardOutputMessage += eventHandler;

            await MuPDF.RedirectOutput();

            string testString = "Test writing to stdout";

            NativeMethods.WriteToFileDescriptor(1, testString + "\n", testString.Length + 1);

            bool result = await semaphore.WaitAsync(1000);

            Assert.IsTrue(result, "Timed out waiting for the event handler to fire.");
            Assert.IsTrue(receivedMessage.Contains(testString), "The string received by the event handler does not contain the test string. Expected <" + testString + ">, received <" + receivedMessage + ">." );

            MuPDF.StandardOutputMessage -= eventHandler;

            MuPDF.ResetOutput();
        }

        [TestMethod]
        public async Task MuPDFOutputRedirectionStderr()
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);
            string receivedMessage = null;

            void eventHandler(object sender, MessageEventArgs e)
            {
                receivedMessage = e.Message;
                semaphore.Release();
            }

            MuPDF.StandardErrorMessage += eventHandler;

            await MuPDF.RedirectOutput();

            string testString = "Test writing to stderr";

            NativeMethods.WriteToFileDescriptor(2, testString + "\n", testString.Length + 1);

            bool result = await semaphore.WaitAsync(1000);

            Assert.IsTrue(result, "Timed out waiting for the event handler to fire.");
            Assert.IsTrue(receivedMessage.Contains(testString), "The string received by the event handler does not contain the test string. Expected <" + testString + ">, received <" + receivedMessage + ">.");

            MuPDF.StandardErrorMessage -= eventHandler;

            MuPDF.ResetOutput();
        }
    }
}