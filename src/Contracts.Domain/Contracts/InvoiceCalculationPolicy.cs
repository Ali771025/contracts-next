namespace Contracts.Domain.Contracts;

public static class InvoiceCalculationPolicy
{
    public static long Total(long amountRial,long vatAmountRial)=>checked(amountRial+vatAmountRial);
}
