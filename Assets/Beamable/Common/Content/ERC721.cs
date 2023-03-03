using Beamable;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;
using UnityEngine;

[System.Serializable]
public class ERC721Ref : ItemRef<ERC721> { }

[ContentType("ERC721")]
[System.Serializable]
public class ERC721 : ItemContent
{
    [TextArea(10, 100)]
    [CannotBeBlank]
    public string smartContractTemplate = @"
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

