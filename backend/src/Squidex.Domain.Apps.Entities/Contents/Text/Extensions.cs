﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using GeoJSON.Net;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public static class Extensions
    {
        public static Dictionary<string, GeoJSONObject>? ToGeo(this ContentData data, IJsonSerializer jsonSerializer)
        {
            Dictionary<string, GeoJSONObject>? result = null;

            foreach (var (field, value) in data)
            {
                if (value != null)
                {
                    foreach (var (key, jsonValue) in value)
                    {
                        GeoJsonValue.TryParse(jsonValue, jsonSerializer, out var geoJson);

                        if (geoJson != null)
                        {
                            result ??= new Dictionary<string, GeoJSONObject>();
                            result[$"{field}.{key}"] = geoJson;
                        }
                    }
                }
            }

            return result;
        }

        public static Dictionary<string, string>? ToTexts(this ContentData data)
        {
            Dictionary<string, string>? result = null;

            if (data != null)
            {
                var languages = new Dictionary<string, StringBuilder>();
                try
                {
                    foreach (var (_, value) in data)
                    {
                        if (value != null)
                        {
                            foreach (var (key, jsonValue) in value)
                            {
                                AppendJsonText(languages, key, jsonValue);
                            }
                        }
                    }

                    foreach (var (key, sb) in languages)
                    {
                        if (sb.Length > 0)
                        {
                            result ??= new Dictionary<string, string>();
                            result[key] = sb.ToString();
                        }
                    }
                }
                finally
                {
                    foreach (var (_, sb) in languages)
                    {
                        DefaultPools.StringBuilder.Return(sb);
                    }
                }
            }

            return result;
        }

        private static void AppendJsonText(Dictionary<string, StringBuilder> languages, string language, JsonValue value)
        {
            switch (value.Type)
            {
                case JsonValueType.String:
                    AppendText(languages, language, value.AsString);
                    break;
                case JsonValueType.Array:
                    foreach (var item in value.AsArray)
                    {
                        AppendJsonText(languages, language, item);
                    }

                    break;
                case JsonValueType.Object:
                    foreach (var (_, item) in value.AsObject)
                    {
                        AppendJsonText(languages, language, item);
                    }

                    break;
            }
        }

        private static void AppendText(Dictionary<string, StringBuilder> languages, string language, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (!languages.TryGetValue(language, out var sb))
                {
                    sb = DefaultPools.StringBuilder.Get();

                    languages[language] = sb;
                }

                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(text);
            }
        }
    }
}
