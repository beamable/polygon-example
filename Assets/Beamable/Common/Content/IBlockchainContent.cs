using System.Threading.Tasks;
using Beamable.Common.Content.Contracts;

public interface IBlockchainContent
{
    Task<IContractTemplate> GetContractTemplate();
}