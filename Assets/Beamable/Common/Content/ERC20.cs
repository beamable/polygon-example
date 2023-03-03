using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;
using UnityEngine;

[System.Serializable]
public class ERC20Ref : CurrencyRef<ERC20> { }

[ContentType("ERC20")]
[System.Serializable]
public class ERC20 : CurrencyContent
{
    [TextArea(10, 100)]
    [CannotBeBlank]
    public string smartContractTemplate = @"
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

