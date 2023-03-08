namespace Beamable.Common.Content.Contracts
{
    public interface IContractTemplate
    {
        string GetTemplate();
        ContractType GetContractType();
    }
}