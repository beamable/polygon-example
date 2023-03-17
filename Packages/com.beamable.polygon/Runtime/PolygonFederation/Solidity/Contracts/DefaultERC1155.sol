// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

import "@openzeppelin/contracts/token/ERC1155/ERC1155.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/token/ERC1155/extensions/ERC1155Supply.sol";

contract GameToken is ERC1155, ERC1155Supply, Ownable {
    mapping(uint256 => string) private _metadata;
    mapping(address => uint256[]) private _tokensPerAddress;
    mapping(address => mapping(uint256 => bool)) private _tokensPerAddressPresence;

    string private _baseURI;

    constructor() ERC1155("") {}

    function setURI(string memory newUri) public onlyOwner {
        _baseURI = newUri;
    }

    function mint(
        address to,
        uint256 tokenId,
        uint256 amount,
        string memory metadataHash
    ) public onlyOwner {
        _mint(to, tokenId, amount, "");
        _metadata[tokenId] = metadataHash;
    }

    function batchMint(
        address to,
        uint256[] memory tokenIds,
        uint256[] memory amounts,
        string[] memory metadataHashes
    ) external onlyOwner {
        require(
            tokenIds.length == amounts.length,
            "ERC1155: tokenIds and amounts length mismatch"
        );
        require(
            tokenIds.length == metadataHashes.length,
            "ERC1155: tokenIds and metadataHashes length mismatch"
        );

        for (uint256 i = 0; i < tokenIds.length; i++) {
            mint(to, tokenIds[i], amounts[i], metadataHashes[i]);
        }
    }

    function uri(uint256 tokenId) public view override returns (string memory) {
        string memory metadata = _metadata[tokenId];
        return bytes(metadata).length > 0 ? string(abi.encodePacked(_baseURI, metadata)) : super.uri(tokenId);
    }

    function getInventory(address account)
    public
    view
    returns (uint256[] memory tokenIds, uint256[] memory tokenAmounts, string[] memory metadataHashes)
    {
        uint256[] memory ids = _tokensPerAddress[account];
        uint256[] memory amounts = new uint256[](ids.length);
        string[] memory metadata = new string[](ids.length);
        for (uint256 i = 0; i < ids.length; i++) {
            uint256 tokenId = ids[i];
            amounts[i] = balanceOf(account, tokenId);
            metadata[i] = _metadata[tokenId];
        }
        return (ids, amounts, metadata);
    }

    function _beforeTokenTransfer(address operator, address from, address to, uint256[] memory ids, uint256[] memory amounts, bytes memory data)
    internal
    override(ERC1155, ERC1155Supply)
    {
        super._beforeTokenTransfer(operator, from, to, ids, amounts, data);
    }

    function _afterTokenTransfer(
        address operator,
        address from,
        address to,
        uint256[] memory ids,
        uint256[] memory amounts,
        bytes memory data
    ) internal virtual override {
        for (uint256 i = 0; i < ids.length; i++) {
            uint256 tokenId = ids[i];
            if (!_tokensPerAddressPresence[to][tokenId]) {
                _tokensPerAddress[to].push(tokenId);
                _tokensPerAddressPresence[to][tokenId] = true;
            }
            if (from != address(0) && balanceOf(from, tokenId) == 0) {
                _tokensPerAddressPresence[from][tokenId] = false;
            }
        }
        super._afterTokenTransfer(operator, from, to, ids, amounts, data);
    }
}
