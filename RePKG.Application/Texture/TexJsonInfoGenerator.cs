using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using RePKG.Core.Texture;
using System.Reflection;

namespace RePKG.Application.Texture
{
    public class TexJsonInfoGenerator : ITexJsonInfoGenerator
    {
        class IgnorePropertiesResolver : DefaultContractResolver
        {
            private readonly HashSet<string> ignoreProps;
            public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
            {
                this.ignoreProps = new HashSet<string>(propNamesToIgnore);
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                if (this.ignoreProps.Contains(property.PropertyName))
                {
                    property.ShouldSerialize = _ => false;
                }
                return property;
            }
        }

        private IgnorePropertiesResolver _ignorePropertiesResolver;

        public TexJsonInfoGenerator()
        {
            _ignorePropertiesResolver = new IgnorePropertiesResolver(
                new[] { "FirstMipmap", "FirstImage" }
            );
        }

        public string GenerateFullInfo(ITex tex)
        {
            //short helper class to ignore some properties from serialization

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                ContractResolver = _ignorePropertiesResolver
            };
            return JsonConvert.SerializeObject(tex, typeof(Tex), settings);
        }

        public string GenerateInfo(ITex tex)
        {
            if (tex == null) throw new ArgumentNullException(nameof(tex));

            var json = new JObject
            {
                ["bleedtransparentcolors"] = true,
                ["clampuvs"] = tex.HasFlag(TexFlags.ClampUVs),
                ["format"] = tex.Header.Format.ToString().ToLower(),
                ["nomip"] = (tex.FirstImage.Mipmaps.Count == 1).ToString().ToLower(),
                ["nointerpolation"] = tex.HasFlag(TexFlags.NoInterpolation).ToString().ToLower(),
                ["nonpoweroftwo"] = (!NumberIsPowerOfTwo(tex.Header.ImageWidth) ||
                                     !NumberIsPowerOfTwo(tex.Header.ImageHeight)).ToString().ToLower()
            };

            if (tex.IsGif)
            {
                if (tex.FrameInfoContainer == null)
                    throw new InvalidOperationException("TEX is animated but doesn't have frame info container");

                json["spritesheetsequences"] = new JArray
                {
                    new JObject
                    {
                        ["duration"] = 1, // not sure what this value is used for
                        ["frames"] = tex.FrameInfoContainer.Frames.Count,
                        ["width"] = tex.FrameInfoContainer.GifWidth,
                        ["height"] = tex.FrameInfoContainer.GifHeight
                    }
                };
            }

            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }

        private static bool NumberIsPowerOfTwo(int n)
        {
            if (n == 0)
                return false;

            while (n != 1)
            {
                if (n % 2 != 0)
                    return false;

                n /= 2;
            }

            return true;
        }
    }
}