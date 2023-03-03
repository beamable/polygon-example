using System;
using Beamable.Common.Content;

[Serializable]
public class ERC721ContractContentRef : SolidityContractContentRef<ERC721ContractContent>
{
}

[ContentType("ERC721")]
[Serializable]
public class ERC721ContractContent : SolidityContractContent
{
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
}