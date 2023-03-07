namespace Beamable.Common.Content.Contracts
{
    public interface IContractContent
    {
        string GetTemplate();
        ContractType GetContractType();
    }
}