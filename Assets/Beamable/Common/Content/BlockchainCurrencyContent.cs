using System;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;

[Serializable]
public class BlockchainCurrencyContentRef : CurrencyRef<BlockchainCurrencyContent> { }

[ContentType("blockchain_currency")]
[Serializable]
public class BlockchainCurrencyContent : CurrencyContent
{
    [MustReferenceContent]
    public SolidityContractContentLink contract;
}