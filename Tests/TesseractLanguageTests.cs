using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.IO;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class TesseractLanguageTests
    {
        [TestMethod]
        public void TesseractLanguageFastCreation()
        {
            TesseractLanguage language = new TesseractLanguage(TesseractLanguage.Fast.Eng);
            Assert.IsTrue(File.Exists(Path.Combine(language.Prefix, language.Language + ".traineddata")));
            
            try
            {
                File.Delete(Path.Combine(language.Prefix, language.Language + ".traineddata"));
            }
            catch
            { }
        }

        [TestMethod]
        public void TesseractLanguageBestCreation()
        {
            TesseractLanguage language = new TesseractLanguage(TesseractLanguage.Best.Eng);
            Assert.IsTrue(File.Exists(Path.Combine(language.Prefix, language.Language + ".traineddata")));

            try
            {
                File.Delete(Path.Combine(language.Prefix, language.Language + ".traineddata"));
            }
            catch
            { }
        }

        [TestMethod]
        public void TesseractLanguageFastScriptCreation()
        {
            TesseractLanguage language = new TesseractLanguage(TesseractLanguage.FastScripts.Latin);
            Assert.IsTrue(File.Exists(Path.Combine(language.Prefix, language.Language + ".traineddata")));

            try
            {
                File.Delete(Path.Combine(language.Prefix, language.Language + ".traineddata"));
            }
            catch
            { }
        }

        [TestMethod]
        public void TesseractLanguageBestScriptCreation()
        {
            TesseractLanguage language = new TesseractLanguage(TesseractLanguage.BestScripts.Latin);
            Assert.IsTrue(File.Exists(Path.Combine(language.Prefix, language.Language + ".traineddata")));

            try
            {
                File.Delete(Path.Combine(language.Prefix, language.Language + ".traineddata"));
            }
            catch
            { }
        }
    }
}
