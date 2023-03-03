using System;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;

[Serializable]
public class BlockchainItemContentRef : ItemRef<BlockchainItemContent> { }

[ContentType("blockchain_items")]
[Serializable]
public class BlockchainItemContent : ItemContent
{
    [MustReferenceContent]
    public SolidityContractContentLink contract;
}