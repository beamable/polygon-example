// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

import "@openzeppelin/contracts/token/ERC1155/ERC1155.sol";
import "@openzeppelin/contracts/access/Ownable.sol";

// TODO: use hooks to manage the custom mappings

contract GameToken is ERC1155, Ownable {
    mapping(uint256 => string) private _metadata;

    mapping(address => uint256[]) private _tokensPerAddress;
    mapping(address => mapping(uint256 => bool))
    private _tokensPerAddressPresence;

    string private _uri;

    constructor() ERC1155("") {}

    function setURI(string memory newUri) public onlyOwner {
        _uri = newUri;
    }

    function mint(
        address account,
        uint256 id,
        uint256 amount,
        string memory metadataHash
    ) public onlyOwner {
        _mint(account, id, amount, "");
        _metadata[id] = metadataHash;

        if (_tokensPerAddressPresence[account][id] != true) {
            _tokensPerAddress[account].push(id);
            _tokensPerAddressPresence[account][id] = true;
        }
    }

    function batchMint(
        address to,
        uint256[] memory tokenIds,
        uint256[] memory amounts,
        string[] memory metadataHashes
    ) external onlyOwner {
        require(
            tokenIds.length == amounts.length,
            "tokenIds and amounts length mismatch"
        );
        require(
            tokenIds.length == metadataHashes.length,
            "tokenIds and metadataHashes length mismatch"
        );

        for (uint256 i = 0; i < tokenIds.length; i++) {
            mint(to, tokenIds[i], amounts[i], metadataHashes[i]);
        }
    }

    function uri(uint256 tokenid)
    public
    view
    override
    returns (string memory)
    {
        return string(abi.encodePacked(_uri, _metadata[tokenid], ".json"));
    }

    function getInventory(address account)
    public
    view
    returns (
        uint256[] memory,
        uint256[] memory
    )
    {
        uint256[] memory tokenIds = _tokensPerAddress[account];
        uint256[] memory tokenAmounts = new uint256[](tokenIds.length);
        for (uint256 i = 0; i < tokenIds.length; i++) {
            uint256 tokenId = tokenIds[i];
            tokenAmounts[i] = balanceOf(account, tokenId);
        }
        return (tokenIds, tokenAmounts);
    }
}