namespace HRAttendanceMVC.Helpers;

public static class NumberToWordsHelper
{
    private static readonly string[] Ones =
    {
        "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
    };

    private static readonly string[] Tens =
    {
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    };

    public static string ToIndianRupeesWords(decimal amount)
    {
        if (amount == 0)
            return "Zero Rupees Only";

        if (amount < 0)
            return "Minus " + ToIndianRupeesWords(Math.Abs(amount));

        long rupees = (long)Math.Floor(amount);
        int paise = (int)Math.Round((amount - rupees) * 100);

        string words = ConvertWholeNumberToWords(rupees);
        if (string.IsNullOrWhiteSpace(words))
            words = "Zero";

        words = words.Trim() + " Rupees";

        if (paise > 0)
        {
            string paiseWords = ConvertWholeNumberToWords(paise);
            words += " and " + paiseWords.Trim() + " Paise";
        }

        return words + " Only";
    }

    private static string ConvertWholeNumberToWords(long number)
    {
        if (number == 0)
            return "";

        if (number < 20)
            return Ones[number] + " ";

        if (number < 100)
            return Tens[number / 10] + " " + Ones[number % 10] + " ";

        if (number < 1000)
            return Ones[number / 100] + " Hundred " + ConvertWholeNumberToWords(number % 100);

        if (number < 100000) // Thousands
            return ConvertWholeNumberToWords(number / 1000) + "Thousand " + ConvertWholeNumberToWords(number % 1000);

        if (number < 10000000) // Lakhs (1,00,000)
            return ConvertWholeNumberToWords(number / 100000) + "Lakh " + ConvertWholeNumberToWords(number % 100000);

        // Crores (1,00,00,000)
        return ConvertWholeNumberToWords(number / 10000000) + "Crore " + ConvertWholeNumberToWords(number % 10000000);
    }
}
