using System;
using MathNet.Numerics;

namespace WsdlRepository.Utils
{
    public  static class OddMath
    {
        public static decimal AllCombinationsSum(decimal[] odds, int comb)
        {
            decimal sum = 0;
            int[,] perms;
            if (odds.Length < 2 || odds.Length <= comb || comb < 1)
                return sum;
            try
            {
                SetPermutations(out perms, odds.Length, comb);

                for (int i = 0; i < perms.GetLength(0); i++)
                {
                    decimal factor = 1;
                    for (int j = 0; j < perms.GetLength(1); j++)
                    {
                        factor *= odds[perms[i, j]];
                    }
                    sum += factor;
                }
                sum = sum / perms.GetLength(0);
                return sum;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        public static void SetPermutations(out int[,] perms, int numVal, int combLength)
        {

            int size = (int)Math.Round(Fn.BinomialCoefficient(numVal, combLength), 0);
            perms = new int[size, combLength];
            for (int j = 0; j < perms.GetLength(1); j++)
            {		//erste zeile beginnt immer mit: 0 1  2 ...
                perms[0, j] = j;
            }
            int remainder = 0;

            for (int i = 1; i < perms.GetLength(0); i++)
            {
                int div = numVal;
                int j = perms.GetLength(1);
                while (--j >= 0)
                    perms[i, j] = perms[i - 1, j];		//default, letzte zeile kopieren

                j = perms.GetLength(1) - 1;
                remainder = (perms[i - 1, j] + 1) % div;
                if (remainder != 0)
                {
                    perms[i, j] = perms[i - 1, j] + 1;		//default, nur letzten wert erhöhen					
                }
                else
                {
                    while (remainder == 0 && --j >= 0)
                    {	//überlauf, zurück bis kein überlauf mehr
                        remainder = (perms[i - 1, j] + 1) % (--div);
                    }
                    if (j >= 0)								//j<0 --> absoluter überlauf --> Fehler
                        perms[i, j] = perms[i - 1, j] + 1;		// wert erhöhen
                    while (++j < perms.GetLength(1) && j > 0)
                    { // und nachfolger immer um (mind) eins höher
                        perms[i, j] = perms[i, j - 1] + 1;
                    }
                }
            }
        }
    }
}