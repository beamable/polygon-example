using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Newtonsoft.Json;

namespace Beamable.Microservices.FederationMicroservice.Features.Minting
{
    internal static class NtfExternalMetadataService
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task<string> SaveExternalMetadata(NftExternalMetadata metadata)
        {
            var metadataJsonString = JsonConvert.SerializeObject(metadata);
            var metadataPayload = Encoding.UTF8.GetBytes(metadataJsonString);

            using (var md5 = MD5.Create())
            {
                var md5Bytes = md5.ComputeHash(metadataPayload);
                var payloadChecksum = BitConverter.ToString(md5Bytes).Replace("-", "");

                var saveBinaryResponse = await ServiceContext.Requester.Request<SaveBinaryResponse>(Method.POST,
                    "/basic/content/binary", new SaveBinaryRequest
                    {
                        binary = new List<BinaryDefinition>
                        {
                            new()
                            {
                                id = "metadata",
                                checksum = payloadChecksum,
                                uploadContentType = "text/plain"
                            }
                        }
                    });

                var binaryResponse = saveBinaryResponse.binary.First();
                var signedUrl = binaryResponse.uploadUri;

                var content = new ByteArrayContent(metadataPayload);
                content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                content.Headers.ContentMD5 = md5Bytes;

                var putContentResponse = await HttpClient.PutAsync(signedUrl, content);

                putContentResponse.EnsureSuccessStatusCode();

                return binaryResponse.uri;
            }
        }

        public static async Task<List<ItemProperty>> LoadItemProperties(string hash)
        {
            var uri = new Uri(ServiceContext.BaseMetadataUri, hash);
            var responseString = await HttpClient.GetStringAsync(uri);
            var metadata = JsonConvert.DeserializeObject<NftExternalMetadata>(responseString);
            if (metadata is not null)
            {
                var properties = metadata.GetProperties();
                return properties.Select(p => new ItemProperty
                {
                    name = p.Key,
                    value = p.Value
                }).ToList();
            }

            BeamableLogger.LogWarning("Couldn't load metadata for {hash}", hash);
            return new List<ItemProperty>();
        }
    }
}