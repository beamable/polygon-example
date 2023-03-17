namespace Beamable.Microservices.PolygonFederation.Features.Minting.Storage.Models
{
    public record TokenIdMapping
    {
        public string ContentId { get; set; }
        public uint TokenId { get; set; }
    }
}