using System;
using System.Security.Cryptography;
using System.Text;

namespace SportBetting.WPF.Prism.OldCode
{
	/// <summary>
    /// Klasse zum generieren von Random Passwörtern.
    /// </summary>
    public class PasswordGenerator
    {
        public PasswordGenerator() 
        {
            this.ConsecutiveCharacters 		= false;
            this.RepeatCharacters 			= true;
            this.ExcludeSymbols             = false;
            this.Exclusions                 = null;

            rng = new RNGCryptoServiceProvider();
        }		
		
        protected int GetCryptographicRandomNumber(int lBound, int uBound)
        {   
            // Assumes lBound >= 0 && lBound < uBound
            // returns an int >= lBound and < uBound
            uint urndnum;   
            byte[] rndnum = new Byte[4];   
            if (lBound == uBound)  
            {
                // test for degenerate case where only lBound can be returned   
                return lBound;
            }
                                                              
            uint xcludeRndBase = (uint.MaxValue - (uint.MaxValue%(uint)(uBound-lBound)));   
            
            do 
            {      
                rng.GetBytes(rndnum);      
                urndnum = System.BitConverter.ToUInt32(rndnum,0);      
            } while (urndnum >= xcludeRndBase);   
            
            return (int)(urndnum % (uBound-lBound)) + lBound;
        }

        protected char GetRandomCharacter(bool NumbersOnly)
        {
            int upperBound;

            if (NumbersOnly)
            {
                upperBound = pwdCharArrayNumber.GetUpperBound(0);
            }
            else
            {
                upperBound = pwdCharArray.GetUpperBound(0);
            }

            if ( true == this.ExcludeSymbols )
            {
                upperBound = PasswordGenerator.UBoundDigit;
            }

            int randomCharPosition;
            char randomChar;

            if (NumbersOnly)
            {
                randomCharPosition = GetCryptographicRandomNumber(pwdCharArrayNumber.GetLowerBound(0), upperBound);
                randomChar = pwdCharArrayNumber[randomCharPosition];
            }
            else
            {
                randomCharPosition = GetCryptographicRandomNumber(pwdCharArray.GetLowerBound(0), upperBound);
                randomChar = pwdCharArray[randomCharPosition];
            }

            

            return randomChar;
        }
        public static string GenerateStatic(int PWMinLeng, int PWMaxLeng, bool NumbersOnly)
        {
            PasswordGenerator PWG = new PasswordGenerator();
            return PWG.Generate(PWMinLeng, PWMaxLeng, NumbersOnly);
        }

        /// <summary>
        /// Methode generiert ein andom Passwort. Als Input muss die Minimal und Maximal-Länge des Passworts angegeben werden
        /// </summary>
        /// <param name="PWMinLeng"></param>
        /// <param name="PWMaxLeng"></param>
        /// <returns>string = Random Passwort</returns>
        public string Generate(int PWMinLeng, int PWMaxLeng, bool NumbersOnly)
        {
            this.Minimum = PWMinLeng;
            this.Maximum = PWMaxLeng;

            // Pick random length between minimum and maximum   
            int pwdLength = GetCryptographicRandomNumber(this.Minimum, this.Maximum);

            StringBuilder pwdBuffer = new StringBuilder();
            pwdBuffer.Capacity = this.Maximum;

            // Generate random characters
            char lastCharacter, nextCharacter;

            // Initial dummy character flag
            lastCharacter = nextCharacter = '\n';

            for ( int i = 0; i < pwdLength; i++ )
            {
                nextCharacter = GetRandomCharacter(NumbersOnly);

                if ( false == this.ConsecutiveCharacters )
                {
                    while ( lastCharacter == nextCharacter )
                    {
                        nextCharacter = GetRandomCharacter(NumbersOnly);
                    }
                }

                if ( false == this.RepeatCharacters )
                {
                    string temp = pwdBuffer.ToString();
                    int duplicateIndex = temp.IndexOf(nextCharacter);
                    while ( -1 != duplicateIndex )
                    {
                        nextCharacter = GetRandomCharacter(NumbersOnly);
                        duplicateIndex = temp.IndexOf(nextCharacter);
                    }
                }

                if ( ( null != this.Exclusions ) )
                {
                    while ( -1 != this.Exclusions.IndexOf(nextCharacter) )
                    {
                        nextCharacter = GetRandomCharacter(NumbersOnly);
                    }
                }

                pwdBuffer.Append(nextCharacter);
                lastCharacter = nextCharacter;
            }

            if ( null != pwdBuffer )
            {
                return pwdBuffer.ToString();
            }
            else
            {
                return String.Empty;
            }	
        }
            
        public string Exclusions
        {
            get { return this.exclusionSet;  }
            set { this.exclusionSet = value; }
        }

        public int Minimum
        {
            get { return this.minSize; }
            set	
            { 
                this.minSize = value;
            }
        }

        public int Maximum
        {
            get { return this.maxSize; }
            set	
            { 
                this.maxSize = value;
            }
        }

        public bool ExcludeSymbols
        {
            get { return this.hasSymbols; }
            set	{ this.hasSymbols = value;}
        }

        public bool RepeatCharacters
        {
            get { return this.hasRepeating; }
            set	{ this.hasRepeating = value;}
        }

        public bool ConsecutiveCharacters
        {
            get { return this.hasConsecutive; }
            set	{ this.hasConsecutive = value;}
        }

        private const int UBoundDigit    = 61;

        private RNGCryptoServiceProvider    rng;
        private int 			minSize;
        private int 			maxSize;
        private bool			hasRepeating;
        private bool			hasConsecutive;
        private bool            hasSymbols;
        private string          exclusionSet;
        private char[] pwdCharArray = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private char[] pwdCharArrayNumber = "0123456789".ToCharArray();                                
    }
}