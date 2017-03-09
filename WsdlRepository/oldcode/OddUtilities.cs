using System;

namespace WsdlRepository.oldcode {

	public class OddUtilities {

        public static decimal RoundOdd(decimal odd) {
            return Math.Floor(odd * 20 + 0.5m) / 20;
        }

        public static decimal RoundOdd1Decimal(decimal odd) {
            if (odd >= 1.25m && odd < 2.0m) {
                return RoundOdd(odd);
            }
            else if (odd >= 2.0m) {
                return Math.Round(odd, 1);
            }
            else {
                return odd;
            }
        }

        public static decimal RoundOddLifeDecimal(decimal odd)
        {
            if (odd >= 1.4m)
            {
                return Math.Round(odd, 1);
            }
            else
            {
                return odd;
            }
        }



        public static String FormatWin(decimal pWin) {
            return String.Format("{0:F2}", pWin);
        }

        public static String ReplaceCommaPoint(String odd) {
            return odd.Replace(",", ".");
        }

		//static int[] Permutations;
		//static int id = -1;
		//static int visitNum = 0;
		//
		//public static void visit(int k) {
		//    id++;
		//    Permutations[k] = id;
		//    if (id == Permutations.Length)
		//        _logger.Info(" id="+ id+" k="+ k+"Permut="+Permutations[0]+Permutations[1]+Permutations[2]+Permutations[3]+" visit="+(++visitNum));
		//    else
		//        for (int t=0; t<Permutations.Length; t++) {
		//            if (Permutations[t] == 0)
		//                visit(t);
		//        }
		//    id--;
		//    Permutations[k] = 0;
		//}

		/// <summary>
		/// liefert ein array, in dem alle möglichen Kominationen als indizes enthalten sind
		/// z.B. 3(=combLength) aus 4(=numVal): 
		/// 0 1 2
		/// 0 1 3
		/// 0 2 3
		/// 1 2 3
		/// </summary>
		/// <param name="perms">array mit allen möglichkeiten</param>
		/// <param name="numVal">anzahl der Werte</param>
		/// <param name="combLength">Kombinationsmöglichkeiten</param>
		public static void SetPermutations(out int[,] perms,  int numVal, int combLength) {
			
			int size = (int)Math.Round(MathNet.Numerics.Fn.BinomialCoefficient(numVal, combLength), 0);
			perms = new int[size, combLength];
			for (int j=0; j<perms.GetLength(1); j++) {		//erste zeile beginnt immer mit: 0 1  2 ...
				perms[0, j] = j;
			}
			int remainder = 0;

			for (int i=1; i<perms.GetLength(0); i++) {
				int div = numVal;
				int j = perms.GetLength(1);
				while (--j >= 0)
					perms[i, j] = perms[i-1, j];		//default, letzte zeile kopieren

				j = perms.GetLength(1)-1;
				remainder = (perms[i-1, j] + 1) % div;
				if (remainder != 0) {
					perms[i, j] = perms[i-1, j]+1;		//default, nur letzten wert erhöhen					
				} else {
					while (remainder==0  &&  --j >=0) {	//überlauf, zurück bis kein überlauf mehr
						remainder = (perms[i-1, j] + 1) % (--div);						
					}
					if (j >= 0)								//j<0 --> absoluter überlauf --> Fehler
						perms[i, j] = perms[i-1, j]+1;		// wert erhöhen
					while (++j < perms.GetLength(1)  &&  j > 0) { // und nachfolger immer um (mind) eins höher
						perms[i, j] = perms[i, j-1]+1;
					}
				}
			}
		}


		/// <summary>
		/// erechnet Max Kombinaton für System X aus Y
		/// </summary>
		/// <param name="odds">Y Quoten @System X aus Y </param>
		/// <param name="comb">=X @System X aus Y</param>
		/// <returns></returns>
		public static decimal AllCombinationsSum(decimal[] odds, int comb) {
			decimal sum = 0;
			int[,] perms;
			if (odds.Length < 2  || odds.Length <= comb  ||  comb < 1)
				return sum;
			try {
				SetPermutations(out perms, odds.Length, comb);

				for (int i=0; i<perms.GetLength(0); i++) {
					decimal factor = 1;
					for (int j=0; j<perms.GetLength(1); j++) {
						factor *= odds[perms[i, j]];
					}
					sum += factor;
				}
				sum = sum / perms.GetLength(0);
				return sum;
			} catch (Exception e) {
				//TODO Berechnung auf Fehler prüfen
				return 0;
			}
		}

		/// <summary>
		/// errechnet minimalste mögliche Kombination
		/// </summary>
		/// <param name="odds">Y Quoten @System X aus Y </param>
		/// <param name="comb">=X @System X aus Y</param>
		/// <returns></returns>
		public static decimal MinCombinationsSum(decimal[] odds, int comb) {
			
			//int[,] perms;
			//if (odds.Length < 2  || odds.Length <= comb  ||  comb < 1)
			//    return 0;

			//SetPermutations(out perms, odds.Length, comb);


			//float minFactor = float.MaxValue;
			//for (int i=0; i<perms.GetLength(0); i++) {
			//    float factor = 1;
			//    for (int j=0; j<perms.GetLength(1); j++) {
			//        factor *= odds[perms[i, j]];
			//    }
			//    if (factor<minFactor)
			//        minFactor = factor;				
			//}
			
			//if (minFactor == float.MaxValue)
			//    return 0;

			//return minFactor / perms.GetLength(0);

			Array.Sort(odds);
			decimal factor = 1;
			for (int i=0; i<comb; i++)
				factor *= odds[i];
			return factor / (decimal) MathNet.Numerics.Fn.BinomialCoefficient(odds.Length, comb);

		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSystemBetOdds">decimal[] containing the values of the system bet odds</param>
        /// <param name="pOtherBetOdds">decimal[] containing the values of all other odds</param>
        /// <param name="pSystemX">int representing the SystemX used by the system bet</param>
        /// <returns>decimal value of the total odds</returns>
        public static decimal GetMaxTotalOdd(decimal[] pSystemBetOdds, decimal[] pOtherBetOdds, int pSystemX)
        {
            decimal tempMaxOdd = 1;
            if (pSystemBetOdds.Length != pSystemX)
            {
                tempMaxOdd = OddUtilities.AllCombinationsSum(pSystemBetOdds, pSystemX);
            }
            for (int k = 0; k < pOtherBetOdds.Length; k++)
            {
                tempMaxOdd *= pOtherBetOdds[k];
            }
            return RoundOdd(tempMaxOdd);
        }

        public static decimal CalculateOddGraduation(decimal odd, decimal graduation)
        {
            //GMA 07.07.2008 Abstufung berechnet
            decimal calcgraduation = 1;
            if (graduation == 0 || odd <= 1.2m)
            {
                return odd;
            }

            if (graduation < 0)
            {
                calcgraduation = calcgraduation + graduation;
            }
            else
            {
                calcgraduation = graduation;
            }
            odd = (odd * calcgraduation);      
            odd = Math.Round(odd,2);           
         
            return odd;
        }

        public static bool IsTwoWayOddFavourite(decimal value)
        {
            return value < 1.7m;
        }

        public static bool IsThreeWayOddFavourite(decimal value, int sort)
        {
            return (value < 2.5m && sort != 2);
        }

        public static decimal calculateGraduatedOddValue(decimal oddValue, decimal oddGraduation)
        {
            if (oddValue >= 1.15m && oddValue < 1.35m)
            {
                return oddValue + oddGraduation / 2;
            }
            else if (oddValue >= 1.35m)
            {
                return oddValue + oddGraduation;
            }
            return oddValue;
        }

    }
}
