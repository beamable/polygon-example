using System;
using Beamable.Common.Content;
using Beamable.Common.Content.Contracts;
using Beamable.Common.Content.Validation;
using UnityEngine;

[Serializable]
public class ERC20ContractContentRef : ContentRef<ERC20ContractContent>
{
}

[Serializable]
public class ERC20ContractContentLink : ContentLink<ERC20ContractContent>
{
}

[ContentType("ERC20_contracts")]
[Serializable]
public class ERC20ContractContent : ContentObject, IContractTemplate
{
    [TextArea(10, 100)] [CannotBeBlank] public string contractTemplate;

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

    public string GetTemplate()
    {
        return contractTemplate;
    }

    public ContractType GetContractType()
    {
        return ContractType.ERC20;
    }
}