using System;
using Beamable.Common.Content;

[Serializable]
public class ERC20ContractContentRef : SolidityContractContentRef<ERC20ContractContent>
{
}

[ContentType("ERC20")]
[Serializable]
public class ERC20ContractContent : SolidityContractContent
{
    public ERC20ContractContent()
    {
        contractTemplate = @"
// SPDX-License-Identifier: MIT
pragma solidity ^0.8.9;

import ""@openzeppelin/contracts/token/ERC20/ERC20.sol"";
import ""@openzeppelin/contracts/access/Ownable.sol"";

contract DefaultContract is ERC20, Ownable {
    constructor() ERC20(""{{ContentId}}"", ""BMC"") {}

    function mint(address to, uint256 amount) public onlyOwner {
        _mint(to, amount);
    }
}
".Trim();
    }
}