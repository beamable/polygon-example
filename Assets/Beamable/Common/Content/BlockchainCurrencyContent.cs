using System;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Beamable.Common.Content.Contracts;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;

[Serializable]
public class BlockchainCurrencyContentRef : CurrencyRef<BlockchainCurrencyContent>
{
}

[ContentType("blockchain")]
[Serializable]
public class BlockchainCurrencyContent : CurrencyContent, IBlockchainContent
{
    [MustReferenceContent] public ERC20ContractContentLink contract;

    public async Task<IContractTemplate> GetContractTemplate()
    {
        return await contract.Resolve();
    }
}