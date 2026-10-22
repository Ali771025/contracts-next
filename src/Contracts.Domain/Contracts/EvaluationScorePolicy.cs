namespace Contracts.Domain.Contracts;

public static class EvaluationScorePolicy
{
    public static decimal Total(int quality,int time,int financial,int hse)=>
        Math.Round((quality+time+financial+hse)/4m,1,MidpointRounding.AwayFromZero);
}
