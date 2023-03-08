namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models
{
    public record MintMapping : Contract
    {
        public string OwnerAddress { get; set; }
    }
}