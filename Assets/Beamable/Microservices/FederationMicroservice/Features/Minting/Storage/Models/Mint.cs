namespace Beamable.Microservices.FederationMicroservice.Features.Minting.Storage.Models
{
    public record Mint : Contract
    {
        public string OwnerAddress { get; set; }
    }
}