using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace XSWHC.NHSNoExtensions
{
    public static class NHSNoExtensions
    {

        #region Validation rules from DOH

        //        NHS Number Check Digit Calculation
        //The NHS number comprises 10 digits. The first nine are the identifier and the tenth is a check
        //digit used to confirm the number's validity. The check digit is calculated using the Modulus 11
        //algorithm. There are four steps in the calculation:
        //• Step 1 - multiply each of the first nine digits by a weighting factor as follows;
        //Digit Position
        //(Starting from the left)
        //Factor
        //1 10
        //2 9
        //3 8
        //4 7
        //5 6
        //6 5
        //7 4
        //8 3
        //9 2
        //•
        //• Step 2 - add the results of each multiplication together
        //• Step 3 - divide the total by 11 and establish the remainder
        //• Step 4 - subtract the remainder from 11 to give the check digit
        //There are two occasions where the check digit calculation process must be modified slightly:
        //• if the result of step 4 is 11 then a check digit of 0 is used
        //• if the result of step 4 is 10 then the number is invalid and not used.
        //Example: Suppose the first nine digits of the number are 401 023 213
        //• Step 1 - apply weighting factors
        //Digit Position Value Factor Result
        //1 4 x 10 = 40
        //2 0 x 9 = 0
        //3 1 x 8 = 8
        //4 0 x 7 = 0
        //5 2 x 6 = 12
        //6 3 x 5 = 15
        //7 2 x 4 = 8
        //8 1 x 3 = 3
        //9 3 x 2 = 6
        //•
        //• Step 2 - add the results of each multiplication together
        //40 + 0 + 8 + 0 + 12 + 15 + 8 + 3 + 6 = 92
        //• Step 3 - divide the total by 11
        //92 ÷ 11 = 8, remainder 4
        //• Step 4 - subtract the remainder from 11 to give the check digit
        //11 - 4 = 7
        //The complete NHS Number in this example is therefore: 401 023 2137
        //Please note there are some restrictions
        //Constant number strings such as 444 444 4444, 666 666 6666 etc are not issued.
        //So, although they pass the checksum, they are invalid.
        //The reason for excluding numbers based on a single repeating digit is to safeguard the
        //integrity of the numbering system and prevent potential mistaken use or abuse.





        #endregion


        public static bool ValidateNHSNo(this string NHSnumber)
        {
            if (NHSnumber == null)
            {
                throw new ArgumentNullException();
            }


            //default to invalid
            bool isValid = false;
            Char[] digits = NHSnumber.ToCharArray();



            //initial checks
            if (digits.Length != 10)
            {
                isValid = false;
                return isValid;
            }

            //are they numbers
            NumberValidation(digits);


            //check that they are not all the same numbers 
            //(this may pass the checksum test but they are not valid
            //NHS nos
            if (AreConstantNumbers(digits))
            {
                isValid = false;
                return isValid;
            }


            if (CheckSumTest(digits))
            {
                isValid = true;
                return isValid;
            }



            return isValid;

        }

        /// <summary>
        /// Are the Chars all numbers
        /// </summary>
        /// <param name="digits">The digits.</param>
        private static void NumberValidation(char[] digits)
        {
            foreach (char item in digits)
            {
                int intResult;
                if (!int.TryParse(item.ToString(), out intResult))
                {
                    throw new ArgumentException("NHS numbers must contain numbers only");
                }
            }
        }

        /// <summary>
        /// Checks against Checksum Logic
        /// </summary>
        /// <param name="digits">The digits.</param>
        /// <returns></returns>
        private static bool CheckSumTest(char[] digits)
        {
            //only use the first 9 values for checksum calculation
            int Checksum = Convert.ToInt32(digits[9].ToString());

            int[] digitTotals = new int[9];
            int[] Factors = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            for (int i = 0; i < digits.Length - 1; i++)
            {
                digitTotals[i] = Convert.ToInt32(digits[i].ToString()) * Factors[i];
            }

            int ComputedCheckSum = 11 - (digitTotals.Sum() % 11);
            ComputedCheckSum = (ComputedCheckSum == 11) ? 0 : ComputedCheckSum;
            if (ComputedCheckSum == 10)
            {
                //?? What do we do??? Guidance is not clear. for now we'll throw them out.
                return false;
            }
            else
            {
                return (Checksum == ComputedCheckSum);

            }

        }

        /// <summary>
        /// Are they constant numbers.
        /// </summary>
        /// <param name="digits">The digits.</param>
        /// <returns></returns>
        private static bool AreConstantNumbers(char[] digits)
        {

            char FirstNo = digits[0];
            for (int i = 1; i < digits.Length; i++)
            {
                if (FirstNo != digits[i])
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Formats the given string in the 3-3-4 format if the number is formattable
        /// </summary>
        /// <param name="NHSnumber">The NHS number.</param>
        /// <returns></returns>
        public static string FormatNHSNo(this string NHSnumber)
        {

            if (NHSnumber == null)
            {
                throw new ArgumentNullException();
            }
            //if the string is 10 chars long then apply the formatting
            if (NHSnumber.Length == 10)
            {
                //call number validation
                NumberValidation(NHSnumber.ToCharArray());               

                return string.Format("{0} {1} {2}", NHSnumber.Substring(0, 3), NHSnumber.Substring(3, 3), NHSnumber.Substring(6, 4));
            }
            return NHSnumber;
        }


        /// <summary>
        /// Strips the NHS no formatting if the passed in string is an NHS formatted string
        /// </summary>
        /// <param name="NHSnumber">The NH snumber.</param>
        /// <returns></returns>
        public static string StripNHSNoFormatting(this string NHSnumber)
        {

            if (NHSnumber == null)
            {
                throw new ArgumentNullException();
            }
            //strip the spaces

            if (isFormatted(NHSnumber))
            {

                return NHSnumber.Replace(" ", "");
            }
            else
            {

                return NHSnumber;
            }



        }



        /// <summary>
        /// Determines whether the given NHS no is formatted
        /// </summary>
        /// <param name="NHSnumber">The NHS number.</param>
        /// <returns>
        /// 	<c>true</c> if [is NHS formatted] [the specified NH snumber]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNHSFormatted(this string NHSnumber)
        {

            return isFormatted(NHSnumber);
        }




        /// <summary>
        /// Determines whether the specified NH snumber is formatted.
        /// </summary>
        /// <param name="NHSnumber">The NH snumber.</param>
        /// <returns>
        /// 	<c>true</c> if the specified NH snumber is formatted; otherwise, <c>false</c>.
        /// </returns>
        private static bool isFormatted(String NHSnumber)
        {

            bool isFormatted = false;


            if (NHSnumber == null)
            {
                throw new ArgumentNullException();
            }

            //is the number 12chars long
            if (NHSnumber.Length == 12)
            {
                MatchCollection matches = Regex.Matches(NHSnumber.Trim(), " ");

                //find two spaces
                if (matches.Count == 2)
                {
                    //are the spaces at index 4 and 8 (3 and 7 in zero indexed string)
                    if (matches[0].Index == 3 && matches[1].Index == 7)
                    {

                        isFormatted = true;

                    }
                }

            }
            return isFormatted;
        }


    }
}
