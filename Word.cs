using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;

namespace akkadict {

    using Syllable = IList<Glyph>;
    public class Word
    {
        public string Headword { get {
                var d = Decline();
                if (d.Count() > 0) {
                    return Decline().First().Item2; // ought to encode this in the type system... but argh for now the first inflection will be the headword.
                }
                else {
                    return BaseForm;
                }
            } }

        public string Meaning { get; set; }
        public PoS PoS { get; set; }
        [Name("Gender")]
        public Gender? NounGender { get; set; }
        public int Chapter { get; set; }
        public string ThemeVowel { get; set; }

        enum GenderOddity {None, VariableGender, FeminineWithoutT, MascThatsFemInPl, Collective } //  TODO

        private string _BaseForm;
        public string BaseForm { get { return Norm(_BaseForm); } set { _BaseForm = Norm(value); } }//Text.EndsWith("um")? Text.Substring(0, Text.LastIndexOf("um")) : Text;

        public static string Norm(string str) => str.ToLower().Normalize(NormalizationForm.FormD);
        public static string ASCIIfy(string s) => String.Join("",Norm(s).Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        private enum SyllableWeight : int { Light = 0, Heavy = 1, Superheavy = 2 };

        private static SyllableWeight weightOfSyllable(string syllable) {
            var glyphs = Word.StringToGlyphs(syllable);
            var weights = glyphs.Select(g => g.IsVowel ? g.Weight : -1).ToArray();
            if ( weights.Last() == 0) // ends with short vowel
                return SyllableWeight.Light;
            else if (weights.Last() == 1 || (weights.Last() == -1 && weights[weights.Length-2] == 0))
                return SyllableWeight.Heavy;
            else if (weights.Last() == 2 || (weights.Last() == -1 && weights[weights.Length-2] == 1))
                return SyllableWeight.Superheavy;
            else
                throw new ArgumentException(syllable + " = " + String.Join("",weights));
        }

        public static IList<Syllable> ApplyPhonology(IList<Syllable> text) {
            //"The rule is that the last vowel of a succession of syllables that end in a short vowel is dropped" 
            var weightSequences = text.GroupAdjacent(x => weightOfSyllable(String.Join("", x)));
            var afterVSyncope = weightSequences.SelectMany((x) =>
            {
                if (x.Key == SyllableWeight.Light && x.Count() >= 2)
                    return x.Take(x.Count() - 1).Concat( new[] { x.Last().Where(y => !y.IsVowel).ToList() });
                else
                    return x.ToList();

            });
            return afterVSyncope.ToList();

        }

        public static string ApplyPhonology(string source) {
            return String.Join("", ApplyPhonology(Syllabify(StringToGlyphs(source))).SelectMany(x=>x)); }


        //using Glyph = IEnumerable<string>;

        /// <summary>
        /// Groups the deconstructed characters by resultant glyph.
        /// That is, puts all the utf chars that combine into a single letter, into their own string.
        /// e.g. (ASCIIfied) "fo`o" -> ["f", "o`", "o"]
        /// </summary>
        public IList<Glyph> Glyphs => StringToGlyphs(BaseForm);

        public static IList<Glyph> StringToGlyphs(string source) =>
            source
            .ToLower()
            .Normalize(NormalizationForm.FormD)
            .ChunkOn(cur => CharUnicodeInfo.GetUnicodeCategory(cur) != UnicodeCategory.NonSpacingMark)
            .Select(x => new Glyph(x)).ToList();

        public static IList<Syllable> Syllabify(IList<Glyph> source)
        {
            var syllables = new List<Syllable>();
            int i = source.Count() - 1;
            var curSyl = new List<Glyph>();
            while (i >= 0)
            {
                var cur = source[i];

                if ((curSyl.Count > 0 && !curSyl.Last().IsVowel && !cur.IsVowel) // if we got a second C in a row
                    || (cur.IsVowel && curSyl.Any(x => x.IsVowel))) // or already have a vowel and encounter one
                {
                    // done with this syllable, start next one on same pos
                    curSyl.Reverse();
                    syllables.Add(curSyl);
                    curSyl = new List<Glyph>();
                    continue;
                }
                curSyl.Add(cur);
                if (i == 0)
                {
                    curSyl.Reverse();
                    syllables.Add(curSyl);
                    break;
                }

                i--;

            }
            syllables.Reverse();
            return syllables;
        }

        public IList<Glyph> Consonants =>
            Glyphs.Where(x => !x.IsVowel).ToList();
            


        public ValueTuple<DecSpec, string>[] Decline() {
            var res = new List<ValueTuple<DecSpec, string>>();
            var none = new DecSpec { };

            var baseForm = BaseForm;

            switch (PoS) {
                case PoS.Noun:
                    switch (this.NounGender) {
                        case Gender.M:
                        default:
                            res.Add((new DecSpec { Number = Number.S, Case = Case.Nom }, baseForm + "um"));
                            res.Add((new DecSpec { Number = Number.S, Case = Case.Gen }, baseForm + "im"));
                            res.Add((new DecSpec { Number = Number.S, Case = Case.Acc }, baseForm + "am"));

                            res.Add((new DecSpec { Number = Number.P, Case = Case.Nom }, baseForm + "u" + Constants.MACRON));
                            res.Add((new DecSpec { Number = Number.P, Case = Case.Gen }, baseForm + "i" + Constants.MACRON));
                            res.Add((new DecSpec { Number = Number.P, Case = Case.Acc }, baseForm + "i" + Constants.MACRON));

                            break;
                        case Gender.F:
                            string femMarker;
                            // if base ends with two consonants
                            if (!Glyphs[Glyphs.Count - 1].IsVowel && Glyphs.Count >= 2 && !Glyphs[Glyphs.Count - 2].IsVowel)
                                femMarker = "at";
                            else
                                femMarker = "t";
                            res.Add((new DecSpec { Number = Number.S, Case = Case.Nom }, baseForm + femMarker + "um"));
                            res.Add((new DecSpec { Number = Number.S, Case = Case.Gen }, baseForm + femMarker + "im"));
                            res.Add((new DecSpec { Number = Number.S, Case = Case.Acc }, baseForm + femMarker + "am"));

                            res.Add((new DecSpec { Number = Number.P, Case = Case.Nom }, baseForm + ("a" + Constants.MACRON + "t") + "um"));
                            res.Add((new DecSpec { Number = Number.P, Case = Case.Gen }, baseForm + ("a" + Constants.MACRON + "t") + "im"));
                            res.Add((new DecSpec { Number = Number.P, Case = Case.Acc }, baseForm + ("a" + Constants.MACRON + "t") + "im"));

                            break;
                    }
                    break;
                case PoS.Verb:
                    var verbRoot_l = Consonants.ToList();
                    verbRoot_l.Insert(2, new Glyph(ThemeVowel));
                    string verbRoot = String.Join("", verbRoot_l);
                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.Third, Number = Number.S, Gender = Gender.C }, "i" + verbRoot));
                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.Second, Number = Number.S, Gender = Gender.M }, "ta" + verbRoot));
                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.Second, Number = Number.S, Gender = Gender.F }, "ta" + verbRoot + "i" + Constants.MACRON));
                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.First, Number = Number.S, Gender = Gender.C }, "a" + verbRoot));

                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.Third, Number = Number.P, Gender = Gender.M }, "i" + verbRoot + "u" + Constants.MACRON));
                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.Third, Number = Number.P, Gender = Gender.F }, "i" + verbRoot + "a" + Constants.MACRON));
                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.Second, Number = Number.P, Gender = Gender.C }, "ta" + verbRoot + "a" + Constants.MACRON));
                    res.Add((new DecSpec { Tense = Tense.Preterite, Person = Person.First, Number = Number.P, Gender = Gender.C }, "ni" + verbRoot));
                    break;
                case PoS.Adjective:
                    // TODO: create female baseForm and apply vowel syncope

                    res.Add((new DecSpec { Gender = Gender.M, Number = Number.S, Case = Case.Nom }, baseForm + "um"));
                    res.Add((new DecSpec { Gender = Gender.M, Number = Number.S, Case = Case.Gen }, baseForm + "im"));
                    res.Add((new DecSpec { Gender = Gender.M, Number = Number.S, Case = Case.Acc }, baseForm + "am"));

                    res.Add((new DecSpec { Gender = Gender.M, Number = Number.P, Case = Case.Nom }, baseForm + "u" + Constants.MACRON + "tum"));
                    res.Add((new DecSpec { Gender = Gender.M, Number = Number.P, Case = Case.Gen }, baseForm + "u" + Constants.MACRON + "tim"));
                    res.Add((new DecSpec { Gender = Gender.M, Number = Number.P, Case = Case.Acc }, baseForm + "u" + Constants.MACRON + "tim"));

                    res.Add((new DecSpec { Gender = Gender.F, Number = Number.S, Case = Case.Nom }, baseForm + "um"));
                    res.Add((new DecSpec { Gender = Gender.F, Number = Number.S, Case = Case.Gen }, baseForm + "im"));
                    res.Add((new DecSpec { Gender = Gender.F, Number = Number.S, Case = Case.Acc }, baseForm + "am"));

                    res.Add((new DecSpec { Gender = Gender.F, Number = Number.P, Case = Case.Nom }, baseForm.Remove(baseForm.Length - 1) + "a" + Constants.MACRON + "tum"));
                    res.Add((new DecSpec { Gender = Gender.F, Number = Number.P, Case = Case.Gen }, baseForm.Remove(baseForm.Length - 1) + "a" + Constants.MACRON + "tim"));
                    res.Add((new DecSpec { Gender = Gender.F, Number = Number.P, Case = Case.Acc }, baseForm.Remove(baseForm.Length - 1) + "a" + Constants.MACRON + "tim"));

                    break;
                default:
                    break;
            }
            return res.Select((x) => (x.Item1, ApplyPhonology(x.Item2))).ToArray();
        }


    }

    public enum PoS { Noun, Verb, Adjective, Conjunction, Preposition, Adverb };
    public enum Person { First, Second, Third };
    public enum Gender { M, F, C };
    public enum Number { S, D, P };
    public enum Tense { Preterite };
    public enum Case { Nom, Acc, Dat, Gen };

    public struct DecSpec {
        public Person? Person;
        public Gender? Gender;
        public Number? Number;
        public Tense? Tense;
        public Case? Case;
        public override string ToString() {
            return String.Join(" ", new object[] { Person, Number, Gender, Tense, Case }
                    .Where(x => x != null));
        }
    }
}
