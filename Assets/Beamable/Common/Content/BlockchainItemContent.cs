using System;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Beamable.Common.Content.Contracts;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;

[Serializable]
public class BlockchainItemContentRef : ItemRef<BlockchainItemContent>
{
}

[ContentType("blockchain")]
[Serializable]
public class BlockchainItemContent : ItemContent, IBlockchainContent
{
    [MustReferenceContent] public ERC721ContractContentLink contract;

    public async Task<IContractTemplate> GetContractTemplate()
    {
        return await contract.Resolve();
    }
}