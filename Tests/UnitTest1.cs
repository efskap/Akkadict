using NUnit.Framework;
using System;
using System.Linq;

namespace akkadict
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }
        [Test]
        public void TestAsciify()
        {

            Assert.AreEqual("hurasum", Word.ASCIIfy("ḫurāṣum"));
        }
        [Test]
        public void TestDecline()
        {

            Assert.AreEqual("mārtum".Normalize(), new Word { BaseForm = "mār", PoS =PoS.Noun, NounGender = Gender.F}.Decline().First().Item2.Normalize());
        }

        [Test]
        public void TestSyllabification()
        {
            Assert.AreEqual(Word.Norm("ṣa/bat"), String.Join("/", Word.Syllabify(Word.StringToGlyphs("ṣabat")).Select(x => String.Join("", x.Select(y => y.ToString())))));
            Assert.AreEqual("ki/am", String.Join("/", Word.Syllabify(Word.StringToGlyphs("kiam")).Select(x => String.Join("", x.Select(y => y.ToString())))));
            Assert.AreEqual("e/lum", String.Join("/", Word.Syllabify(Word.StringToGlyphs("elum")).Select(x => String.Join("", x.Select(y => y.ToString())))));

            Assert.AreEqual("ba/la/ti", String.Join("/", Word.Syllabify(Word.StringToGlyphs("balati")).Select(x => String.Join("", x.Select(y => y.ToString())))));
            Assert.AreEqual("i/te/nep/pus", String.Join("/", Word.Syllabify(Word.StringToGlyphs("iteneppus")).Select(x => String.Join("", x.Select(y => y.ToString())))));

            Assert.AreEqual("e/pis/ta/su", String.Join("/", Word.Syllabify(Word.StringToGlyphs("epistasu")).Select(x => String.Join("", x.Select(y => y.ToString())))));
        }
        [Test]
        public void TestVowelSyncope()
        {
            Assert.AreEqual(Word.Norm("napsātum"), Word.ApplyPhonology("napisātum"));
        }
    }
}