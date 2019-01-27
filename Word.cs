using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;

class Word
{
    private string _Text;
    public string Text { get {return _Text;} set {_Text = value.ToLower().Normalize(NormalizationForm.FormD);} }
    public string Meaning { get; set; }
    public PoS PoS { get; set; }
	[Name("Gender")]
    public Gender?  NounGender  { get; set; }
    public int Chapter{ get; set; }
    public string ThemeVowel {get; set; }

	public string BaseForm => Text.EndsWith("um")? Text.Substring(0, Text.LastIndexOf("um")) : Text;

    private char[] vowels = new char[]{'a','e','o','i','u'};

    /// <summary>
    /// Groups the deconstructed characters by resultant glyph.
    /// That is, puts all the utf chars that combine into a single letter, into their own string.
    /// e.g. (ASCIIfied) "fo`o" -> ["f", "o`", "o"]
    /// </summary>
    public IEnumerable<string> Glyphs =>
        BaseForm.ChunkOn(cur => CharUnicodeInfo.GetUnicodeCategory(cur) != UnicodeCategory.NonSpacingMark )
            .Select(x=>String.Join("",x));

    public IEnumerable<string> Consonants =>
		Glyphs.Where(x=>!vowels.Contains(x[0]));

        

	public ValueTuple <DecSpec, string>[] Decline() {
		var res = new List<ValueTuple<DecSpec, string>>();
		var none = new DecSpec{};

		var baseForm = BaseForm;

		switch (PoS) {
			case PoS.Noun:
				switch (this.NounGender) {
					case Gender.M:
					default:
						res.Add((new DecSpec {Number=Number.S, Case=Case.Nom}, baseForm + "um"));
						res.Add((new DecSpec {Number=Number.S, Case=Case.Gen}, baseForm + "im"));
						res.Add((new DecSpec {Number=Number.S, Case=Case.Acc}, baseForm + "am"));

						res.Add((new DecSpec {Number=Number.P, Case=Case.Nom}, baseForm + "u" + Constants.MACRON));
						res.Add((new DecSpec {Number=Number.P, Case=Case.Gen}, baseForm + "i" + Constants.MACRON));
						res.Add((new DecSpec {Number=Number.P, Case=Case.Acc}, baseForm + "i" + Constants.MACRON));

						break;
					case Gender.F:
						res.Add((new DecSpec {Number=Number.S, Case=Case.Nom}, baseForm + "um"));
						res.Add((new DecSpec {Number=Number.S, Case=Case.Gen}, baseForm + "im"));
						res.Add((new DecSpec {Number=Number.S, Case=Case.Acc}, baseForm + "am"));

						res.Add((new DecSpec {Number=Number.P, Case=Case.Nom}, baseForm.Remove(baseForm.Length-1) + "a" + Constants.MACRON + "tum"));
						res.Add((new DecSpec {Number=Number.P, Case=Case.Gen}, baseForm.Remove(baseForm.Length-1) + "a" + Constants.MACRON + "tim"));
						res.Add((new DecSpec {Number=Number.P, Case=Case.Acc}, baseForm.Remove(baseForm.Length-1) + "a" + Constants.MACRON + "tim"));

						break;
				}
				break;
            case PoS.Verb:
                List<string> verbRoot_l = Consonants.ToList();
                verbRoot_l.Insert(2, ThemeVowel);
                string verbRoot = String.Join("", verbRoot_l);
                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.Third, Number=Number.S, Gender=Gender.C}, "i" + verbRoot));
                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.Second, Number=Number.S, Gender=Gender.M}, "ta" + verbRoot));
                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.Second, Number=Number.S, Gender=Gender.F}, "ta" + verbRoot + "i" + Constants.MACRON));
                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.First, Number=Number.S, Gender=Gender.C}, "a" + verbRoot));

                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.Third, Number=Number.P, Gender=Gender.M}, "i" + verbRoot + "u" + Constants.MACRON));
                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.Third, Number=Number.P, Gender=Gender.F}, "i" + verbRoot + "a" + Constants.MACRON));
                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.Second, Number=Number.P, Gender=Gender.C}, "ta" + verbRoot + "a" + Constants.MACRON));
                res.Add((new DecSpec {Tense=Tense.Preterite, Person=Person.First, Number=Number.P, Gender=Gender.C}, "ni" + verbRoot));
                break;
            case PoS.Adjective:
                    // TODO: create female baseForm and apply vowel syncope

                    res.Add((new DecSpec {Gender=Gender.M, Number=Number.S, Case=Case.Nom}, baseForm + "um"));
                    res.Add((new DecSpec {Gender=Gender.M, Number=Number.S, Case=Case.Gen}, baseForm + "im"));
                    res.Add((new DecSpec {Gender=Gender.M, Number=Number.S, Case=Case.Acc}, baseForm + "am"));

                    res.Add((new DecSpec {Gender=Gender.M, Number=Number.P, Case=Case.Nom}, baseForm + "u" + Constants.MACRON + "tum"));
                    res.Add((new DecSpec {Gender=Gender.M, Number=Number.P, Case=Case.Gen}, baseForm + "u" + Constants.MACRON + "tim"));
                    res.Add((new DecSpec {Gender=Gender.M, Number=Number.P, Case=Case.Acc}, baseForm + "u" + Constants.MACRON + "tim"));

                    res.Add((new DecSpec {Gender=Gender.F, Number=Number.S, Case=Case.Nom}, baseForm + "um"));
                    res.Add((new DecSpec {Gender=Gender.F, Number=Number.S, Case=Case.Gen}, baseForm + "im"));
                    res.Add((new DecSpec {Gender=Gender.F, Number=Number.S, Case=Case.Acc}, baseForm + "am"));

                    res.Add((new DecSpec {Gender=Gender.F, Number=Number.P, Case=Case.Nom}, baseForm.Remove(baseForm.Length-1) + "a" + Constants.MACRON + "tum"));
                    res.Add((new DecSpec {Gender=Gender.F, Number=Number.P, Case=Case.Gen}, baseForm.Remove(baseForm.Length-1) + "a" + Constants.MACRON + "tim"));
                    res.Add((new DecSpec {Gender=Gender.F, Number=Number.P, Case=Case.Acc}, baseForm.Remove(baseForm.Length-1) + "a" + Constants.MACRON + "tim"));

                    break;
			default:
				break;
		}
		return res.ToArray();
	}


}

public enum PoS {Noun, Verb, Adjective, Conjunction, Preposition, Adverb};
public enum Person { First, Second, Third };
public enum Gender { M, F, C };
public enum Number {S, D, P};
public enum Tense {Preterite};
public enum Case {Nom, Acc, Dat, Gen};

public struct DecSpec {
    public Person? Person;
    public Gender? Gender;
    public Number? Number;
    public Tense? Tense;
    public Case? Case;
	public override string ToString() {
        return String.Join(" ",new object[] {Person, Number, Gender, Tense, Case}
                .Where(x => x != null));
	}
}
