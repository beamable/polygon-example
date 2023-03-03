using System;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using UnityEngine;

[Serializable]
public class SolidityContractContentRef<TContent> : ContentRef<TContent> where TContent : SolidityContractContent, new() { }

[Serializable]
public class SolidityContractContentLink : ContentLink<SolidityContractContent> { }

[ContentType("solidity_contracts")]
[Serializable]
public class SolidityContractContent : ContentObject
{
    [TextArea(10, 100)]
    [CannotBeBlank]
    public string contractTemplate;
}