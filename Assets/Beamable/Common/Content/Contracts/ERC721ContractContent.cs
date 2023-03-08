using System;
using Beamable.Common.Content;
using Beamable.Common.Content.Contracts;
using Beamable.Common.Content.Validation;
using UnityEngine;

[Serializable]
public class ERC721ContractContentRef : ContentRef<ERC721ContractContent>
{
}

[Serializable]
public class ERC721ContractContentLink : ContentLink<ERC721ContractContent>
{
}

[ContentType("ERC721_contracts")]
[Serializable]
public class ERC721ContractContent : ContentObject, IContractTemplate
{
    [TextArea(10, 100)] [CannotBeBlank] public string contractTemplate;

    public ERC721ContractContent()
    {
        contractTemplate = @"
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

import ""@openzeppelin/contracts/token/ERC721/ERC721.sol"";
import ""@openzeppelin/contracts/access/Ownable.sol"";

contract DefaultContract is ERC721, Ownable {
    constructor() ERC721(""{{ContentId}}"", ""ITS"") {}

    function _baseURI() internal pure override returns (string memory) {
        return ""{{Properties.ExternalUri}}"";
    }

    function safeMint(address to, uint256 tokenId) public onlyOwner {
        _safeMint(to, tokenId);
    }
}
".Trim();
    }

    public string GetTemplate()
    {
        return contractTemplate;
    }

    public ContractType GetContractType()
    {
        return ContractType.ERC721;
    }
}