using System;
using System.Collections.Generic;
using System.Linq;
public struct Glyph
{
    
    static readonly char[] vowels = new char[] { 'a', 'e', 'o', 'i', 'u' };

    public char Base { get; set; }
    public char? Diacritic { get; set; }

    public bool IsVowel => vowels.Contains(Base);
    public int Weight
    {
        get
        {
            switch (Diacritic ?? ' ')
            {
                case Constants.CIRCUMFLEX:
                    return 2;
                case Constants.MACRON:
                    return 1;
                default:
                    return 0;
            }
        }
    }

    public Glyph(string str) : this(str.ToList()) { }
    
    public Glyph(IList<char> str)  {
        switch (str.Count)
        {
            case 1:
                Base = str[0];
                Diacritic = null;
                break;
            case 2:
                Base = str[0];
                Diacritic = str[1];
                break;
            default:
                throw new ArgumentException($"{str} has invalid length {str.Count}");
        }
    }
 
    public override string ToString() => Base.ToString() + Diacritic ?? "";
 
}