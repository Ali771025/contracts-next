namespace Contracts.Domain.Contracts;

public static class MoneyConverter
{
    public static long ToRial(long amountMinor, int currencyScale, long exchangeRateRialPerUnit)
    {
        Guard.NonNegativeAmount(amountMinor, nameof(amountMinor));
        Guard.PositiveAmount(exchangeRateRialPerUnit, nameof(exchangeRateRialPerUnit));
        if (currencyScale is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(currencyScale));
        }

        BigInteger divisor = BigInteger.Pow(10, currencyScale);
        BigInteger numerator = (BigInteger)amountMinor * exchangeRateRialPerUnit;
        BigInteger quotient = BigInteger.DivRem(numerator, divisor, out BigInteger remainder);

        if (remainder * 2 >= divisor)
        {
            quotient += BigInteger.One;
        }

        if (quotient > long.MaxValue)
        {
            throw new OverflowException("Converted rial amount exceeds Int64 capacity.");
        }

        return (long)quotient;
    }
}
